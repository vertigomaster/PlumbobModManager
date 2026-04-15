using System.Diagnostics;

namespace IDEK.Tools.Trove;

/// <summary>
/// Container for storing objects with finite lifecycles determined by external systems. When a Trove is disposed, all of its elements are disposed as well, ensuring all items are cleaned up. These elements can be other Troves as well.
/// </summary>
/// <remarks>
/// How to use: add an IDisposable element to the Trove, this can be a wrapper for an event, a service that needs to run cleanup code, anything else. The main benefit over using a deconstructor is that it occurs on a thread used by your application instead of the other specialized thread(s) used by deconstructors. That and being able to dispose anything, not just a (de)constructable class.
/// </remarks>
public class Trove(string name) : IDisposable
{
    public string Name { get; } = name;
    
    /// <summary>
    /// A static instance of an empty Trove with a unique identifier.
    /// This instance contains no elements and serves as a convenient placeholder or default value.
    /// </summary>
    public static Trove Empty => new Trove(UniqueTag);

    public static string UniqueTag => Guid.NewGuid().ToString("N"); 

    // public string Name { get; init; }
    private Dictionary<string, IDisposable> _items = [];

    public IEnumerable<IDisposable> Items => _items.Values;

    public int Count => _items.Count;

    /// <summary>
    /// How many elements are in this Trove, including troves nested within it.
    /// Watch out for cyclical-nesting infinite loops!
    /// </summary>
    public int RecursiveCount => _GetRecursiveCount(0);

    private const int MAX_RECURSIVE_DEPTH = 50;

    // public Trove() : this()
    // { }

    public static Trove Create(string name) => new Trove(name);
    public static Trove CreateOuterTrove(Trove innerTrove)
    {
         return Trove
             .Create("\"" + innerTrove.Name + "\" wrapper")
             .AddCleanup(innerTrove);
    }
    
    // /// <summary>
    // /// Creates a new Trove and adds the given disposables to it
    // /// </summary>
    // public Trove(IEnumerable<IDisposable> disposableTroveItems)
    // {
    //     AddRange(disposableTroveItems);
    // }
    private int _GetRecursiveCount(int currentDepth)
    {
        if (currentDepth > MAX_RECURSIVE_DEPTH)
        {
            throw new Exception(
                $"Trove has reached the maximum recursive depth of " +
                $"{MAX_RECURSIVE_DEPTH}. Please check for cyclical nesting " +
                $"or rethink your design.");
        }

        int recursiveCount = 0;
        foreach (KeyValuePair<string, IDisposable> item in _items)
        {
            if (item.Value is Trove trove)
            {
                recursiveCount += trove.RecursiveCount;
            }
        }

        return recursiveCount + Count;
    }

    //TODO; hash the tag strings into a cheaper int
    

    
    /// <summary>
    /// Returns whether the given disposable was found within this trove
    /// </summary>
    /// <param name="disposableTroveItem"></param>
    /// <returns></returns>
    public bool Contains(IDisposable disposableTroveItem) => 
        _items.Values.Contains(disposableTroveItem);

    /// <summary>
    /// Determines whether the Trove contains an element with the specified tag.
    /// </summary>
    /// <param name="tag">The tag associated with the element to locate in the Trove.</param>
    /// <returns>True if the Trove contains an element with the specified tag; otherwise, false.</returns>
    public bool Contains(string tag) => _items.ContainsKey(tag);

    // /// <summary>
    // /// Add multiple new elements which will be disposed when the Trove is
    // /// </summary>
    // /// <param name="disposableTroveItem"></param>
    // /// <returns></returns>
    // public Trove AddRange(IEnumerable<IDisposable> disposableTroveItems)
    // {   
    //     foreach (var item in disposableTroveItems)
    //     {
    //         Add(item);
    //     }
    //     return this;
    // }

    /// <summary>
    /// Removes an element from the Trove (without disposing it)
    /// </summary>
    /// <param name="disposableTroveItem"></param>
    /// <returns></returns>
    public Trove Remove(string tag)
    {
        _items.Remove(tag);
        return this;
    }
    
    public void Dispose()
    {
        foreach (KeyValuePair<string, IDisposable> item in _items)
        {
            //TODO: enable debug logging
            Debug.WriteLine($"Disposing {item.Key}");
            item.Value.Dispose();
        }
    }
    
    #region Helper Functions

    /// <summary>
    /// Adds a new element which will be disposed when the Trove is
    /// </summary>
    /// <param name="disposableTroveItem"></param>
    /// <returns></returns>
    /// <remarks>We recommend giving your elements explicit names to avoid accidentally re-adding them semantically</remarks>
    public Trove AddCleanup(string tag, IDisposable disposableTroveItem)
    {
        Debug.WriteLine($"fooble [Trove] Adding key {tag} to {Name}"); 
        if (!_items.TryAdd(tag, disposableTroveItem))
        {
            Console.WriteLine($"[Trove] Warning - Key {tag} already exists in {Name}, ignoring.");
        }

        return this;
    }

    public Trove AddCleanup(string tag, Action onDispose) => 
        AddCleanup(tag, new DisposableAction(onDispose));

    public Trove AddCleanup(Action onDispose) => 
        AddCleanup(UniqueTag, new DisposableAction(onDispose));

    public Trove AddCleanup(Trove innerTrove) => AddCleanup(innerTrove.Name, innerTrove);
    
    #endregion
}

/// <summary>
/// Wraps a given Action within an IDisposable
/// </summary>
/// <param name="onDispose"></param>
public class DisposableAction(Action onDispose) : IDisposable
{
    private Action _onDispose = onDispose;
    
    /// <summary>
    /// Runs the action it was given on Dispose
    /// </summary>
    public void Dispose()
    {
        
        var toDispose = Interlocked.Exchange(ref _onDispose, null);

        toDispose?.Invoke();
    }
}