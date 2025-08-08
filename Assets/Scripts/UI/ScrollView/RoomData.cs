
using System;

[Serializable]
public class RoomData
{
    public string RoomName { get; }
    public int PlayerCount { get; }
    public int MaxPlayers { get; }

    public RoomData(string roomName, int playerCount, int maxPlayers)
    {
        RoomName = roomName;
        PlayerCount = playerCount;
        MaxPlayers = maxPlayers;
    }
}