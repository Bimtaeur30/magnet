using GGMLib.Anim;
using GGMLib.FSM;

public abstract class EnemyState : IState<EnemyAgent>
{
    private readonly AnimationParamSO _animationParam;

    protected EnemyAgent Agent { get; private set; }

    protected EnemyState(AnimationParamSO animationParam)
    {
        _animationParam = animationParam;
    }

    public void Enter(EnemyAgent context)
    {
        Agent = context;

        if (_animationParam != null)
            Agent.GetModule<IAgentRenderer>().PlayAnimation(_animationParam);

        OnEnter();
    }

    public void Update(EnemyAgent context)
    {
        OnUpdate();
    }

    public void Exit(EnemyAgent context)
    {
        OnExit();
        Agent = null;
    }

    protected virtual void OnEnter() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnExit() { }
}
