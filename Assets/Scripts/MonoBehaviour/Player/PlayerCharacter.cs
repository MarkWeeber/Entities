using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public Animator Animator { get => animator; }
    private static PlayerCharacter instance;
    public static PlayerCharacter Instance { get => instance; }
    

    public void Awake()
    {
        instance = this;
    }
}
