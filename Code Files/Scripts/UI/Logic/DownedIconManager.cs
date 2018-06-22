using UnityEngine;
using UnityEngine.UI;

public class DownedIconManager : MonoBehaviour
{
    public Image DownIcon;

    private bool m_IsOtherPlayerDowned = false;

    private void Update()
    {
        if (PhotonNetwork.room.PlayerCount > 1)
        {
            if (PlayerHUDManager.instance.OtherPlayer != null)
            {
                if (!m_IsOtherPlayerDowned && PlayerHUDManager.instance.OtherPlayer.IsDowned &&
                    PlayerHUDManager.instance.OtherPlayer.MyIslandRoom == PlayerHUDManager.instance.LocalPlayer.MyIslandRoom)
                {
                    Vector3 newPos = PlayerHUDManager.instance.OtherPlayer.transform.position;
                    newPos.y += 1.0f;
                    DownIcon.gameObject.transform.position = newPos;
                    DownIcon.gameObject.SetActive(true);

                    m_IsOtherPlayerDowned = true;
                }
                else if ((m_IsOtherPlayerDowned && !PlayerHUDManager.instance.OtherPlayer.IsDowned) ||
                          PlayerHUDManager.instance.OtherPlayer.MyIslandRoom != PlayerHUDManager.instance.LocalPlayer.MyIslandRoom)
                {
                    DownIcon.gameObject.SetActive(false);

                    m_IsOtherPlayerDowned = false;
                }

                if (DownIcon.gameObject.activeInHierarchy)
                {
                    //set pos
                    Vector3 newPos = PlayerHUDManager.instance.OtherPlayer.transform.position;
                    newPos.y += 1.0f;
                    DownIcon.gameObject.transform.position = newPos;

                    //set rotation
                    DownIcon.transform.LookAt(PlayerHUDManager.instance.LocalPlayer.transform);
                    DownIcon.transform.Rotate(new Vector3(0.0f, 180.0f, 0.0f));

                    //set scale
                    float scale = Vector3.Magnitude(Camera.main.transform.position - DownIcon.transform.position) / 5.0f;
                    DownIcon.transform.localScale = new Vector3(scale, scale, scale);
                }
            }
        }
    }
}
