using Fusion;
using UnityEngine;

public enum InputButtons
{
    Jump,
    Fire,

    Heal,

    Stealth
}
public struct NetworkInputData : INetworkInput
{
    public Vector2 Direction;
    public NetworkButtons Buttons;
    public Vector2 LookDelta;
}
