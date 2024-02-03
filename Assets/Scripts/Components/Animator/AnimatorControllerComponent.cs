using System.Collections.Generic;
using Unity.Entities;
using UnityEditor.Animations;

public class AnimatorControllerComponent : IComponentData, IEnableableComponent
{
    public List<AnimatorController> Value;
    public Entity EmptyEntity;
}