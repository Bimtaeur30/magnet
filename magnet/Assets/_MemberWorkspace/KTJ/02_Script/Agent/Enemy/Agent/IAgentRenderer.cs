using GGMLib.Anim;

public interface IAgentRenderer
{
    public void PlayAnimation(AnimationParamSO param);
    public bool IsAnimationFinished(AnimationParamSO param);
    public void PlayBlink();
    public void PlayDeathEffect();
    public bool IsDeathEffectFinished { get; }
}
