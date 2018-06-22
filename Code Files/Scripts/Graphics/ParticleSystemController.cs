using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemController : LiamBehaviour {
    private ParticleSystem   m_ParticleSystem;
    private Vector3          m_LocalRotVelo;
    private bool             m_IsActive = false;

    public ParticleSystem ParticleSystem { get { return m_ParticleSystem; } set { m_ParticleSystem = value; } }
    public bool IsActive { get { return m_IsActive; } set { m_IsActive = value; } }

    override protected void Awake()
    {
        base.Awake();

        m_ParticleSystem = GetComponent<ParticleSystem>();
        m_LocalRotVelo = Vector3.zero;
    }

    private void LateUpdate()
    {
        if(m_ParticleSystem.IsAlive() == false)
        {
            gameObject.SetActive(false);
        }

        if(m_LocalRotVelo != Vector3.zero)
        {
            Vector3 frameStep = m_LocalRotVelo * Time.fixedDeltaTime;
            transform.Rotate(frameStep);
        }
    }

    //Takes a transfrom and fires the particle System 
    public void FireFromPosition(Vector3 pos)
    {
        transform.position = pos;
        StartParticleSystem();
    }

    //Takes a transfrom and fires the particle System 
    [PunRPC] 
    public void FireAtPosRPC(float x, float y ,float z)
    {
        transform.position = new Vector3(x,y,z);
        StartParticleSystem();
    }

    //use this if the netowrking doesnt work
 
    public void FireFromPositionNetworked(Vector3 pos)
    {
        float x = pos.x;
        float y = pos.y;
        float z = pos.z;
        photonView.RPC("FireAtPosRPC",PhotonTargets.All, x, y, z);
    }

    public void StartParticleSystem()
    {
        photonView.RPC("NetworkActivate", PhotonTargets.All);
    }

    [PunRPC]
    public void NetworkActivate()
    {
        m_IsActive = true;
        gameObject.SetActive(true);
        m_ParticleSystem.Play();
    }

    public void OfflineActivate()
    {
        m_IsActive = true;
        gameObject.SetActive(true);
        m_ParticleSystem.Play();
    }


    public void StopParticleSystem()
    {
        photonView.RPC("NetworkDectivate", PhotonTargets.All);
    }

    [PunRPC]
    public void NetworkDectivate()
    {
        m_IsActive = false;
        m_ParticleSystem.Stop();
        gameObject.SetActive(false);
    }

    public void OfflineDectivate()
    {
        m_IsActive = false;
        m_ParticleSystem.Stop();
        gameObject.SetActive(false);
    }
}
