//Created By: Ray Batts (Presumably)
//Edited By: Julian Noel (janoel777@gmail.com)

using IDEK.Tools.ShocktroopUtils;
using IDEK.Tools.Logging;

#if ODIN_INSPECTOR
using Sirenix.Utilities;
#else 
using IDEK.Tools.ShocktroopUtils.OdinFallbacks;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace IDEK.Tools.Management.StateMachine
{
    [Obsolete("Use StateMachine instead. It no longer requires superflous enum usage and involves less reflection.")]
    /// <summary>
    /// Non-generic base class to preserve sanity.
    /// </summary>
    public abstract class BaseStateMachine 
    { 
        public abstract bool TryChangeState<TStateEnumType>(TStateEnumType stateType) where TStateEnumType : Enum;
    }

    [Obsolete("Use StateMachine<TRootState, TContextType, TUpdateArgType> instead. It no longer requires superflous enum usage and involves less reflection.")]
    public class BaseStateMachine<TStateEnumType, TContextType, TUpdateArgType> : BaseStateMachine
        where TStateEnumType : Enum
        where TContextType : new()
    {
        public delegate void StateChangedHandler(TStateEnumType prevState, TStateEnumType newState);
        public event StateChangedHandler OnPreStateChange = (a, b) => { };
        public event StateChangedHandler OnEnterState = (a, b) => { };
        public event StateChangedHandler OnExitState = (a, b) => { };
        public event StateChangedHandler OnPostStateChange = (a, b) => { };

        public TStateEnumType CurrentState { get; private set; }

        public TContextType ContextObject = new TContextType();

        private readonly Dictionary<TStateEnumType, AbstractMachineState<TStateEnumType, TContextType, TUpdateArgType>> _states = new();

        private static IEnumerable<Type> allDerivedStateTypes;

        public BaseStateMachine(TStateEnumType uninitializedState)
        {
            CurrentState = uninitializedState;
            // RBTODO: Put this in a factory so that we don't have to eat this reflection cost every initialization.

            //only build state type table for this type if one has not yet been built
            if(allDerivedStateTypes == null) 
            {
                // Get all state classes that derive from the same IMachineState that we were declared with.
                Type[] typesInAssembly = Assembly.GetAssembly(this.GetType()).GetTypes();
                //UnityEngine.Debug.Log($"stuff in assembly \"{Assembly.GetAssembly(this.GetType()).FullName}\" (count {typesInAssembly.Length}): \n" +
                //    $"{string.Join<Type>(",\n", typesInAssembly)}");

                allDerivedStateTypes = typesInAssembly.Where(
                    //t => t.IsInstanceOfType AbstractMachineState<TStateEnumType, TContextType, TUpdateArgType>); //"is" keyword also works for derived types
                    t => t.IsClass && !t.IsAbstract && !t.IsStatic() && t.GetBaseClasses().Contains(typeof(AbstractMachineState<TStateEnumType, TContextType, TUpdateArgType>)));

                //UnityEngine.Debug.Log($"stuff in assembly \"{nameof(allDerivedStateTypes)}\" (count {allDerivedStateTypes.Count()}): \n" +
                //    $"{string.Join<Type>(",\n", allDerivedStateTypes)}");
            }

            //Type baseStateType = typeof(AbstractMachineState<TStateType, TContextType, TUpdateArgType>);
            //IEnumerable<Type> allDerivedStateTypes = Assembly.GetAssembly(this.GetType()).GetTypes().Where(t => 
            //    t.BaseType != null && 
            //    t.BaseType.IsGenericType && 
            //    t.BaseType == baseStateType);

            foreach(Type type in allDerivedStateTypes)
            //foreach(Type type in typesInAssembly)
            {
                var newState = (AbstractMachineState<TStateEnumType, TContextType, TUpdateArgType>)Activator.CreateInstance(type);
                newState.Machine = this;
                _states[newState.StateValue] = newState;
            }

#if DEBUG
            foreach(TStateEnumType enumValue in Enum.GetValues(typeof(TStateEnumType)))
            {
                ConsoleLog.Assert(_states.ContainsKey(enumValue), $"{this.GetType().Name} failed to find type for {enumValue.GetType().Name}.{enumValue.ToString()}. See _states readout below:" +
                    $"\n {string.Join(",", _states)}, length = {_states.Count}");
            }

#endif // DEBUG
        }

        // public override bool TryChangeState_Internal<TStateEnumType>(TStateEnumType desiredState) => TryChangeState(desiredState);

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
        public bool TryChangeState(TStateEnumType desiredState)
        {
            bool returnValue = false;
            if(desiredState.Equals(CurrentState))
            {
                return returnValue;
            }

            AbstractMachineState<TStateEnumType, TContextType, TUpdateArgType> oldState = _states[CurrentState];
            AbstractMachineState<TStateEnumType, TContextType, TUpdateArgType> newState = _states[desiredState];

            Debug.WriteLine($"State Machine {this.GetType()} is transitioning from {oldState} to {newState}.");

            OnPreStateChange(oldState.StateValue, newState.StateValue);

            oldState.OnExitState(ContextObject, newState.StateValue);
            OnExitState(oldState.StateValue, newState.StateValue);

            CurrentState = desiredState;

            TStateEnumType newDesiredState = newState.OnEnterState(ContextObject, oldState.StateValue);
            OnEnterState(oldState.StateValue, newState.StateValue);

            CurrentState = desiredState;

            OnPostStateChange(oldState.StateValue, newState.StateValue);

            returnValue = newDesiredState.Equals(CurrentState) ? true : TryChangeState(newDesiredState);

            return returnValue;
        }

        public void Reset(bool fireBaseOnDefault = true, TStateEnumType desiredState = default(TStateEnumType))
        {
            OnPreStateChange = (a, b) => { };
            OnEnterState = (a, b) => { };
            OnExitState = (a, b) => { };
            OnPostStateChange = (a, b) => { };

            CurrentState = desiredState;

            if(fireBaseOnDefault)
            {
                _states[CurrentState].OnEnterState(ContextObject, CurrentState);
            }
        }

        public void Update(TUpdateArgType updateArg = default(TUpdateArgType))
        {
            // Update the current state and transition to a new state if it has been requested.
            TryChangeState(_states[CurrentState].OnUpdate(ContextObject, updateArg));
        }

        public override bool TryChangeState<TOuterEnumType>(TOuterEnumType stateType) => TryChangeState((TStateEnumType)(System.Enum)stateType);
    }
}
