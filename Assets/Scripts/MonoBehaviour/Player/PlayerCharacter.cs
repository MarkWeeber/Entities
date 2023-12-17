using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : SingletonBehaviour<PlayerCharacter>
{
    [SerializeField] private Animator animator;
    public Animator Animator { get => animator; }
}
