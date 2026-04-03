//Created By: Julian Noel (janoel777@gmail.com)
//Inspired By: Ray Batt's previous StateMachine framework

using IDEK.Tools.Logging;
using System.Diagnostics;

namespace IDEK.Tools.StateExMachina.Core
{
    /// <summary>
    /// Non-generic base class to preserve sanity.
    /// </summary>
    public abstract class StateMachine { }

    public class StateMachine<TRootState, TContext, TUpdateArg> : StateMachine 
        where TRootState : State<TRootState, TContext, TUpdateArg> 
        where TContext : new()
    {
        public delegate void StateChangedHandler(TRootState prevState, TRootState newState);
        public event StateChangedHandler OnPreStateChange = (a, b) => { };
        public event StateChangedHandler OnEnterState = (a, b) => { };
        public event StateChangedHandler OnExitState = (a, b) => { };
        public event StateChangedHandler OnPostStateChange = (a, b) => { };

        public TRootState CurrentState { get; private set; }

        public TContext context;

        private readonly Dictionary<Type, TRootState> _stateCache = new();

        public bool debuggingEnabled = false;

        public StateMachine()
        {
            ProvideContext(default);
        }

        public StateMachine(TContext contextToUse) 
        { 
            ProvideContext(contextToUse);
        }

        public StateMachine(TRootState initState, TContext contextToUse)
        {
            ProvideContext(contextToUse);

            TryChangeState(initState);
        }

        //Factory, because generics
        public static StateMachine<TRootState, TContext, TUpdateArg> CreateMachineIn<TInitState>() where TInitState : TRootState, new()
        {
            var newMachine = new StateMachine<TRootState,TContext, TUpdateArg>();
            newMachine.ResetAt<TInitState>();
            return newMachine;
        }

        /// <summary>
        /// Enables providing the state machine with a given Context object to work from. 
        /// <para/>
        /// If one isn't supplied, a blank/default one will be created for the machine.
        /// </summary>
        /// <param name="newContext">The new context to use. If null, a new instance of the context type will be used. 
        /// <para/>
        /// Would recommend using a reference type as opposed 
        /// to a value type so that you can still externally view/reference it as needed.</param>
        public virtual void ProvideContext(TContext newContext)
        {
            if (newContext == null) ConsoleLog.LogWarning($"No given {nameof(TContext)} context provided to {this.GetType().Name}. Using default context.");
            context = newContext ?? new();
        }

        /// <summary>
        /// Attempts to transition between the current and desired states.
        /// </summary>
        /// <param name="desiredState">The state to which the caller would like to transition to.</param>
        /// <returns>True if the state change was successful and a different state was set.</returns>
        /// <remarks>This will call several events in a row.
        /// First, the OnPreStateChange. Just in case any external subscriber wants to do some kind of cleanup before any transitions.
        /// Second, the IMachineState.OnExitState for the current state will be called.
        /// Third, we will switch states and then call IMachineState.OnEnterState for the new state
        /// Lastly we will call a final OnPostStateChange(), in case external subscribers want to do any general cleanup.</remarks>
        public virtual bool TryChangeState<TState>() where TState : TRootState, new()
        {
            if(CurrentState is TState)
            {
                return false; //already in state
            }

            return TryChangeState(new TState());
        }

        public virtual bool TryChangeState(TRootState newState)
        {
            newState.Machine = this;

            if (newState == CurrentState) return false; //this is the exact state already
            if (newState == null) return false;

            TRootState oldState = CurrentState;

            if (debuggingEnabled)
                Debug.WriteLine($"State Machine {this.GetType()} is transitioning from {oldState} to {newState}.");

            OnPreStateChange(oldState, newState);

            oldState?.OnExitState(context, newState);
            //state cleanup must happen before anything external can react to the state 
            //being existed. This prevents a lot of accidental data/state corruption with 
            //things like event callbacks still having an opportunity to be invoked after 
            //they are generally expected to have been cleaned up/removed.
            oldState?.OnCleanupState(context, newState); 
            
            OnExitState(oldState, newState);

            CurrentState = newState;
            
            TRootState newDesiredState = newState.Internal_OnEnterState(context, oldState);

            OnEnterState(oldState, newState);

            // CurrentState = newState;

            OnPostStateChange(oldState, newState);

            //staying in same state?
            if (newDesiredState.Equals(newState) || newDesiredState.Equals(CurrentState))
            {
                return true;
            }

            //Moving to new state type?
            if (!newDesiredState.GetType().Equals(newState.GetType()))
            {
                return TryChangeState(newDesiredState);
            }
            
            //if state type matches but it's a diff instance, ERROR. 
            //this means we were incorrectly given a new instance of the same state. 
            //This is not supported.

            //TODO: contemplating throwing an InvalidArgumentException for this one, honestly
            //Or maybe an InvalidOperationException?
            ConsoleLog.LogError("Unsupported operation: OnEnterState must either return the exact instance "
                + "it was given or return a different compile-time type. Transition denied to avoid infinite loop.");
            ConsoleLog.LogError($"newState = {newState}, newDesiredState = {newDesiredState}, oldState = {oldState}");

            return false;
        }

        public virtual void ResetAt<TInitState>() where TInitState : TRootState, new()
        {
            OnPreStateChange = (a, b) => { };
            OnEnterState = (a, b) => { };
            OnExitState = (a, b) => { };
            OnPostStateChange = (a, b) => { };

            TryChangeState<TInitState>();
        }

        public virtual void Update(TUpdateArg tick = default(TUpdateArg))
        {
            // Update the current state and transition to a new state if it has been requested.
            TryChangeState(CurrentState?.OnUpdate(context, tick) as TRootState);
        }

        public virtual void FixedUpdate(TUpdateArg fixedTick = default(TUpdateArg))
        {
            TryChangeState(CurrentState?.OnFixedUpdate(context, fixedTick) as TRootState);
        }
    }
}
