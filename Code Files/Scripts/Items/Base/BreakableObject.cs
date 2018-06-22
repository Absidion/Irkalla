using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObject : MonoBehaviour, AITakeDamageInterface
{
    public List<Mesh> m_DamageStates; //List of all the Damage States
    private MeshFilter m_MeshFilter; //The meshfilter used to swap the mesh
    private PhotonView m_PhotonView; //For calling RPCs over the network 
    private int m_CurrentMeshIndex; //Current Index the mesh is on
	// Use this for initialization
	void Start () {
        m_PhotonView = GetComponent<PhotonView>();
        m_MeshFilter = GetComponent<MeshFilter>();
        ResetBreakable();
    }

    public void ResetDamageState()
    {
        m_PhotonView.RPC("ResetBreakable", PhotonTargets.All);
    }

    [PunRPC]
    public void ResetBreakable()
    {
        //Reset the vars
        m_CurrentMeshIndex = 0;
        m_MeshFilter.mesh = m_DamageStates[m_CurrentMeshIndex];
    }

    [PunRPC]
    public void ChangeDamageState()
    {
        //Increment the CurrentMeshIndex and set the next mesh state
        m_CurrentMeshIndex += 1;
        m_MeshFilter.mesh = m_DamageStates[m_CurrentMeshIndex];
    }


    public void TakeDamage(int playerNumber, int damage, Status[] statusEffects, int multiplier)
    {
        //If the number is past the number of Damage states return
        if (m_CurrentMeshIndex + 1 > m_DamageStates.Count)
            return;

        m_PhotonView.RPC("ChangeDamageState", PhotonTargets.All);
    }

    private void OnTriggerEnter(Collider other)
    {
        //If a projectile hits the breakable object check its tpye an break it when the proper object hits it
        GameObject obj = other.gameObject;
        if (other.tag == "Untagged" || other.tag.Contains("Projecile"))
        {
            if (m_CurrentMeshIndex + 1 > m_DamageStates.Count)
                return;

            m_PhotonView.RPC("ChangeDamageState", PhotonTargets.All);
        }
    }
}
