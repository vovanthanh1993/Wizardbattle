using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton buffer for collecting player input from Unity each frame.
/// </summary>
public class InputManager : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
{
    public static InputManager Instance { get; private set; }
    private NetworkInputData _accumulatedInput;
    private bool _resetInput;
    public bool IsVisibleLeaderBoard { get; set; }
    public bool IsVisibleMenuInGame { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void UpdateCursorState()
    {
        if (IsVisibleLeaderBoard || IsVisibleMenuInGame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    public void BeforeUpdate()
    {
        if (_resetInput)
        {
            _resetInput = false;
            _accumulatedInput = default;
        }

        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (SceneManager.GetActiveScene().name != "LobbyScene")
        {
            if (keyboard != null && keyboard.tabKey.wasPressedThisFrame && !IsVisibleMenuInGame)
            {
                IsVisibleLeaderBoard = !IsVisibleLeaderBoard;
                UIManager.Instance.ShowLeaderBoard(IsVisibleLeaderBoard);
                UpdateCursorState();

            }

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                IsVisibleMenuInGame = !IsVisibleMenuInGame;
                UIManager.Instance.ShowInGameMenu(IsVisibleMenuInGame);
                UpdateCursorState();
            }
        }

        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        NetworkButtons buttons = default;
        if (mouse != null)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            Vector2 lookRotationDelta = new(-mouseDelta.y, mouseDelta.x);
            _accumulatedInput.LookDelta += lookRotationDelta;
        }

        if (keyboard != null)
        {
            Vector2 moveDirection = Vector2.zero;

            if (keyboard.wKey.isPressed)
                moveDirection += Vector2.up;
            if (keyboard.sKey.isPressed)
                moveDirection += Vector2.down;
            if (keyboard.aKey.isPressed)
                moveDirection += Vector2.left;
            if (keyboard.dKey.isPressed)
                moveDirection += Vector2.right;

            _accumulatedInput.Direction += moveDirection;
            buttons.Set(InputButtons.Jump, keyboard.spaceKey.isPressed);
            buttons.Set(InputButtons.Fire, mouse.leftButton.isPressed);
            buttons.Set(InputButtons.Heal, keyboard.qKey.isPressed);
            buttons.Set(InputButtons.Stealth, keyboard.eKey.isPressed);
        }

        _accumulatedInput.Buttons = new NetworkButtons(_accumulatedInput.Buttons.Bits | buttons.Bits);
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        _accumulatedInput.Direction.Normalize();
        input.Set(_accumulatedInput);
        _resetInput = true;
        _accumulatedInput.LookDelta = default;
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}