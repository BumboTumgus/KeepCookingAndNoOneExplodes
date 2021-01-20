using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class NetworkManagerLobby : NetworkManager
{
    [SerializeField] private int minPlayers = 2;
    [SerializeField] private string menuSceneName;

    [Header("Room")]
    [SerializeField] private NetworkRoomPlayerKCANOE roomPlayerPrefab = null;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

    public List<NetworkRoomPlayerKCANOE> RoomPlayers { get; } = new List<NetworkRoomPlayerKCANOE>();

    // ste the spawn prefabs to a folder in the resources fodler called spawnable prefabs.
    public override void OnStartServer() => spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

    public override void OnStartClient()
    {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach (var prefab in spawnablePrefabs)
            ClientScene.RegisterPrefab(prefab);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        OnClientDisconnected?.Invoke();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        // Remove this connection and reset the lobbies ready up check.
        if(conn.identity != null)
        {
            var player = conn.identity.GetComponent<NetworkRoomPlayerKCANOE>();

            RoomPlayers.Remove(player);

            NotifyPlayersOfReadyState();
        }

        base.OnServerDisconnect(conn);

        OnClientDisconnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        // If we have too many players in the lobby, disconnect this player
        if(numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }

        // If we are not in the main menu where the lobby is located, disconnect the player.
        if(SceneManager.GetActiveScene().name != menuSceneName)
        {
            conn.Disconnect();  
            return;
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // Create a new player and give ownership of this player to the incoming connection we are assigning it to.
        if (SceneManager.GetActiveScene().name == menuSceneName)
        {
            bool isLeader = RoomPlayers.Count == 0;

            NetworkRoomPlayerKCANOE roomPlayerInstance = Instantiate(roomPlayerPrefab);

            roomPlayerInstance.IsLeader = isLeader;

            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);

            //FindObjectOfType<MainMenuBehaviour>().NewPlayerJoined();
            //CmdUpdatePlayerCounter();
        }
    }

    public override void OnStopServer()
    {
        RoomPlayers.Clear();

    }

    public void NotifyPlayersOfReadyState()
    {
        foreach(var player in RoomPlayers)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    private bool IsReadyToStart()
    {
        if (numPlayers < minPlayers) { return false; }

        foreach(var player in RoomPlayers)
        {
            if (!player.IsReady) { return false; }
        }

        return true;
    }
}
