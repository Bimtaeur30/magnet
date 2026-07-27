using GGMLib.Anim;

public interface IAgentRenderer
{
    public void PlayAnimation(AnimationParamSO param);
    public bool IsAnimationFinished(AnimationParamSO param);
}
