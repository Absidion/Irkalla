using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularAOE : LiamBehaviour
{
    public Projector Projector;             //circular radius projected on the ground to show range of attack
    public ParticleSystem ParticleEmitter;  //particle emitter to give acompanying visual effect
    public SphereCollider Collider;         //damage collider that gets set active after the delay
    public float TimeActive = 1.0f;         //amount of time this AOE will remain active in the world
    public int Damage = 1;                  //amount of damage done per interval
    public float DamageInterval = 0.2f;     //how much time in seconds must pass before damage is dealt

    private float m_Timer = 0.0f;           //when this timer is over the time active given, destroys the AOE 
    private float m_TimeInside = 0.0f;      //this increments while the player is in the AOE. when it is over the damage interval time, apply damage
    private bool m_ShouldDamage = false;    //set to true when the local player is inside the AOE
    private Player m_LocalPlayer = null;    //reference to the local player
	
	protected override void Update ()
    {
        base.Update();

        //update active timer 
        m_Timer += Time.deltaTime;

        //disable AOE attack after time has passed
        if (m_Timer >= TimeActive)
        {
            Reset();
        }

        //if can damage, increment timer and when it passes interval apply damage
        if (m_ShouldDamage)
        {
            m_TimeInside += Time.deltaTime;

            if (m_TimeInside >= DamageInterval)
            {
                m_TimeInside = 0.0f;
                m_LocalPlayer.TakeDamage(Damage, transform.position ,null);
            }
        }
	}

    public void OnTriggerEnter(Collider other)
    {
        Player p = other.GetComponent<Player>();

        if (p != null)
        {
            //if the player that collided is mine, and local player isn't set yet
            if (p.photonView.isMine && m_LocalPlayer == null)
            {
                m_LocalPlayer = p;
                m_ShouldDamage = true;
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        Player p = other.GetComponent<Player>();

        if (p != null)
        {
            if (p.photonView.isMine)
            {
                m_ShouldDamage = false;
                m_LocalPlayer = null;
            }
        }
    }

    public void SetProjectorActive(bool flag)
    {
        photonView.RPC("RPCSetProjectorActive", PhotonTargets.All, flag);
    }

    public void SetAOEActive(bool flag)
    {
        photonView.RPC("RPCSetAOEActive", PhotonTargets.All, flag);
    }

    public void SetAOEPos(Vector3 pos)
    {
        photonView.RPC("RPCSetAOEPos", PhotonTargets.All, pos.x, pos.y, pos.z);
    }

    [PunRPC]
    public void RPCSetProjectorActive(bool flag)
    {
        Projector.gameObject.SetActive(flag);
    }

    [PunRPC]
    public void RPCSetAOEActive(bool flag)
    {
        ParticleEmitter.gameObject.SetActive(flag);
        Collider.enabled = flag;
    }

    [PunRPC]
    public void RPCSetAOEPos(float x, float y, float z)
    {
        Vector3 newPos = new Vector3(x, y, z);
        gameObject.transform.position = newPos;
    }

    private void Reset()
    {
        ParticleEmitter.gameObject.SetActive(false);
        Collider.enabled = false;
        Projector.gameObject.SetActive(false);
        gameObject.SetActive(false);
        m_Timer = 0.0f;
        m_TimeInside = 0.0f;
        m_ShouldDamage = false;
        m_LocalPlayer = null;
    }
}
