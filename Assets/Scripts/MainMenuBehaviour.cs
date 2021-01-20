using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuBehaviour : MonoBehaviour
{
    public static MainMenuBehaviour instance;
    public InputField nameField = null;

    [SerializeField] private GameObject nameAndIpPanel = null;
    //[SerializeField] private GameObject lobbyPanel = null;
    [SerializeField] private GameObject mainMenuPanel = null;
    [SerializeField] private GameObject attemptingToJoinPanel = null;
    [SerializeField] private GameObject failedToJoinPanel = null;

    [SerializeField] private Button hostButton = null;
    [SerializeField] private Button joinButton = null;
    [SerializeField] private InputField ipField = null;

    //[SerializeField] private Text playerLobbyCounterText = null;
    //[SerializeField] private PlayerLobbyCounter playerLobbyCounter= null;

    [SerializeField] private NetworkManagerLobby networkManager = null;

    private void Start()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);

        mainMenuPanel.SetActive(true);
        nameAndIpPanel.SetActive(false);
        //lobbyPanel.SetActive(false);
        attemptingToJoinPanel.SetActive(false);
        failedToJoinPanel.SetActive(false);

        hostButton.interactable = false;
        joinButton.interactable = false;
    }

    // Called wehn we hit the play buttonto show the panel.
    public void PlayButtonPressed()
    {
        nameAndIpPanel.SetActive(true);
    }

    // Called when we press the back button on the name and ip panel.
    public void BackButtonPressed()
    {
        nameAndIpPanel.SetActive(false);
    }

    // Called when the quit button is pressed
    public void QuitButtonPressed()
    {
        Application.Quit();
    }

    // Called after the cancel button is pressed when we are trying to load in.
    public void CancelConnectionButtonPressed()
    {
        nameAndIpPanel.GetComponent<CanvasGroup>().interactable = true;
    }

    // Called after we press the okay button after we failed a connection.
    public void AcceptConnectionFailedButtonPressed()
    {
        nameAndIpPanel.GetComponent<CanvasGroup>().interactable = true;
        failedToJoinPanel.SetActive(false);
    }

    // Called after we stop interacting with an input field to see what buttons we should set as interactable.
    public void CheckButtonStatus()
    {
        if(!string.IsNullOrEmpty(nameField.text))
        {
            hostButton.interactable = true;

            if (!string.IsNullOrEmpty(ipField.text))
            {
                joinButton.interactable = true;
                NetworkManagerLobby.OnClientConnected += HandleClientConnected;
                NetworkManagerLobby.OnClientDisconnected += HandleClientDisconnected;
            }
            else
            {
                joinButton.interactable = false;
                NetworkManagerLobby.OnClientConnected -= HandleClientConnected;
                NetworkManagerLobby.OnClientDisconnected -= HandleClientDisconnected;
            }
        }
        else
        {
            hostButton.interactable = false;
            joinButton.interactable = false;
            NetworkManagerLobby.OnClientConnected -= HandleClientConnected;
            NetworkManagerLobby.OnClientDisconnected -= HandleClientDisconnected;
        }
    }

    // Called when the host button is pressed
    public void HostButtonPressed()
    {
        networkManager.StartHost();

        mainMenuPanel.SetActive(false);
        nameAndIpPanel.SetActive(false);
        //lobbyPanel.SetActive(true);
    }

    // Called when the join button is pressed
    public void JoinButtonPressed()
    {
        networkManager.networkAddress = ipField.text;
        networkManager.StartClient();
        attemptingToJoinPanel.SetActive(true);
        nameAndIpPanel.GetComponent<CanvasGroup>().interactable = false;
        joinButton.interactable = false;
    }

    // This is called when a new player has joined, force the server to update all the player counters.
    /*
    public void NewPlayerJoined()
    {
        playerLobbyCounter.NewPlayerJoined();
    }
    */

    private void HandleClientConnected()
    {
        joinButton.interactable = true;

        //lobbyPanel.SetActive(true);
        attemptingToJoinPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        nameAndIpPanel.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        Debug.Log("main menu just realized were disconnected");
        joinButton.interactable = true;
        nameAndIpPanel.GetComponent<CanvasGroup>().interactable = false;
        attemptingToJoinPanel.SetActive(false);
        failedToJoinPanel.SetActive(true);
    }
    

}
