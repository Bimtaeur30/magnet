using GGMLib.Anim;
using GGMLib.ModuleSystem;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AgentRenderer : MonoBehaviour, IModule, IAgentRenderer
{
    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Initialize(ModuleOwner owner) { }
    public void PlayAnimation(AnimationParamSO param)
    {
        animator.Play(param.Hash);
    }

    public bool IsAnimationFinished(AnimationParamSO param)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isTargetState = stateInfo.shortNameHash == param.Hash || stateInfo.fullPathHash == param.Hash;

        return isTargetState && !animator.IsInTransition(0) && stateInfo.normalizedTime >= 1f;
    }
}
