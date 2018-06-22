using System;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PlayerManager : SyncBehaviour
{
    public static PlayerManager Instance;
    public static string PlayerType = "Cowgirl";

    private Player m_LocalPlayer;
    private Player m_OtherPlayer;

    protected override void Awake()
    {
        if (Instance == null)
        {
            base.Awake();
            Instance = this;
            GameManager.PlayerPopulation += SpawnPlayer;
            GameManager.PostPlayer += FindPlayers;
        }
        else
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    #region Event Calls
    private void SpawnPlayer(object sender, EventArgs args)
    {
        if (!PhotonNetwork.player.IsLocal)
            return;

        //get the player number from the photon player's custom properties
        int playerNumber = (int)PhotonNetwork.player.CustomProperties["PlayerNumber"];

        //find a player spawn point who's name is assosiated with the photon player's number
        PlayerSpawnPoint[] spawnPositions = FindObjectsOfType<PlayerSpawnPoint>();
        PlayerSpawnPoint playerSpawn = null;

        foreach (PlayerSpawnPoint spawnPos in spawnPositions)
        {
            if (spawnPos.SpawnPointNumber == playerNumber)
                playerSpawn = spawnPos;
        }

        if (m_LocalPlayer == null)
        {
            //get the character that needs to be spawned for this local player
            PlayerType = GameManager.Instance.GetLocalPlayer();

            GameObject obj = PhotonNetwork.Instantiate("Players/" + PlayerType, playerSpawn.transform.position, playerSpawn.transform.rotation, 0);
            Player player = obj.GetComponent<Player>();
            player.SetName(PhotonNetwork.player.NickName);
            player.photonView.RPC("SyncPlayerNumber", PhotonTargets.All, playerNumber);
            m_LocalPlayer = player;
        }
        else
        {
            m_LocalPlayer.transform.position = playerSpawn.transform.position;
            m_LocalPlayer.transform.rotation = playerSpawn.transform.rotation;
        }
    }

    private void FindPlayers(object sender, EventArgs args)
    {
        Player[] players = FindObjectsOfType<Player>();

        foreach (Player player in players)
        {
            if (player.photonView.isMine)
            {
                m_LocalPlayer = player;
            }
            else
            {
                m_OtherPlayer = player;
            }
        }
    }
    #endregion

    #region Player Death Managment
    public void OnPlayerDeath(int playerNumber)
    {
        Player currentPlayer = null;
        //if we are currently dealing with the Local Player
        if (m_LocalPlayer.PlayerNumber == playerNumber)
            currentPlayer = m_LocalPlayer;
        //we are dealing the other non local player
        else
            currentPlayer = m_OtherPlayer;

        //check to see if the extra lives counter is greater then 0 and if it is resurrect the player
        if (currentPlayer.GetStat(StatType.EXTRALIFE) > 0)
        {
            currentPlayer.ResurrectPlayer();
            return;
        }

        //enable the death canvas for the player
        PlayerHUDManager.instance.DeathCanvas.gameObject.SetActive(true);

        if (m_OtherPlayer != null)
        {
            if (m_LocalPlayer.Health.IsDead && m_OtherPlayer.Health.IsDead)
            {
                //call the RPC method to invoke the gameovercanvas on both player's screen
                photonView.RPC("RPCSetGameoverScreenEnabled", PhotonTargets.All);
            }
        }
        else if (m_OtherPlayer == null)
        {
            //call the RPC method to invoke the gameovercanvas on both player's screen
            photonView.RPC("RPCSetGameoverScreenEnabled", PhotonTargets.All);
        }
    }

    [PunRPC]
    private void RPCSetGameoverScreenEnabled()
    {
        //enabled the gameover canvas
        PlayerHUDManager.instance.GameOverCanvas.gameObject.SetActive(true);
    }

    #endregion
}

public enum GameCharacters
{
    Samurai,
    Cowgirl
}