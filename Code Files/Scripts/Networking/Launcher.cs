using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Photon;

//reference pages for any aid in using photon
//https://doc.photonengine.com/en-us/pun/current/tutorials/pun-basics-tutorial/intro pun basic tutorial
//https://doc.photonengine.com/en-us/pun

public class Launcher : PunBehaviour
{
    #region Public Members
    public static Launcher Instance = null;

    public string m_GameVersion = "1.0";
    public Animator MainMenuAnimator;
    #endregion

    #region Private Members
    private RoomOptions m_RoomDetails;   
    #endregion

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }        

        PhotonNetwork.autoJoinLobby = false;
        PhotonNetwork.automaticallySyncScene = true;
        //the game will start in networked mode on and then by calling SwapOfflineMode() will swap to the opposite online mode
        PhotonNetwork.offlineMode = false;

        if (RoomDetails == null)
        {
            m_RoomDetails = new RoomOptions();
            //makes it so that way this room can be viewed
            RoomDetails.IsVisible = true;
            //set the maximum players per room to be 2 because this is a coop game
            RoomDetails.MaxPlayers = 2;
        }

        GameManager.Instance.NeverDestroy(this.gameObject);

        Connect();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    #region Photon Server Connection Functionality
    //connect to the photon master server
    public void Connect()
    {
        if (!PhotonNetwork.connected)
        {
            //connect to the photon master server using the settings set up in this class
            PhotonNetwork.ConnectUsingSettings(m_GameVersion);
        }
    }

    //disconnect the user from the photon network
    public void Disconnect()
    {
        if (PhotonNetwork.connected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            //Connect();
        }
    }

    //joins a photonlobby
    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.autoJoinLobby = true;
        MainMenuAnimator.SetBool("MoveToLobby", true);
    }

    //call this to change the photonnetwork into offline mode
    public void PlayOfflineMode()
    {
        PhotonNetwork.offlineMode = true;
    }

    //call this to change the photonnetwork into online mode
    public void PlayOnlineMode()
    {
        PhotonNetwork.offlineMode = false;
        Connect();
    }

    //this will make the user leave the room they are in and bring them back to the lobby
    public void LeaveRoom()
    {
        PhotonNetwork.room.IsVisible = false;
        PhotonNetwork.LeaveRoom();
        GameManager.Instance.OnReceivedRoomListUpdate();
        
    }

    //this will make the user leave the lobby screne and they won't recieve updates on available rooms
    public void LeaveLobby()
    {
        PhotonNetwork.LeaveLobby();       
    }

    //this will create a new room
    public void CreateNewGame(string roomName)
    {
        Debug.Log("Creating a new game", this);
        PhotonNetwork.player.CustomProperties["PlayerNumber"] = PhotonNetwork.playerList.Length;
        //if we wanted to handle joinning a friends game we could do it here
        PhotonNetwork.CreateRoom(roomName, RoomDetails, null);
        MainMenuAnimator.SetBool("AmMasterClient", true);
        MainMenuAnimator.SetBool("AmInRoom", true);

    }

    //usable for a lobby this will connect the player to a room that is displayed by the UI
    public void JoinRoom(Text roomName)
    {
        Debug.Log("Joinning an active game with room name " + roomName.text, this);
        PhotonNetwork.JoinRoom(roomName.text);
        MainMenuAnimator.SetBool("AmMasterClient", false);
        MainMenuAnimator.SetBool("AmInRoom", true);
    }

    public void JoinRandomRoom()
    {
        Debug.Log("Joinning random room", this);
        PhotonNetwork.JoinRandomRoom();
    }
    #endregion

    #region Scene Navigation
    public void LoadScene()
    {
        //used if the game is in offline mode
        if (PhotonNetwork.offlineMode)
        {
            SceneManager.LoadScene(1);
        }
        //used if the game is in online mode connected to the photon cloud
        else if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.LoadLevel(1);
        }
    }

    public void LoadScene(int sceneIndex)
    {
        //used if the game is in offline mode
        if (PhotonNetwork.offlineMode)
        {
            SceneManager.LoadScene(sceneIndex);
        }
        //used if the game is in online mode connected to the photon cloud
        else if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.LoadLevel(sceneIndex);
        }
    }

    public void ReturnToMain()
    {
        //used if the game is in offline mode
        if (PhotonNetwork.offlineMode)
        {
            SceneManager.LoadScene(0);
        }
        //used if the game is in online mode connected to the photon cloud
        else if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.LoadLevel(0);
        }
    }
    #endregion

    #region Photon Callback Methods

    //if we try to connect to a random room and fail then we want to handle that case here
    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        Debug.Log("No room existed that we could join, creating a new room", this);
        PhotonNetwork.CreateRoom("new room", RoomDetails, null);
    }

    //call back for if we aren't able to join a specific room
    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        Debug.Log("Was unable to join that room: " + codeAndMsg.ToString(), this);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Player has joinned room", this);
        PhotonNetwork.player.CustomProperties["PlayerNumber"] = PhotonNetwork.playerList.Length;
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Player has left room", this);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Player has connected to the lobby system", this);
    }

    public override void OnLeftLobby()
    {
        Debug.Log("Player has exited the lobby", this);
    }

    //handle the logic needed for when we connect to the photon master server
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to the Photon Master Server", this);
    }

    [PunRPC]
    public void SyncScenes(bool flag)
    {
        PhotonNetwork.automaticallySyncScene = flag;
    }    
    #endregion

    #region Properties
    public RoomOptions RoomDetails { get { return m_RoomDetails; } }
    #endregion
}
