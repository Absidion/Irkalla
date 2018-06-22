using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//Last Updated: 3/14/2018 James
public class MenuManager : Photon.PunBehaviour
{
    public Animator MenuAnimator;                                                       //The Animator object for the main camera that controls it's movements in the scene.
    public Transform MasterClientCameraTransform;                                       //Location of where the camera should move to for the MC Room canvas.
    public Transform ClientCameraTransform;                                             //Located of where the camera should move to for the Client Room Canvas
    public Dropdown CharacterSelect;

    private Launcher m_Launcher;                                                        //an instance of the game launcher
    private Dictionary<string, Canvas> m_CanvasList;                                    //a list of the canvases in the scene   

    private string m_CurrentCanvas;                                                     //the current canvas that is active in the scene
    private string m_PreviousCanvas;                                                    //the previous canvas that was active in the scene

    private const string UserName = "UserName";                                         //private string to hold the value "Username"
    private const string StartMenu = "StartMenu";                                       //private string to hold the value "StartMenu"
    private const string GameRoomMasterClient = "MasterClientRoomCanvas";               //private string to hold the value "MasterClientRoomCanvas"
    private const string GameRoomClient = "ClientRoomCanvas";                           //private string to hold the value "ClientRoomCanvas"
    private const string WarningPopup = "WarningPopup";                                 //Private string to hold the value "WarningPopup"

    private bool m_CountDownReady = false;

    private float m_TimeToStart = 4.0f;

    private UnityEngine.Events.UnityAction StartGameAction;

    public Launcher Launcher { get { return m_Launcher; } }

    void Awake()
    {
        StartGameAction += GameObject.Find("Managers").GetComponent<Launcher>().LoadScene;

        //initialize the canvas dictionary
        m_CanvasList = new Dictionary<string, Canvas>();
        //get the launcher in the scene
        m_Launcher = FindObjectOfType<Launcher>();
        Canvas[] sceneCanvases = FindObjectsOfType<Canvas>();
        for (int i = 0; i < sceneCanvases.Length; i++)
        {
            //save the canvases into the dictionary and set them to be inactive
            m_CanvasList[sceneCanvases[i].name] = sceneCanvases[i];
            m_CanvasList[sceneCanvases[i].name].gameObject.SetActive(false);
            if (m_CanvasList[sceneCanvases[i].name].name == "Pre-Alpha Build Overlay")
            {
                m_CanvasList[sceneCanvases[i].name].gameObject.SetActive(true);
            }
        }
        //set the value of the inputfield in the UserName canvas to be equal to a stored value in the registrey
        m_CanvasList[UserName].GetComponentInChildren<InputField>().text = PlayerPrefs.GetString(UserName);
        //reset the canvas data
        ResetCanvasData();

        MenuAnimator = Camera.main.GetComponent<Animator>();
        Launcher.MainMenuAnimator = Camera.main.GetComponent<Animator>();

        if (GameManager.m_StartInLobby)
        {
            ReturnToLobby();
            GameObject.Find("ReadyGameMaster").GetComponent<Button>().onClick.AddListener(StartGameAction);
        }

        SoundManager.GetInstance();
        SoundManager.PlayAmbient();

    }

    private void Update()
    {
        if (m_CurrentCanvas == "Lobby")
        {
            UpdateRoomList();
        }

        if (m_CanvasList[GameRoomMasterClient].gameObject.activeSelf)
        {
            if (GameObject.Find("MasterClientToggleInMaster").GetComponent<Toggle>().isOn && GameObject.Find("ClientToggleInMaster").GetComponent<Toggle>().isOn && !GameManager.CountDownCanStart)
                GameManager.Instance.photonView.RPC("CanStartCountdown", PhotonTargets.All);
            else if ((!GameObject.Find("MasterClientToggleInMaster").GetComponent<Toggle>().isOn || !GameObject.Find("ClientToggleInMaster").GetComponent<Toggle>().isOn) && GameManager.CountDownCanStart)
                GameManager.Instance.photonView.RPC("CanStartCountdown", PhotonTargets.All);
        }

        if (GameManager.CountDownCanStart)
        {
            m_TimeToStart -= Time.deltaTime;
            if (m_TimeToStart <= 0)
            {
                m_TimeToStart = 0;
                GameManager.CountDownCanStart = false;
                StartGame();
            }
            UpdateStartCountdown(m_TimeToStart);
        }
        else
        {
            m_TimeToStart = 4.0f;
            UpdateStartCountdown(10.0f);
        }
    }

    private void ReturnToLobby()
    {
        MenuAnimator.SetBool("MoveToMain", true);
        MenuAnimator.SetBool("MoveToLobby", true);
        MenuAnimator.SetBool("AmInRoom", true);

        if (PhotonNetwork.isMasterClient)
        {
            MenuAnimator.SetBool("AmMasterClient", true);
            ActivateNewCanvas("MasterClientRoomCanvas");
        }
        else
        {
            MenuAnimator.SetBool("AmMasterClient", false);
            ActivateNewCanvas("ClientRoomCanvas");
        }

        SetupServerinfo();
        UpdatePlayerList();
        GameManager.m_StartInLobby = false;
    }

    //create a coop game from the lobby
    public void CreateCoopGame(Text roomName)
    {
        if (roomName.text.ToString() != string.Empty)
        {
            Launcher.CreateNewGame(roomName.text);
        }
        else
        {
            Launcher.CreateNewGame(PhotonNetwork.player.NickName + "'s Room");
        }
    }

    //saves the users data into photon as well as into the registry
    public void SaveUserData(Text username)
    {
        GameManager.Instance.SetUserNickname(username.text);
        PlayerPrefs.SetString(UserName, username.text);
        PlayerPrefs.Save();
    }

    //loads a new canvas to be the current active one
    public void ActivateNewCanvas(string newCanvas)
    {
        m_PreviousCanvas = m_CurrentCanvas;
        m_CurrentCanvas = newCanvas;

        if (newCanvas == "Options")
        {
            MenuAnimator.SetBool("MoveToOptions", true);
            MenuAnimator.SetBool("MoveToMain", false);
        }
        if (newCanvas == "MainMenu")
        {
            MenuAnimator.SetBool("MoveToOptions", false);
            MenuAnimator.SetBool("MoveToLobby", false);
            MenuAnimator.SetBool("MoveToMain", true);
            MenuAnimator.SetBool("MoveToCredits", false);
            MenuAnimator.SetBool("MoveToHelp", false);
        }
        if (newCanvas == "Lobby")
        {
            MenuAnimator.SetBool("MoveToMain", false);
            MenuAnimator.SetBool("AmInRoom", false);
            MenuAnimator.SetBool("MoveToLobby", true);

        }
        if (newCanvas == "UserName")
        {
            MenuAnimator.SetBool("MoveToMain", false);
            MenuAnimator.SetBool("MoveToCredits", false);
        }
        if (newCanvas == "Credits")
        {
            MenuAnimator.SetBool("MoveToMain", false);
            MenuAnimator.SetBool("MoveToCredits", true);
        }
        if (newCanvas == "HelpCanvas")
        {
            //MenuAnimator.SetBool("MoveToMain", false);
            MenuAnimator.SetBool("MoveToHelp", true);
        }

        UpdateCanvases();
    }

    //load up the previous canvas, used for a back button
    public void LoadPreviousCanvas()
    {
        m_CanvasList[m_CurrentCanvas].gameObject.SetActive(false);
        m_CanvasList[m_PreviousCanvas].gameObject.SetActive(true);
        m_CanvasList[m_PreviousCanvas].GetComponentInChildren<Button>().Select();

        string temp = m_CurrentCanvas;
        m_CurrentCanvas = m_PreviousCanvas;
        m_PreviousCanvas = temp;
    }

    //reset the canvas data back to what it is when you first launch the game
    public void ResetCanvasData()
    {
        m_CanvasList[StartMenu].gameObject.SetActive(true);
        m_CanvasList[UserName].gameObject.SetActive(true);
        m_CurrentCanvas = UserName;
        m_PreviousCanvas = UserName;
    }

    //update the canvases so the correct one is currently showing
    private void UpdateCanvases()
    {
        m_CanvasList[m_CurrentCanvas].gameObject.SetActive(true);
        m_CanvasList[m_CurrentCanvas].GetComponentInChildren<Button>().Select();
        m_CanvasList[m_PreviousCanvas].gameObject.SetActive(false);
    }

    private void UpdatePlayerList()
    {
        if (PhotonNetwork.inRoom)
        {
            if (PhotonNetwork.playerList.Length == 2)
            {
                PhotonNetwork.room.IsVisible = false;
            }
            else
            {
                PhotonNetwork.room.IsVisible = true;
            }
        }

        if (m_CurrentCanvas == GameRoomMasterClient || m_CurrentCanvas == GameRoomClient)
        {
            //clear the list of players in the scroll view
            m_CanvasList[m_CurrentCanvas].GetComponentInChildren<ScrollRect>().viewport.GetComponentInChildren<Text>().text = string.Empty;
            //add the players into the list one after the other
            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                m_CanvasList[m_CurrentCanvas].GetComponentInChildren<ScrollRect>().viewport.GetComponentInChildren<Text>().text += (player.NickName + "\n");
            }
        }
    }

    public void UpdateStartCountdown(float elapsed)
    {
        string CountDownText = "Game Starting in... " + (int)elapsed;

        if (elapsed == 10.0f)
        {
            CountDownText = "";
        }

        if (m_CanvasList[GameRoomMasterClient].gameObject.activeSelf)
            GameObject.Find("MasterCountdown").GetComponent<Text>().text = CountDownText;

        if (m_CanvasList[GameRoomClient].gameObject.activeSelf)
            GameObject.Find("ClientCountdown").GetComponent<Text>().text = CountDownText;
    }

    public void StartGame()
    {
        Launcher.LoadScene();
    }

    public void LeaveLobby()
    {
        Launcher.LeaveLobby();
    }

    public void JoinRoom(Text roomName)
    {
        Launcher.JoinRoom(roomName);
    }

    public void LeaveRoom()
    {
        Launcher.LeaveRoom();
    }

    public void JoinLobby()
    {
        Launcher.JoinLobby();
        PhotonNetwork.automaticallySyncScene = true;
    }

    public void ReturnToRoomFromWarning()
    {
        ActivateNewCanvas(GameRoomMasterClient);
    }

    private void WarningSoloPlayer()
    {
        ActivateNewCanvas(WarningPopup);
    }
    public static void UpdateTogglesForMaster()
    {
        if (GameObject.Find("ClientToggleInMaster").GetComponent<Toggle>().isOn)
            GameObject.Find("ClientToggleInMaster").GetComponent<Toggle>().isOn = false;
        else
            GameObject.Find("ClientToggleInMaster").GetComponent<Toggle>().isOn = true;
    }

    public static void UpdateTogglesForClient()
    {
        if (GameObject.Find("MasterClientToggleInClient").GetComponent<Toggle>().isOn)
            GameObject.Find("MasterClientToggleInClient").GetComponent<Toggle>().isOn = false;
        else
            GameObject.Find("MasterClientToggleInClient").GetComponent<Toggle>().isOn = true;
    }

    //UI Elements cannot access the Managers Object when we re-enter the Main Menu scene from the Game Scene.
    //This function is a way to still access what we need, and this means all UI functionality can be using the MenuManager instead.
    public void SetCharacter(UnityEngine.UI.Dropdown character)
    {
        //gets the value from the dropdown
        string gameCharacter = character.options[character.value].text;
        //check to see the value inside of the string is equal to samurai or cowboy and set the local player equal to the right one
        GameManager.Instance.SetCharacter(gameCharacter);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    //UI Elements cannot access the Managers Object when we re-enter the Main Menu scene from the Game Scene.
    //This function is a way to still access what we need, and this means all UI functionality can be using the MenuManager instead.

    public void ReadyUp()
    {
        if (PhotonNetwork.room.PlayerCount == 1)
        {
            WarningSoloPlayer();
        }
        else
        {
            if (PhotonNetwork.isMasterClient)
            {

                if (GameObject.Find("MasterClientToggleInMaster").GetComponent<Toggle>().isOn)
                    GameObject.Find("MasterClientToggleInMaster").GetComponent<Toggle>().isOn = false;
                else
                    GameObject.Find("MasterClientToggleInMaster").GetComponent<Toggle>().isOn = true;

                GameManager.Instance.photonView.RPC("TellPlayerYouAreReady", PhotonTargets.Others);
            }
            else
            {
                GameManager.Instance.photonView.RPC("TellPlayerYouAreReady", PhotonTargets.MasterClient);

                if (GameObject.Find("ClientToggleInClient").GetComponent<Toggle>().isOn)
                    GameObject.Find("ClientToggleInClient").GetComponent<Toggle>().isOn = false;
                else
                    GameObject.Find("ClientToggleInClient").GetComponent<Toggle>().isOn = true;
            }

        }
    }

    public void UpdateRoomList()
    {
        int index = 0;

        ScrollRect rect = m_CanvasList[m_CurrentCanvas].GetComponentInChildren<ScrollRect>();
        Text t = rect.viewport.GetComponentInChildren<Text>();
        Button[] b = m_CanvasList[m_CurrentCanvas].GetComponentInChildren<ScrollRect>().GetComponentsInChildren<Button>();
        for (int i = 0; i < b.Length; i++)
        {
            b[i].transform.GetChild(0).GetComponent<Text>().text = "";
            b[i].transform.GetChild(1).GetComponent<Text>().text = "";
            b[i].transform.GetChild(2).GetComponent<Text>().text = "";
            b[index].interactable = false;
        }

        foreach (RoomInfo room in PhotonNetwork.GetRoomList())
        {
            b[index].transform.GetChild(0).GetComponent<Text>().text = room.PlayerCount + "/" + room.MaxPlayers;
            b[index].transform.GetChild(1).GetComponent<Text>().text = room.Name;
            b[index].transform.GetChild(2).GetComponent<Text>().text = PhotonNetwork.networkingPeer.RoundTripTime.ToString();
            b[index].interactable = true;

            index++;
        }
    }

    #region Photon Overriden Methods

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        UpdatePlayerList();
    }
    public override void OnReceivedRoomListUpdate()
    {
        if (m_CurrentCanvas == "Lobby")
        {
            UpdateRoomList();
        }
    }

    public override void OnJoinedRoom()
    {
        SetupServerinfo();

        UpdatePlayerList();
    }

    public void SetupServerinfo()
    {
        foreach (Text text in m_CanvasList[m_CurrentCanvas].GetComponentsInChildren<Text>())
        {
            if (text.name == "Server Name: ")
            {
                text.text = (PhotonNetwork.room.Name);
            }
        }
    }

    public override void OnLeftRoom()
    {
        UpdatePlayerList();
    }

    #endregion
}