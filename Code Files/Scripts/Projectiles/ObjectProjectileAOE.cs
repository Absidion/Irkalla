using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheNegative.AI;

//Author: Josue
//Last edited: 10/31/2017

public class ObjectProjectileAOE : ObjectProjectile
{
    public float Radius = 1.0f;                       //the area in which enemies can be damaged in

    private List<AITakeDamageInterface> m_EnemiesHit; //stores enemies hit already by the explosion so duplicate damage isn't done
    private bool m_CanCollide = true;

    protected override void Awake()
    {
        base.Awake();

        m_EnemiesHit = new List<AITakeDamageInterface>();
    }

    protected override void Update()
    {
        base.Update();

        if (!m_CanCollide)
        {
            m_CanCollide = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!m_CanCollide)
            return;
        SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "GrenadeExplosion", transform.position);
        //instantiate explosion render
        GameObject explosionRenderer = (GameObject)(Instantiate(Resources.Load("Effects/ExplosionRenderer"), gameObject.transform.position, Quaternion.identity));
        ExplosionEffect explosionEffect = explosionRenderer.GetComponent<ExplosionEffect>();
        explosionEffect.ActivateExplosion(Radius);

        m_CanCollide = false;
        gameObject.SetActive(false); //return object to pool

        Collider[] colliders; //stores all colliders that were hit
        m_EnemiesHit.Clear();

        colliders = Physics.OverlapSphere(gameObject.transform.position, Radius, m_LayerMask);

        //if we collided with any objects of our specific target layer mask then they take damage
        for (int i = 0; i < colliders.Length; i++)
        {
            AITakeDamageInterface enemy = colliders[i].transform.root.GetComponent<AITakeDamageInterface>();

            if (enemy != null && !m_EnemiesHit.Contains(enemy)) //if the collided object has a health component
            {
                m_EnemiesHit.Add(enemy);

                if (PhotonNetwork.isMasterClient)
                {                    
                    enemy.TakeDamage(m_ProjectileOwner, m_Damage, null, AIUtilits.GetCritMultiplier(colliders[i].gameObject));
                }
            }
        }
    }
}
