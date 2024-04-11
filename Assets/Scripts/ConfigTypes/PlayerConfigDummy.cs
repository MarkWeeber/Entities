using System;

public class PlayerConfigDummy : IPlayerConfig
{
    private float moveSpeed = 2.5f;
    private float turnSpeed = 10f;
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public float TurnSpeed { get => turnSpeed; set => turnSpeed = value; }
}
