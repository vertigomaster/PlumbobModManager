using System;
using System.Collections.Generic;

namespace IDEK.Tools.StateExMachina.Core
{
    public abstract class State 
    {
        /// <summary>
        /// List of actions to perform on cleanup (run after OnExitState())
        /// </summary>
        /// <returns></returns>
        internal List<Action> cleanupActions = new();

        /// <summary>
        /// Ties up any loose ends, like event subscriptions, floating refs, etc
        /// </summary>
        /// <remarks>
        /// Intended to be automatically run as part of the state machine's lifecycle.
        /// </remarks> 
        internal void RunCleanupActions()
        {
            if(cleanupActions == null) return;
            cleanupActions.ForEach(cleanupAction => cleanupAction?.Invoke());
            cleanupActions.Clear();
            cleanupActions = null;
        }
    
        protected void SubscribeForStateLifetime<TAction>( Action<TAction> addListener, Action<TAction> removeListener, TAction callback) 
            where TAction : Delegate
        {
            addListener(callback);
            cleanupActions.Add(() => removeListener(callback));
        }
        
        protected void SubscribeForStateLifetime(IDisposable disposable)
        {
            cleanupActions.Add(disposable.Dispose);
        }
    }

    public abstract class State<TRootState, TContext, TUpdateArg> : State
        where TRootState  : State<TRootState, TContext, TUpdateArg>
        where TContext : new()
    {
        public StateMachine<TRootState, TContext, TUpdateArg> Machine { get; internal set; }
        public string StateLogHeader => $"[*{this.GetType().Name} State*] ";

        /// <summary>
        /// Embeddable alias for AppStateMachine.TryChangeState()
        /// </summary>
        public virtual bool TryChangeState<T>() where T : TRootState, new() => Machine.TryChangeState<T>();

        public virtual bool TryChangeState(TRootState newState) => Machine.TryChangeState(newState);

        /// <summary>
        ///  A simple update function, meant to be called on a regular frequence, such as a tick.
        /// </summary>
        /// <param name="stateContext">State context object, this is often used to pass information between multiple states. </param>
        /// <param name="updateArg">A semi-optional argument, often used for things like passing delta time to a state.</param>
        /// <returns>The state that the system would like to transition to after returnin gfrom update. Return the same value as this class's IMachineState.StateValue to stay in the same state. Should never return null.</returns>
        public virtual TRootState OnUpdate(TContext context, TUpdateArg updateArg) => (TRootState)this;

        /// <summary>
        ///  A simple update function, meant to be called on a regular frequence, such as a tick.
        /// </summary>
        /// <param name="stateContext">State context object, this is often used to pass information between multiple states. </param>
        /// <param name="updateArg">A semi-optional argument, often used for things like passing delta time to a state.</param>
        /// <returns>The state that the system would like to transition to after returnin gfrom update. 
        /// Return the same value as this class's IMachineState.StateValue to stay in the same state.
        /// Should never return null.</returns>
        public virtual TRootState OnFixedUpdate(TContext context, TUpdateArg updateArg) => (TRootState)this;

        /// <summary>
        /// Template that OnEnterState is embedded within. 
        /// Given context, it decides what state it should be in 
        /// (which may be itself or another state to chain into.)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="prevState"></param>
        /// <returns>Destination state that the state decided it should be in, given context. Should never return null.</returns>
        /// <remarks>
        /// This runs underlying logic relating to more complex mechanics, 
        /// like fallthrough (can have parent state classes immediately 
        /// redirect to one of their child states to "containerize" how 
        /// much information is needed to interact with other parts of 
        /// the state machine.). 
        /// The parent state's virtual OnEnterState() will still act as normal, but this fallback approach avoids it being called twice!
        /// </remarks>
        internal virtual TRootState Internal_OnEnterState(TContext context, TRootState prevState)
        {
            TRootState destinationSubstate = GetDesignatedInitialSubstate(context, prevState);

            if(destinationSubstate == this ||
                destinationSubstate == null || 
                destinationSubstate.GetType() == GetType() ||
                !destinationSubstate.GetType().IsAssignableFrom(GetType()))
            {
                return OnEnterState(context, prevState);
            }

            return destinationSubstate;
        }

        /// <summary>
        /// Designated state to drop into instead of executing this one. Intended for use with hierarchical state machines 
        /// to let parent state classes easily redirect to a relevant child state. 
        /// This helps reduce coupling with other parts of the machine's hierarchy 
        /// </summary>
        /// <returns>The recommended state to fallback to (or itself, by default). Should never return null.</returns>
        protected virtual TRootState GetDesignatedInitialSubstate(TContext context, TRootState prevState) => (TRootState)this;

        /// <summary>
        /// An event-style callback that is fired off as this new state begins. 
        /// Offers opprtunity for cleanup and set up of the state and related objects.
        /// </summary>
        /// <param name="stateContext">State context object, this is often used to pass information between multiple states. </param>
        /// <param name="prevState">The state that the machine was previously in before calling this.</param>
        /// <returns>The state that the executing state would like to be in after running through its entry logic.
        ///
        /// By default, (so via base.OnEnterState()) state entry returns the CurrentState of the <see cref="Machine"/>.
        /// If state entry does not explicitly desire a state change by returning some other state, it is suggested to return the default from base.
        /// In that scenario, CurrentState matches the state that was entered. The Machine's intent is to navigate to the returned scene, so returning the same state instance results in expected inaction.
        ///
        /// This approach also implicitly guards against any state changes that occur within the callstack scope of OnEnterState.
        /// Example: an event is fired when a door is closed, and a callback subscribed to that event checks if it needs to lock the door, decides it does,
        /// and kicks off a transition to the locked door state (which recurses its way back here, updating CurrentState to the locked state along the way).
        /// Locked state finishes its OnEnterState and the calls finish until the focus returns to the OnEnterState function of the previous state.
        /// The big thing to remember: while there is only ever one CurrentState, there is not always just one State running within the machine.
        ///
        /// all those implicit "nested" transitions that had time to complete before the originally entered state can manipulate CurrentState,
        /// meaning that it is a more accurate state target than the state which is executing OnEnterStater
        /// Thusly, CurrentState is the ideal value to return, NOT the reflexive state reference.
        ///
        /// Explicitly returning an instance of another state is often helpful for states that want to fire off a one time event and then immediately
        /// transition to another state before processing any updates. Should never return null.
        /// If it does, it should rightfully hit an exception so that we can find and fix it, not silently break things.
        /// </returns>
        protected virtual TRootState OnEnterState(TContext context, TRootState prevState) => Machine?.CurrentState;//?? (TRootState)this;

        /// <summary>
        /// An event-style callback that is fired off as the current state transitions away. Offers opprtunity for cleanup and set up of the state and related objects.
        /// </summary>
        /// <param name="stateContext">State context object, this is often used to pass information between multiple states. </param>
        /// <param name="newState">The new state that the state machine will transition to after this function returns.</param>
        public virtual void OnExitState(TContext context, TRootState newState) { }

        /// <summary>
        /// Runs post-exit cleanup actions that must always run--barring extenuating circumstances (hence why this method is marked virtual)
        /// This includes automatically removing event subscriptions added via <see cref="SubscribeForStateLifetime"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="newState"></param>
        /// <remarks>
        /// Runs after this state's OnExitState() function but before the StateMachine invokes the external OnExitState callback.
        /// If required, this method can be overriden to additional cleanup actions or to overwrite this default cleanup behavior. 
        /// For most use cases, you shouldn't have to override this function.
        /// </remarks>
        public virtual void OnCleanupState(TContext context, TRootState newState) => RunCleanupActions();
    }
}
