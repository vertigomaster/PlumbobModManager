using System.CommandLine;
using System.Reflection;
using System.Runtime.ExceptionServices;
using IDEK.Tools.ShocktroopExtensions;
using IDEK.Tools.ShocktroopUtils.CILAnalysis;

namespace IDEK.Tools.ConsoleCommander;

public static class CommandRegistry
{
    public static RootCommand Build(Assembly asm)
    {
        //starting point; will declaratively build tree from here
        var root = new RootCommand();
        
        //grab and flatten all methods in the given assembly which have a CommandAttribute
        var methods = asm.GetTypes()
            .SelectMany(t => 
                t.GetMethods(
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
            .Select(m => new { //anon entry type for method and attribute data
                Method = m, 
                CmdAttributes = m.GetCustomAttributes<CommandAttribute>()
            })
            .Where(x => x.CmdAttributes.Any());

        //register the method with the root command, declaratively ensuring the command hierarchy is established
        //we already null checked the attribute
        foreach (var entry in methods)
            foreach(var attr in entry.CmdAttributes)
                _RegisterMethod(root, entry.Method, attr!);

        return root;
    }

    private static void _RegisterMethod(RootCommand root, MethodInfo method, CommandAttribute attr)
    {
        //use the command attribute metadata to ensure build the command tree has the required structure
        Command command = _GetOrAddMatchingCommand(root, attr);

        //we have now walked the entire command tree to the desired location, creating segments as needed (like mkdir -p).
        
        command.Description = attr.Description;
        
        //sets up the parameters for the method; its a bit involved.
        _BuildParameters(command, method); 
    }

    private static Command _GetOrAddMatchingCommand(RootCommand root, CommandAttribute attr)
    {
        Command current = root; //we're going to walk the tree, laying bricks as needed

        foreach (var segment in attr.Path)
        {
            //our next step
            
            //try to find the next segment in the tree
            var existing = current.Subcommands.FirstOrDefault(c => c.Name == segment);
            if (existing == null)
            {
                //if next segment not found, create it and attach it to the current segment.
                existing = new Command(segment);
                current.Add(existing);
            }
            
            current = existing; //move to next segment
        }

        return current;
    }

    private record BindableCommandSymbol(string Name, Symbol Symbol);

    /// <summary>
    /// Builds out the parameters for the given method--both options and arguments--based on the method signature.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="method"></param>
    private static void _BuildParameters(Command command, MethodInfo method)
    {
        var srcMethodParameters = method.GetParameters();
        var commandParamsToBind = new List<BindableCommandSymbol>();
        //tracks which letters have already been used/reserved to
        //avoid multiple options that start with the same letter(s) from conflicting.
        var reservedFlagLetters = new HashSet<char>();
        
        foreach (var p in srcMethodParameters)
        {
            if(p.Name == null)
                throw new ArgumentNullException(nameof(p.Name), 
                    "Anonymous/nameless Parameters are not support (ParameterInfo.Name cannot be null).");
            
            //parse result is ignored
            if(p.ParameterType == typeof(ParseResult)) continue;
            
            //parameters with a default value are options; those without it are not.
            if(p.HasDefaultValue)
            {
                Option opt = _BuildOption(p, reservedFlagLetters);
                command.Add(opt);//registers option to the System.CommandLine.Command
                commandParamsToBind.Add(new(p.Name, opt)); //prepares the list of parameters to bind to the method (relayed from the Command)
            }
            else
            {
                Argument arg = _BuildArgument(p);
                command.Add(arg);
                commandParamsToBind.Add(new(p.Name, arg));
            }
        }
        
        _BindHandler(command, method, commandParamsToBind);
    }

    /// <summary>
    /// Builds an <see cref="Argument"/> for the given parameter.
    /// </summary>
    /// <param name="parameterInfo"></param>
    /// <returns></returns>
    private static Argument _BuildArgument(ParameterInfo parameterInfo)
    {
        var paramType = parameterInfo.ParameterType;
        var argumentType = typeof(Argument<>).MakeGenericType(paramType);
        
        return Activator.CreateInstance(argumentType, parameterInfo.Name) as Argument ?? 
            throw new InvalidOperationException($"Failed to create an " +
                $"Argument<{paramType.Name}> for parameter '{parameterInfo.Name}'");
    }

    /// <summary>
    /// Builds an <see cref="Option"/> for the given parameter.
    /// </summary>
    /// <param name="parameterInfo"></param>
    /// <param name="reservedFlagLetters">Set of reserved flag letters. Used for aliases.</param>
    /// <returns></returns>
    private static Option _BuildOption(ParameterInfo parameterInfo, HashSet<char> reservedFlagLetters)
    {
        var paramType = parameterInfo.ParameterType;
        var optionType = typeof(Option<>).MakeGenericType(paramType);
        
        //handle aliases
        //the name should be non-null; or at least that should already be checked externally. 
        string[] aliases = _BuildAliases(parameterInfo.Name!, reservedFlagLetters);
        
        //mimics new Option<T>(string name, params string[] aliases)
        return Activator.CreateInstance(optionType, aliases) as Option ?? 
            throw new InvalidOperationException($"Activator failed to create an Option<{paramType.Name}> " +
                $"with parameter name '{parameterInfo.Name}' and aliases '{string.Join(", ", aliases)}'");
    }

    private static string[] _BuildAliases(string parameterInfoName, HashSet<char> reservedFlagLetters)
    {
        List<string> aliases = [$"--{parameterInfoName.ToKebabCase()}"];
        
        char singleLetterAlias = default;
        for(int i = 0; i < parameterInfoName.Length; i++)
        {
            var testChar = char.ToLowerInvariant(parameterInfoName[i]);
            //try to add it - that only succeeds if it's not already in the set.
            if(!reservedFlagLetters.Add(testChar)) continue;
            singleLetterAlias = testChar;
            break;
        }

        //try march through all letters
        if (singleLetterAlias == default)
        {
            //if that somehow failed, count to the next letter char that is open
            //not all systems are case-sensitive, so a-z is safest.
            //Honestly, if you have more than 26 parameters,
            //you really need to switch to a config file or something anyway.
            for (char c = 'a'; c <= 'z'; c++) 
            {
                if (!reservedFlagLetters.Add(c)) continue;
                singleLetterAlias = c;
                break;
            }
        }

        if (singleLetterAlias != default)
            aliases.Add($"-{singleLetterAlias}");
        
        return aliases.ToArray();
    }

    private static void _BindHandler(
        Command command,
        MethodInfo method,
        List<BindableCommandSymbol> commandParamsToBind)
    {
        command.SetAction(parseResult =>
        {
            List<object?> finalOrderedArgsList = _BuildCommandActionOrderedArgsList(
                command, method, commandParamsToBind, parseResult);

            //static, so null object
            method.Invoke(null, finalOrderedArgsList.ToArray());
        });
    }

    private static List<object?> _BuildCommandActionOrderedArgsList(
        Command command,
        MethodInfo method,
        List<BindableCommandSymbol> commandParamsToBind,
        ParseResult parseResult)
    {
        List<object?> finalOrderedArgsList = new List<object?>(commandParamsToBind.Count);

        //going in method param order to ensure the correct order of parameters.
        foreach (ParameterInfo param in method.GetParameters())
        {
            if (param.ParameterType == typeof(ParseResult))
            {
                finalOrderedArgsList.Add(parseResult);
                continue;   
            }

            Symbol matchingCommandParam = commandParamsToBind
                .FirstOrDefault(bindable => bindable.Name == param.Name)
                ?.Symbol ?? throw new InvalidOperationException(
                $"Could not find a parameter with name '{param.Name}' in the command. " +
                $"There is likely a mismatch between the command '{command.Name}' and " +
                $"the method signature '{method.Name}({method.ParamsToString()})'.");
                
            //example: Option<bool>, Argument<string> - the runtime type
            Type symbolType = matchingCommandParam.GetType();
                
            //have to do a search for first due to method overloads
            //Problem: there are two generic ParseResult.GetValue<T>() overloads -
            //one for Option<T> and one for Argument<T> 
            var getValueMethod = typeof(ParseResult)
                .GetMethods()
                .FirstOrDefault(m => {
                        if (m.Name != nameof(ParseResult.GetValue)) return false; 
                        var parameters = m.GetParameters();
                        if(parameters.Length != 1) return false;
                        if(!parameters[0].ParameterType.IsGenericType) return false;
                        //make sure the types of the method's arg and our symbol
                        //come from the same generic type def (i.e. Option<> vs Argument<>)
                        return parameters[0].ParameterType.GetGenericTypeDefinition() ==
                            symbolType.GetGenericTypeDefinition();
                    }
                ) ?? throw new InvalidOperationException(
                    $"Could not find generic method " +
                    $"'{nameof(ParseResult.GetValue)}<{typeof(ParseResult).Name}>()'. " +
                    $"Did the API change?");
            
            //dynamically build and evaluate parseResult.GetValue<ParameterType>(matchingCommandParam)
            var argValue = getValueMethod
                .MakeGenericMethod(param.ParameterType)
                .Invoke(parseResult, [matchingCommandParam]);
                
            finalOrderedArgsList.Add(argValue);
        }

        return finalOrderedArgsList;
    }
}