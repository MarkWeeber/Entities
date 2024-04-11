using UnityEngine;

[CreateAssetMenu(fileName = "New Player Config Asset", menuName = "Custom Assets/Player Config")]
public class PlayerConfig : ScriptableObject, IPlayerConfig
{
    [SerializeField]
    private float moveSpeed;
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    [SerializeField]
    private float turnSpeed;
    public float TurnSpeed { get => turnSpeed; set => turnSpeed = value; }
}
