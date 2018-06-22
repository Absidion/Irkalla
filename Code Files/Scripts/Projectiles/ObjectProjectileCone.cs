using UnityEngine;
using TheNegative.AI;

//Author: James
//Last edited: James 11/8/2017

public class ObjectProjectileCone : ObjectProjectile
{
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & m_LayerMask) != 0) //if the collided gameobject is on the layer provided
        {
            AITakeDamageInterface enemy = other.transform.root.GetComponent<AITakeDamageInterface>();

            if (enemy != null) //if the collided gameobject has a health component
            {
                if (PhotonNetwork.isMasterClient)
                    enemy.TakeDamage(m_ProjectileOwner, m_Damage, null, AIUtilits.GetCritMultiplier(other.gameObject));
            }
        }
    }
}
