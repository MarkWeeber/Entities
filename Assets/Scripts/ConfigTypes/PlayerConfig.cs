using UnityEngine;

[CreateAssetMenu(fileName = "New Player Config Asset", menuName = "Custom Assets/Player Config")]
public class PlayerConfig : ScriptableObject
{
    public float MoveSpeed = 1.0f;
    public float TurnSpeed = 5.0f;
    public float SpeedMultiplier = 1.7f;
    public float SprintTime = 1f;
}