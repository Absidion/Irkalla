using System.Collections.Generic;
using UnityEngine;

public class LevelTransitionPortal : MonoBehaviour
{
    private List<Player> m_PlayersReadyToTransition;

	void Awake ()
    {
        m_PlayersReadyToTransition = new List<Player>();
        gameObject.SetActive(false);
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Player playerComp = other.GetComponent<Player>();
            if(!m_PlayersReadyToTransition.Contains(playerComp))
            {
                m_PlayersReadyToTransition.Add(playerComp);
                PlayerHUDManager.instance.ToggleLevelTransition(true);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(m_PlayersReadyToTransition.Count == PhotonNetwork.playerList.Length)
        {
            PlayerHUDManager.instance.ToggleLevelTransition(false);
            if (PhotonNetwork.isMasterClient)
                GameManager.Instance.ProceedToNextLevel();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            Player playerComp = other.GetComponent<Player>();
            if (m_PlayersReadyToTransition.Contains(playerComp))
            {
                m_PlayersReadyToTransition.Remove(playerComp);
                PlayerHUDManager.instance.ToggleLevelTransition(false);
            }
        }
    }
}
