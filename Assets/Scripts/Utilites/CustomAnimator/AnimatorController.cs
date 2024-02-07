using UnityEngine;

public class AnimatorController
{
    [SerializeField] private Animator Animator;

    private void Execute()
    {
        var x = Animator.parameters;
        var stateInfo = Animator.GetCurrentAnimatorStateInfo(1);
        var stateMachineBehaviour = ScriptableObject.CreateInstance<StateMachineBehaviour>();
        var behaviours = Animator.GetBehaviours<StateMachineBehaviour>();
        foreach (var item in behaviours)
        {
            
        }
    }
}


