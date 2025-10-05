using Fusion;
using UnityEngine;

public enum InputButtons
{
    Run,
    Fire,

    Heal,

    Stealth,

    Emoji1,

    Emoji2,

    Emoji3,

    Emoji4
}
public struct NetworkInputData : INetworkInput
{
    public Vector2 Direction;
    public NetworkButtons Buttons;
    public Vector2 LookDelta;
}
