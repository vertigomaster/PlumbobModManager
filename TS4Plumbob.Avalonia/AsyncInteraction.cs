using System;
using System.Threading.Tasks;

namespace TS4Plumbob.Avalonia;

/// <summary>
/// Provides a way for the ViewModel to request an asynchronous action from the View (UI)
/// without the ViewModel having a direct reference to the View.
/// This is commonly used for dialogs, file pickers, or popups in MVVM.
/// </summary>
/// <typeparam name="TInput">The type of input parameter passed to the handler.</typeparam>
/// <typeparam name="TOutput">The type of output result returned by the handler.</typeparam>
public class AsyncInteraction<TInput, TOutput>
{
    private Func<TInput, Task<TOutput>>? _handler;

    /// <summary>
    /// Registers a handler for the interaction. This is typically called by the View.
    /// </summary>
    /// <param name="handler">The asynchronous function that performs the action.</param>
    public void RegisterHandler(Func<TInput, Task<TOutput>> handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// Removes the current handler. Should be called when the View is unloaded to prevent memory leaks.
    /// </summary>
    public void UnregisterHandler()
    {
        _handler = null;
    }

    /// <summary>
    /// Triggers the interaction and waits for the handler to return a result.
    /// </summary>
    /// <param name="input">The input data for the interaction.</param>
    /// <returns>The result returned by the UI handler.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no handler is registered when called.</exception>
    public async Task<TOutput> Handle(TInput input)
    {
        if (_handler == null)
            throw new InvalidOperationException("Interaction handler not registered.");

        return await _handler(input);
    }
}
