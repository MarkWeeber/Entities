using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class AnimatorBaseControllerComponent : IComponentData
{
    public bool Updated;
    public List<RuntimeAnimatorController> Value;
    public Entity EmptyEntity;
    public AnimatorBaseControllerComponent()
    {

    }
}