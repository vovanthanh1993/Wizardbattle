using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameConstants
{
    // Scenes
    public const string LOBBY_SCENE = "MainMenu";
    public const int GAMEPLAY_SCENE_INDEX = 2;

    // UI Messages
    public const string CONNECTING = "Connecting...";
    public const string CREATING_ROOM = "Creating Room...";
    public const string JOINING_ROOM = "Joining Room...";
    public const string SEARCHING_ROOM = "Searching Room...";
    public const string ROOM_NAME_REQUIRED = "Please enter a room name!";
    public const string PLAYER_NAME_REQUIRED = "Please enter your name before joining a room!";
    public const string CONNECTED_STATUS = "Connected!";

    // Format Strings
    public const string WIN_GAME_FORMAT = "{0} Win Game!";
    public const string JOIN_ROOM_FORMAT = "Joining room: {0}";
    public const string RESPAWN_FORMAT = "Respawn in {0}...";
    public const string ROOM_NAME_DISPLAY_FORMAT = "Room: {0}";
    public const string UNKNOWN_ROOM = "Unknown Room";

    // Defaults
    public const string DEFAULT_PLAYER_NAME_PREFIX = "Player";
    public const int DEFAULT_PLAYER_NAME_MIN = 1000;
    public const int DEFAULT_PLAYER_NAME_MAX = 9999;
    public const string DEFAULT_ROOM_NAME = "DefaultRoom";
    public const string PLAYER_PREFS_NAME_KEY = "PlayerName";

    // Timing
    public const float STATUS_HIDE_DELAY = 2f;

    // Random room code range
    public const int RANDOM_ROOM_MIN = 100000;
    public const int RANDOM_ROOM_MAX = 999999;

    // UI Text Element Names
    public const string ORDER_TEXT_NAME = "OderText";
    public const string NAME_TEXT_NAME = "NameText";
    public const string KILL_TEXT_NAME = "KillText";
    public const string DEATH_TEXT_NAME = "DeathText";

    // Skill Cooldown Constants
    public const float DEFAULT_FIREBALL_COOLDOWN = 2f;
    public const string FIREBALL_SKILL_NAME = "Fireball";
    public const string COOLDOWN_TEXT_FORMAT = "{0}";

    public static readonly SessionLobby LOBBY = SessionLobby.Shared;
    public const int RANDOM_ROOM_SUFFIX_MIN = 1;
    public const int RANDOM_ROOM_SUFFIX_MAX = 1001;
    public const string STATUS_JOINING_LOBBY = "Joining lobby...";
    public const string STATUS_IN_LOBBY = "In Lobby";
    public const string STATUS_FAILED_LOBBY = "Failed to join lobby! ";
    public const string STATUS_GETTING_ROOM_LIST = "Getting room list...";
    public const string STATUS_JOINING_ROOM = "Joining room: ";
    public const string STATUS_CREATING_ROOM = "Creating new room: ";
    public const string STATUS_FAILED_JOIN_ROOM = "Failed to join room!";
    public const string STATUS_FAILED_CREATE_ROOM = "Failed to create room!";
    public const string STATUS_ROOM_NOT_FOUND = "Room not found";
    public const string STATUS_FAILED_CONNECT = "Failed to connect. Reason: ";
    public const string STATUS_SERVER_DISCONNECTED = "Server disconnected!";
}