namespace GGMLib.FSM
{
    public interface IState<in TContext>
    {
        void Enter(TContext context);
        void Update(TContext context);
        void Exit(TContext context);
    }
}
