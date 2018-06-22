using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Josue
//Last edited: Josue 11/10/2017
public class GroundSpike : LiamBehaviour
{
    private int m_Damage = 10;          //damage of the spike when it hits
    private bool m_IsMovingUp = true;   //is this spike moving up or down
    private float m_LerpTimer = 0.0f;   //timer incremements while the spikes are moving
    private Health m_AIHealth = null;   //if the AI health is 0 then spikes are all killed
    private int m_LayerMask;            //spike will do damage when it hits player layer

    protected override void Awake()
    {
        base.Awake();
        
        m_LayerMask = LayerMask.GetMask("Player");   
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & m_LayerMask) != 0)
        {
            Player player = other.gameObject.GetComponent<Player>();
            if (player.photonView.isMine)
                player.TakeDamage(m_Damage, transform.position ,null);
        }
    }

    public void ToggleSpike(bool flag)
    {
        SetActive(flag);
    }

    public void ResetValues()
    {
        photonView.RPC("NetworkResetValues", PhotonTargets.All);
    }

    [PunRPC]
    private void NetworkResetValues()
    {
        m_IsMovingUp = true;
        m_LerpTimer = 0.0f;
    }

    public bool IsMovingUp { get { return m_IsMovingUp; } set { m_IsMovingUp = value; } }
    public float LerpTimer { get { return m_LerpTimer; } set { m_LerpTimer = value; } }
    public Health AIHealth { get { return m_AIHealth; } set { m_AIHealth = value; } }
}
