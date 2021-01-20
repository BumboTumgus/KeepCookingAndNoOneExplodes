using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerLobbyCounter : NetworkBehaviour
{
    [SerializeField] private Text playerLobbyCounterText = null;
    [SerializeField] private NetworkManagerLobby networkManager;

    // This is called when a new player joined so we can force all the lobby text fields to sync.
    [Client]
    public void NewPlayerJoined()
    {
        Debug.Log("recieved new player join method call from main menu");
        CmdUpdatePlayerCounter();

        Debug.Log(" trying to call the rpcs directly");
        RpcUpdatePlayerCounter();
    }
    
    [Command]
    private void CmdUpdatePlayerCounter()
    {
        Debug.Log("running the update player count command");
        RpcUpdatePlayerCounter();
    }

    [ClientRpc]
    private void RpcUpdatePlayerCounter()
    {
        Debug.Log("Updating the player count");
        playerLobbyCounterText.text = string.Format("Current Player count: {0} / {1}", networkManager.numPlayers, networkManager.maxConnections);
    }
}
