using IDEK.Tools.StateExMachina.Core;

namespace IDEK.Tools.StateExMachina.Controllers
{
    public interface IStateMachineController<TStateMachine, TRootState, TContext, TUpdateArg>
        where TStateMachine : StateMachine<TRootState, TContext, TUpdateArg>, new()
        where TRootState : State<TRootState, TContext, TUpdateArg>
        where TContext : new()
    {
        string CurrentStateName { get; }
        bool HasBeenStarted { get; }
        TStateMachine StateMachine { get; }
        TContext Context { get; }

        void DEBUG_LogStateChange(TRootState prevState, TRootState newState);
    }
}