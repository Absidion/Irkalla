using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Daniel
//Last edited: 11/12/2017 by Josue

[RequireComponent(typeof(BoxCollider))]
public class FistCollider : SyncBehaviour
{
    private int m_Damage = 1;
    private Status[] m_ElementalDamage;

    [SyncThis]
    private bool m_CanDamage = false;

    public bool CanDamage { get { return m_CanDamage; } set { m_CanDamage = value; } }
    public int Damage { get { return m_Damage; } set { m_Damage = value; } }
    public Status[] ElementalDamage { get { return m_ElementalDamage; } set { m_ElementalDamage = value; } }

    private void OnCollisionEnter(Collision collision)
    {
        //If the thing collided with is tagged as a player get the health Component
        if (collision.collider.gameObject.tag == "Player")
        {
            if (m_CanDamage)
            {
                SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "GolemPunchImpact", transform.position);
                //Get the health component and deal the damage if a component is found
                Player player = collision.collider.gameObject.GetComponent<Player>();
                if (player.photonView.isMine)
                    player.TakeDamage(m_Damage, transform.position ,m_ElementalDamage);

                m_CanDamage = false;
            }
        }
    }
}
