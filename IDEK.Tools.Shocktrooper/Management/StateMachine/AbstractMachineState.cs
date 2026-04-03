//Created By: Ray Batts 
//Edited By: Julian Noel (janoel777@gmail.com)

using System;

namespace IDEK.Tools.Management.StateMachine
{
    [Obsolete("Use RootMachineState instead. It no longer requires superflous enum usage and involves less reflection.")]
    /// <summary>
    /// Non-generic base class to preserve sanity.
    /// </summary>
    public abstract class AbstractMachineState { }

    [Obsolete("Use RootMachineState<TContextType, TUpdateArgType> instead. It no longer requires superflous enum usage and involves less reflection.")]
    /// <summary>
    /// The template for a singular state in a BaseStateMachine
    /// </summary>
    /// <typeparam name="TStateEnumType"></typeparam>
    /// <typeparam name="TContextType"></typeparam>
    /// <typeparam name="TUpdateArgType"></typeparam>
    public abstract class AbstractMachineState<TStateEnumType, TContextType, TUpdateArgType> : AbstractMachineState
        where TStateEnumType : Enum
        where TContextType : new()
    {
        public abstract TStateEnumType StateValue { get; }
        public BaseStateMachine<TStateEnumType, TContextType, TUpdateArgType> Machine { get; internal set; }
        public string StateLogHeader => $"[*{StateValue} State*] ";

        /// <summary>
        /// Embeddable alias for AppStateMachine.TryChangeState()
        /// </summary>
        public virtual bool TryChangeState(TStateEnumType desiredState) => Machine.TryChangeState(desiredState);

        /// <summary>
        ///  A simple update function, meant to be called on a regular frequence, such as a tick.
        /// </summary>
        /// <param name="stateContext">State context object, this is often used to pass information between multiple states. </param>
        /// <param name="updateArg">A semi-optional argument, often used for things like passing delta time to a state.</param>
        /// <returns>The state that the system would like to transition to after returnin gfrom update. Return the same value as this class's IMachineState.StateValue to stay in the same state.</returns>
        public virtual TStateEnumType OnUpdate(TContextType stateContext, TUpdateArgType updateArg) => StateValue;

        /// <summary>
        /// An event-style callback that is fired off as a new state begins. Offers opprtunity for cleanup and set up of the state and related objects.
        /// </summary>
        /// <param name="stateContext">State context object, this is often used to pass information between multiple states. </param>
        /// <param name="prevState">The state that the machine was previously in before calling this.</param>
        /// <returns>The state that the system would like to transition to after returnin gfrom update. Return the same value as this class's IMachineState.StateValue to stay in the same state. This is often helpful
        /// for states that want to fire off a one time event and then immediately transition to another state before processing any updates.</returns>
        public virtual TStateEnumType OnEnterState(TContextType stateContext, TStateEnumType prevState) => StateValue;

        /// <summary>
        /// An event-style callback that is fired off as the current state transitions away. Offers opprtunity for cleanup and set up of the state and related objects.
        /// </summary>
        /// <param name="stateContext">State context object, this is often used to pass information between multiple states. </param>
        /// <param name="newState">The new state that the state machine will transition to after this function returns.</param>
        public virtual void OnExitState(TContextType stateContext, TStateEnumType newState) { }
    }
}