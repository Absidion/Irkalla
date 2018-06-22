using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Writer: Liam
//Last Updated: 2/4/2018 James

[RequireComponent(typeof(PhotonView))]
public class GameManager : SyncBehaviour
{
    #region Public Variables
    public bool DebugMode = false;
    public static bool m_StartInLobby = false;
    private static bool countDownCanStart = false;
    public static GameManager Instance;
    public static int FloorNumber = 0;

    public static event EventHandler LevelGeneration;
    public static event EventHandler LevelBaking;
    public static event EventHandler LevelPopulation;
    public static event EventHandler PlayerPopulation;
    public static event EventHandler PostPlayer;
    public static event EventHandler FinalizeObjectPools;
    public static event EventHandler FinalStep;
    #endregion

    #region Private Variables
    private List<GameObject> m_NormalObjects;
    private List<GameObject> m_NetworkObjects;

    private bool m_EventsCompleted = false;
    private float m_Delay = 0.0f;

    private LevelCreationState m_MasterClientState = LevelCreationState.LevelGeneration;
    private LevelCreationState m_ClientState = LevelCreationState.LevelGeneration;

    [SyncThis]
    protected int m_SeedValue = -1;
    [SyncThis]
    protected string m_SeedString = string.Empty;

    private string m_PreTranslatedSeed = string.Empty;
    private string m_TranslatedSeed = string.Empty;


    private GameCharacters m_LocalPlayerCharacter = GameCharacters.Cowgirl;
    #endregion

    #region Unity Methods/Callbacks
    protected override void Awake()
    {
        base.Awake();

        if (Instance == null)
        {
            //create the singleton instance of this object in the world
            Instance = this;
        }
        else
        {
            //if there is already an instance of the singleton object in the world destory, across the network, this new imposter version
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        m_NormalObjects = new List<GameObject>();
        m_NetworkObjects = new List<GameObject>();

        NeverDestroy(this.gameObject);

        SceneManager.sceneLoaded += OnLevelLoaded;
    }

    private void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetGameManager();
    }

    private void OnDestroy()
    {
        ClearEvents();
    }

    private void OnDisable()
    {
        ClearEvents();
    }

    public void ClearEvents()
    {
        //loop through every event and unsubscribe the events
        if (LevelGeneration != null)
            foreach (var eventDelegate in LevelGeneration.GetInvocationList())
            {
                LevelGeneration -= (eventDelegate as EventHandler);
            }

        if (LevelBaking != null)
            foreach (var eventDelegate in LevelBaking.GetInvocationList())
            {
                LevelBaking -= (eventDelegate as EventHandler);
            }

        if (LevelPopulation != null)
            foreach (var eventDelegate in LevelPopulation.GetInvocationList())
            {
                LevelPopulation -= (eventDelegate as EventHandler);
            }

        if (PlayerPopulation != null)
            foreach (var eventDelegate in PlayerPopulation.GetInvocationList())
            {
                PlayerPopulation -= (eventDelegate as EventHandler);
            }

        if (PostPlayer != null)
            foreach (var eventDelegate in PostPlayer.GetInvocationList())
            {
                PostPlayer -= (eventDelegate as EventHandler);
            }

        if (FinalizeObjectPools != null)
            foreach (var eventDelegate in FinalizeObjectPools.GetInvocationList())
            {
                FinalizeObjectPools -= (eventDelegate as EventHandler);
            }

        if (FinalStep != null)
            foreach (var eventDelegate in FinalStep.GetInvocationList())
            {
                FinalStep -= (eventDelegate as EventHandler);
            }
    }

    private void ResetGameManager()
    {
        m_EventsCompleted = false;
        m_MasterClientState = LevelCreationState.LevelGeneration;
        m_ClientState = LevelCreationState.LevelGeneration;
    }

    void Update()
    {
        if (!m_EventsCompleted)
        {
            if (m_Delay >= 1.5f)
            {
                m_Delay = 0.0f;
                UpdateEventSequence();
            }
            else
                m_Delay += Time.deltaTime;
        }

        if (CharacterController.GetNextLevelDown())
        {
            ProceedToNextLevel();
        }
    }
    #endregion

    //This method will move through the steps of setting up the game generation states
    private void UpdateEventSequence()
    {
        bool canRunEvents = false;
        LevelCreationState currentState = m_ClientState;

        //what this code will do is compare the master client's event state and the client's event state. If the master client is behind or at the equivalent state then that means
        //that the master client can run. If the client is at an equivelent or lesser state then it can run. This is all relative to how values get recieved but should allow the client
        //and master client to run indepently but also while feeding from eachother.
        if (PhotonNetwork.isMasterClient)
        {
            canRunEvents = m_MasterClientState <= m_ClientState;
            currentState = m_MasterClientState;
        }
        else
            canRunEvents = m_ClientState <= m_MasterClientState;

        //DEBUGING PURPOSES ONLY
        if (DebugMode)
        {
            m_MasterClientState = m_ClientState;
            canRunEvents = true;
        }
        if (PhotonNetwork.playerList.Length == 1)
            canRunEvents = true;

        if (canRunEvents)
        {
            switch (currentState)
            {
                case LevelCreationState.LevelGeneration:

                    if (LevelGeneration != null)
                        LevelGeneration(this, null);
                    Debug.Log("LevelGeneration completed, moving to LevelBaking.");
                    break;

                case LevelCreationState.LevelBaking:

                    if (LevelBaking != null)
                        LevelBaking(this, null);
                    Debug.Log("LevelBaking complete, moving to LevelPopulation.");
                    break;

                case LevelCreationState.LevelPopulation:

                    if (LevelPopulation != null)
                        LevelPopulation(this, null);
                    Debug.Log("LevelPopulation complete, moving to PlayerPopulation");
                    break;

                case LevelCreationState.PlayerPopulation:

                    if (PlayerPopulation != null)
                        PlayerPopulation(this, null);
                    Debug.Log("PlayerPopulation complete, moving to PostPlayer.");
                    break;

                case LevelCreationState.PostPlayer:

                    if (PostPlayer != null)
                        PostPlayer(this, null);
                    Debug.Log("PostPlayer complete, moving to FinalizeObjectPools.");
                    break;

                case LevelCreationState.FinalizeObjectPools:
                    if (FinalizeObjectPools != null)
                        FinalizeObjectPools(this, null);
                    Debug.Log("Object pools are now synced, moving to final step.");
                    break;

                case LevelCreationState.FinalStep:
                    if (FinalStep != null)
                        FinalStep(this, null);
                    m_EventsCompleted = true;
                    Debug.Log("Final step completed, game can now begin.");
                    return;
            }

            //if the master client is the one who finished the current event then that value needs to be incremented and then synced to the client
            if (PhotonNetwork.isMasterClient)
            {
                m_MasterClientState++;
                if (PhotonNetwork.inRoom)
                    photonView.RPC("RPCSyncEventState", PhotonTargets.All, true, m_MasterClientState);
            }
            //if the client is finished with the current event then that needs to be synced and the master client needs to know that the client is ready to begin the next event sequence
            else
            {
                m_ClientState++;
                if (PhotonNetwork.inRoom)
                    photonView.RPC("RPCSyncEventState", PhotonTargets.All, false, m_ClientState);
            }
        }
    }

    [PunRPC]
    //This RPC is meant to updated the states between client and master client that way they're always matching in terms of event states
    private void RPCSyncEventState(bool isMasterClient, LevelCreationState currentState)
    {
        if (isMasterClient)
            m_MasterClientState = currentState;

        else
            m_ClientState = currentState;
    }

    [PunRPC]
    private void TellPlayerYouAreReady()
    {
        if (PhotonNetwork.isMasterClient)
        {
            MenuManager.UpdateTogglesForMaster();
        }
        else
        {

            MenuManager.UpdateTogglesForClient();
        }
    }

    [PunRPC]
    private void CanStartCountdown()
    {
        if (countDownCanStart)
            countDownCanStart = false;
        else
            countDownCanStart = true;
    }

    #region Scene Destruction Management Methods
    //This method will take the object passed in and mark it to not be destoryed as well as store it, this logic is done for objects that are either spawned over the network or have a photon view
    public void DontDestroyNetworkObject(GameObject obj)
    {
        if (!m_NetworkObjects.Contains(obj))
        {
            DontDestroyOnLoad(obj);
            m_NetworkObjects.Add(obj);
        }
    }

    //This method will take the object passed in and mark it to not be destroyed as well as store it, this logic is done for none network objects
    public void DontDestroyNormalObject(GameObject obj)
    {
        if (!m_NormalObjects.Contains(obj))
        {
            DontDestroyOnLoad(obj);
            m_NormalObjects.Add(obj);
        }
    }

    //This method will destory a network object marked to not be destroyed on load
    public void DestroyNetworkObject(GameObject obj)
    {
        if (m_NetworkObjects.Contains(obj))
        {
            m_NetworkObjects.Remove(obj);
            PhotonNetwork.Destroy(obj);
        }
    }

    //This method will destory a normal object marked to not be destroyed on load
    public void DestroyNormalObject(GameObject obj)
    {
        if (m_NormalObjects.Contains(obj))
        {
            m_NormalObjects.Remove(obj);
            Destroy(obj);
        }
    }

    //This will destroy all objects in networkObjects over the network, be careful
    public void DestroyAllNetwork()
    {
        if (PhotonNetwork.isMasterClient)
            foreach (GameObject OBJ in m_NetworkObjects)
            {
                OBJ.SetActive(false);
                PhotonNetwork.Destroy(OBJ);
            }

        m_NetworkObjects.Clear();
    }

    //This will destroy all Normal objects
    public void DestroyAllNormal()
    {
        foreach (GameObject OBJ in m_NormalObjects)
        {
            OBJ.SetActive(false);
            Debug.Log(OBJ.name.ToString() + " Destroyed");
            Destroy(OBJ);
        }
        m_NormalObjects.Clear();
    }

    //This will destory all objects that are marked to not be destoryed on load but only localy. This is good for moving a single client to another scene without ruinning gameplay for another
    public void DestoryAllLocal()
    {
        //Iterate through the network object's list and destory them
        foreach (GameObject nOBJ in m_NetworkObjects)
        {
            if (nOBJ != null)
            {
                nOBJ.SetActive(false);
                Debug.Log(nOBJ.name.ToString() + " Destroyed");
                Destroy(nOBJ);
            }
        }
        m_NetworkObjects.Clear();

        DestroyAllNormal();
    }

    //This method will destroy all objects in the normal objects list as well as destroy all network objects over the network. Should only be used when:
    //a. The master client is going to move both clients back to a different scene
    //b. There is only one client and therefore they're the master client.
    //NOTE: This method will destroy all objects marked for don't destroy on load on the network. Watch when using this as you may delete object for a player who is still connected
    public void DestroyAll()
    {
        DestroyAllNetwork();
        DestroyAllNormal();
    }

    //anything that calls this should NEVER be destroyed
    public void NeverDestroy(GameObject obj)
    {
        DontDestroyOnLoad(obj);
    }
    #endregion

    #region Player Options
    //sets the player's nickname on the network
    public void SetUserNickname(string nickname)
    {
        PhotonNetwork.player.NickName = nickname;
    }

    //gets the local player's character
    public string GetLocalPlayer()
    {
        switch (m_LocalPlayerCharacter)
        {
            case GameCharacters.Samurai:
                return "Samurai";

            case GameCharacters.Cowgirl:
                return "Cowgirl";

            default:
                return string.Empty;
        }
    }

    public void SetCharacter(string gameCharacter)
    {
        //check to see the value inside of the string is equal to samurai or cowboy and set the local player equal to the right one
        if (gameCharacter == "Blademaster")
        {
            m_LocalPlayerCharacter = GameCharacters.Samurai;
        }
        else if (gameCharacter == "Gunslinger")
        {
            m_LocalPlayerCharacter = GameCharacters.Cowgirl;
        }
    }
    #endregion

    #region Level Progression
    public void ProceedToNextLevel()
    {
        photonView.RPC("RPCProceedToNextLevel", PhotonTargets.All);
    }

    [PunRPC]
    private void RPCProceedToNextLevel()
    {
        //increase the floor number
        FloorNumber++;
        Debug.Log("Automatically sync scenes equals" + PhotonNetwork.automaticallySyncScene);

        //tell the launcher to load the next scene
        if (PhotonNetwork.isMasterClient)
        {
            Launcher.Instance.LoadScene();
        }
        else
        {
            SceneManager.LoadScene(1);
        }

        ObjectPoolManager.Instance.CanSceneUnload = true;
    }

    public void ReloadCurrentLevel()
    {
        photonView.RPC("RPCReloadCurrentLevel", PhotonTargets.All);
    }

    [PunRPC]
    private void RPCReloadCurrentLevel()
    {
        //reset the floor number
        FloorNumber = 0;
        //destroy all of the gameobject that are associated with the object pools
        DestroyAll();
        //clear all of the events from the GameManager
        ClearEvents();
        if (PhotonNetwork.isMasterClient)
        {
            Launcher.Instance.LoadScene();
        }
        else
        {
            SceneManager.LoadScene(1);
        }

        ObjectPoolManager.Instance.CanSceneUnload = true;
    }

    public void QuitToLobby()
    {
        photonView.RPC("RPCQuitToLobby", PhotonTargets.All);
    }

    [PunRPC]
    private void RPCQuitToLobby()
    {
        m_StartInLobby = true;
        //reset the floor number
        FloorNumber = 0;
        //Destroy all objects marked not destroy on load making sure to remove the networked objects
        DestroyAll();
        //clear all of the events that way everything can be reset later properly
        ClearEvents();
        //reload the first scene
        if (PhotonNetwork.isMasterClient)
            Launcher.Instance.LoadScene(0);
    }

    public void QuitToMain()
    {
        // change the scene to be the main menu scene
        SceneManager.LoadScene(0);
        //force this player to leave the room
        PhotonNetwork.LeaveRoom();
        //reset the floor number
        GameManager.FloorNumber = 0;
        //destroy all of the objects marked to not be destroyed but only locally as to not ruin the other players experience
        GameManager.Instance.DestoryAllLocal();
        //clear all of the events that way everything can be reset later properly
        GameManager.Instance.ClearEvents();

    }
    #endregion

    #region Properties
    public int SeedValue { get { return m_SeedValue; } set { m_SeedValue = value; m_TranslatedSeed = value.ToString(); } }
    public string SeedString { get { return m_SeedString; } set { m_SeedString = value; m_PreTranslatedSeed = value; } }
    public string PreTranslatedSeed { get { return m_PreTranslatedSeed; } set { m_PreTranslatedSeed = value; } }
    public string TranslatedSeed { get { return m_TranslatedSeed; } set { m_TranslatedSeed = value; } }
    public static bool CountDownCanStart { get { return countDownCanStart; } set { countDownCanStart = value; } }

    #endregion

    private enum LevelCreationState
    {
        LevelGeneration = 1,
        LevelBaking = 2,
        LevelPopulation = 3,
        PlayerPopulation = 4,
        PostPlayer = 5,
        FinalizeObjectPools = 6,
        FinalStep = 7
    }
}

