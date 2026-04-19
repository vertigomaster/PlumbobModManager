// Created by: Ryan King

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

using IDEK.Tools.ShocktroopUtils;
using IDEK.Tools.ErrorHandling;
#if UNITY_5_3_OR_NEWER
using Object = UnityEngine.Object;
#endif
namespace IDEK.Tools.Logging
{
    public static class ConsoleLog
    {
        public static string ProjectDataRoot =
#if UNITY_5_3_OR_NEWER
            Application.dataPath;
#else
            AppContext.BaseDirectory;
#endif
        
        public const string PACKAGE_DEBUG_WARNING =
            "for temporary debug only; if you are seeing this in a packaged release, please contact the developer!";
        
        private static string logFilePath = ProjectDataRoot + "/Logs/" + 
            DateTime.Today.ToString("MMddyy") + ".log";

        #region ConsoleSettings
#if UNITY_5_3_OR_NEWER
        private static ConsoleSettings _settings;
        public static ConsoleSettings Settings
        {
            get
            {
                if (_settings != null) return _settings;
                else
                {
                    _settings = new ConsoleSettings();
                    return _settings;
                }
            }
        }
        
        public static bool writeToFileLog => ConsoleSettings.fileLog;
#else
        public static bool writeToFileLog = true;
#endif
        #endregion ConsoleSettings

        //-----------Public Functions-----------//

        #region Basic Logging

        /// <inheritdoc cref="LogTemp"/>
        /// <summary>
        /// A "fooble" is a temporary log, prefixed with "fooble - ".
        /// </summary>
        /// <remarks>
        /// Being a nonsensical made-up word not used in really any other context,
        /// searching for "fooble" provides a fairly reliable way to filter out non-temporary logs.
        /// </remarks>
        /// <param name="output">Will be prefixed with "fooble - ".</param>
        public static void Fooble(string output, Object context = null, bool toConsole = false,
            [CallerFilePath] string f = "", [CallerLineNumber] int l = 0, [CallerMemberName] string m = "")
        {
            // "for internal debug only; if you are seeing this in a packaged release, please contact the developer!"
            LogTemp("fooble - " + output, context, toConsole, f, l, m);
        }

        /// <inheritdoc cref="Log"/>
        public static void LogTemp(string output, Object context = null, bool toConsole = false,
            [CallerFilePath] string f = "", [CallerLineNumber] int l = 0, [CallerMemberName] string m = "")
        {
            // "for internal debug only; if you are seeing this in a packaged release, please contact the developer!"
            Log(output + "\n" + PACKAGE_DEBUG_WARNING, context, toConsole, f, l, m);
        }

        /// <summary>
        /// Basic Log() wrapper that a string value as well as the file, line number, 
        /// and function that the call originated from.
        /// </summary>
        /// <param name="output">String value to be logged.</param>
        public static void Log(string output, Object context = null, bool toConsole = false, 
            [CallerFilePath] string f = "", [CallerLineNumber] int l = 0, [CallerMemberName] string m = "")
        {
            // Log(GenerateDebugMsg(output, f, l, m), context);
            string genMsg = GenerateDebugMsg(output, f, l, m);
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.Log(genMsg, context);

            if(DebugConsole.Singleton && (DebugConsole.Singleton.IsUnityLogOutputtingEnabled || toConsole))
            {
                AddToDebugConsole(output, LogType.Log);
            }
#else
            Console.ForegroundColor = ConsoleColor.Cyan;
            Debug.WriteLine("ℹ " + genMsg);
            Console.ResetColor();
#endif
        }

        /// <summary>
        /// Basic LogWarning() wrapper that outputs a string value as well as the file, 
        /// line number, and function that the call originated from.
        /// </summary>
        /// <param name="output">String value to be logged.</param>
        public static void LogWarning(string output, Object context = null, bool toConsole = false, [CallerFilePath] string f = "", 
            [CallerLineNumber] int l = 0, [CallerMemberName] string m = "")
        {
            string genMsg = GenerateDebugMsg(output, f, l, m);
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.LogWarning(genMsg, context);

            if(DebugConsole.Singleton && (DebugConsole.Singleton.IsUnityLogOutputtingEnabled || toConsole))
            {
                AddToDebugConsole(output, LogType.Warning);
            }
#else
            Console.ForegroundColor = ConsoleColor.Yellow;
            Debug.WriteLine("⚠ " + genMsg);
            Console.ResetColor();
#endif
        }

        /// <summary>
        /// Basic LogError() wrapper that outputs a string value as well as the file, 
        /// line number, and function that the call originated from.
        /// </summary>
        /// <param name="output">String value to be logged.</param>
        public static void LogError(string output, Object context=null, bool toConsole = false, [CallerFilePath] string f = "", 
            [CallerLineNumber] int l = 0, [CallerMemberName] string m = "")
        {
            string genMsg = GenerateDebugMsg(output, f, l, m);
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.LogError(genMsg, context);

            if(DebugConsole.Singleton && (DebugConsole.Singleton.IsUnityLogOutputtingEnabled || toConsole))
            {
                AddToDebugConsole(output, LogType.Error);
            }
#else
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("🛑 " + genMsg);
            Console.ResetColor();
#endif
        }

        /// <summary>
        /// Exception wrapper/overload for the basic LogError() wrapper.<br/>
        /// Outputs a string value as well as the file, 
        /// line number, and function that the call originated from.
        /// </summary>
        /// <param name="output">String value to be logged.</param>
        public static void LogError(IDEKException dhException, Object context=null, bool toConsole = false, [CallerFilePath] string f = "", 
            [CallerLineNumber] int l = 0, [CallerMemberName] string m = "")
        {
            string output = dhException.Message;
            LogError(dhException.Message, context, toConsole, f, l, m);
        }

        #endregion Basic Logging

        public static void Assert(bool condition, string output, bool toConsole = false, [CallerFilePath] string f = "",
            [CallerLineNumber] int l = 0, [CallerMemberName] string m = "")
        {
            if(!condition)
            {
                LogError(GenerateDebugMsg(output, f, l, m));
#if UNITY_5_3_OR_NEWER
                if(DebugConsole.Singleton && (DebugConsole.Singleton.IsUnityLogOutputtingEnabled || toConsole))
                {
                    AddToDebugConsole(output, LogType.Assert);
                }
#endif
            }
        }

        public static void AssertWarn(bool condition, string output, bool toConsole = false, [CallerFilePath] string f = "",
            [CallerLineNumber] int l = 0, [CallerMemberName] string m = "")
        {
            if(!condition)
            {
                LogWarning(GenerateDebugMsg(output, f, l, m));
#if UNITY_5_3_OR_NEWER
                if(DebugConsole.Singleton && (DebugConsole.Singleton.IsUnityLogOutputtingEnabled || toConsole))
                {
                    AddToDebugConsole(output, LogType.Warning);
                }
#endif
            }
        }

        #region Log to config file
        /// <summary>
        /// Basic logging plus uses ConsoleSettings.txt to do advanced logging. Currently only supports file logging. If set to true then also saves logs to file at logFilePath.
        /// </summary>
        /// <param name="output">String value to be logged.</param>
        public static void LogWithConfig(string output, [CallerFilePath] string filePath = "", 
            [CallerLineNumber] int lineNum = 0, [CallerMemberName] string methodName = "")
        {
            string logValue = "File - " + GetFileNameFromPath(filePath) + " | Line - " + 
                lineNum + " | Method - " + methodName + " | " + output;

            //TODO: Ryan @Trevor: This makes a link but the link doesn't go to the
            //line it says it should. It goes to this line.
            //Log($"<a href=\"{f}\" line=\"{l}\">{f}:{l}</a>");

            Log("[" + DateTime.Now.ToShortDateString() + " " + 
                DateTime.Now.ToShortTimeString() + "] " + logValue);
            
            //Write to explicit log file
            if (writeToFileLog)
            {
                if (!File.Exists(logFilePath))
                {
                    File.Create(logFilePath);
                }
                File.AppendAllText(logFilePath, "\n" + "[" + DateTime.Now.ToShortDateString() + 
                    " " + DateTime.Now.ToShortTimeString() + "] " + logValue);
            }
        }

        #endregion Log to config file

        #region Log Variable
        //TODO: Ryan: Working on making a log that outputs a variable's name and value.
        //Currently doesn't output the correct variable name.
        public static void LogPlusVariable(object value, [CallerFilePath] string f = "", 
            [CallerLineNumber] int l = 0, [CallerMemberName] string m = "")
        {
            Log(GenerateDebugVariableMsg(value, f, l, m));
            //MemberInfoGetting.GetMemberName(() => value) + ": " + value.ToString());
        }

        #endregion Log Variable

        //-----------Private Functions-----------//

        private static void Log(string msg)
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.Log(msg);
#else
            // System.Diagnostics.Debug.WriteLine(msg);
            Debug.WriteLine(msg);
#endif
        }

        private static string GetFileNameFromPath(string path)
        {
            return path.Substring(path.LastIndexOf('\\') + 1);
        }
        
#if UNITY_5_3_OR_NEWER
        private static void AddToDebugConsole(string outputValue, LogType logType)
        {
            if(RunConsoleInstanceCheck())
                DebugConsole.Singleton.AddOutputEntry(outputValue, logType);
        }

        private static void AddToDebugConsole(string outputValue, Color customLogColor)
        {
            if(RunConsoleInstanceCheck())
                DebugConsole.Singleton.AddOutputEntry(outputValue, customLogColor);
        }

        private static bool RunConsoleInstanceCheck()
        {
            if(DebugConsole.Singleton)
            {
                //avoids it triggering twice since IsUnityLogOutputtingEnabled will mean it's already be running AddOutputEntry().
                if(!DebugConsole.Singleton.IsUnityLogOutputtingEnabled) 
                    return true;
            }
            else
            {
                LogWarning("Add Debug Console prefab to managers.");
            }
            return false;
        }
#endif

        #region Generators

        private static string GenerateDebugMsg(string output, string filePath, 
            int lineNum, string methodName)
        {
            return $"[File: {GetFileNameFromPath(filePath)} | Line: {lineNum} | Method: {methodName}]" +
                $"\n{output}";
        }

        //TODO: Ryan: Working on making a log that outputs a variable's name and value.
        //Currently doesn't output the correct variable name.
        private static string GenerateDebugVariableMsg(object value, string filePath, 
            int lineNum, string methodName)
        {
            return "File: " + GetFileNameFromPath(filePath) + " | Line: " + lineNum + 
                " | Method: " + methodName + " | Variable - " + nameof(value) + " - " + 
                value.ToString() + " | Value - " + value.ToString();
        }

        

        #endregion Generators
    }

}
