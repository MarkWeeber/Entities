using UnityEngine;

public class AnimatorController
{
    [SerializeField] private Animator Animator;

    private void Execute()
    {
        var x = Animator.parameters;
        var runtimeAnimator = Animator.runtimeAnimatorController;
        var anims = runtimeAnimator.animationClips;
        var stateInfo = Animator.GetCurrentAnimatorStateInfo(1);
        var stateMachineBehaviour = ScriptableObject.CreateInstance<StateMachineBehaviour>();
        var behaviours = Animator.GetBehaviours<StateMachineBehaviour>();
        foreach (var item in behaviours)
        {
            
        }
        foreach (var animationClip in anims)
        {
            
        }
    }
}


