using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Josue
//Last edited: Josue 11/10/2017

public class ObjectProjectileSingleTarget : ObjectProjectile
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Reflector" || m_IsBeingReflected)
        {
            m_IsBeingReflected = false;
            return;
        }

        gameObject.SetActive(false);

        if (((1 << collision.gameObject.layer) & m_LayerMask) != 0) //if the collided gameobject is on the layer provided
        {
            if (collision.gameObject.tag == "Player")
            {
                Player playerRef = collision.gameObject.GetComponent<Player>();
                if (playerRef.photonView.isMine)
                    playerRef.TakeDamage(m_Damage, transform.position, m_StatusEffects);
            }
        }
    }
}
