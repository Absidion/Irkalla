using TheNegative.AI;
using System.Collections.Generic;
using UnityEngine;

public class ObjectProjectileOverPenatrate : ObjectProjectile
{
    private List<AI> m_AttackedAI = null;           //The list of ai attacked by the object

    protected override void Awake()
    {
        base.Awake();

        m_AttackedAI = new List<AI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //if the overpen shot hits an enemy
        if (other.gameObject.tag == "Enemy")
        {
            //deal damage to the enemy that collided with it
            AI collidedAI = other.gameObject.transform.root.GetComponent<AI>();

            if (!m_AttackedAI.Contains(collidedAI))
            {
                collidedAI.TakeDamage(m_ProjectileOwner, Damage, null, AIUtilits.GetCritMultiplier(other.gameObject));
                m_AttackedAI.Add(collidedAI);
            }
        }
        else
        {
            gameObject.SetActive(false);
            m_AttackedAI.Clear();
        }
    }
}
