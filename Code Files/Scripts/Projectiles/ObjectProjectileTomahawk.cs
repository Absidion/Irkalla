using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheNegative.AI;

//Author: Josue
//Date: 01/28/2018

public class ObjectProjectileTomahawk : ObjectProjectile
{
    public GameObject Mesh;

    private int m_NumberOfSurfaceBounces = 0;               //when number of bounces reaches 2 on either value, the tomahawk will return to the player
    private int m_NumberOfEnemyBounces = 0; 
    private float m_DamageMultiplier = 1.0f;                //Multiplier keeps incrementing over time to a maximum multiplier of 1.9

    private Transform m_Target = null;                      //Reference to current target the tomahawk is flying towards
    private bool m_LerpingToEnemy = false;                  //if true, lerps projectile towards current target
    private bool m_LerpingToPlayer = false;                 //if true, lerps projectile back to player
    private float m_CollisionTimer = 0.0f;
    private AITakeDamageInterface m_LastHitEnemy = null;    //last enemy hit by the tomahawk

    protected override void Update()
    {
        base.Update();

        m_DamageMultiplier = Mathf.Clamp(m_DamageMultiplier + Time.deltaTime / 5, 1.0f, 2.0f);

        Mesh.transform.localEulerAngles = new Vector3(Mesh.transform.localEulerAngles.x, Mesh.transform.localEulerAngles.y, Mesh.transform.localEulerAngles.z - Time.deltaTime * 600);

        if (m_LerpingToEnemy || m_LerpingToPlayer)
        {
            if (m_LerpingToEnemy && m_Target == null)
                ReturnToPlayer();

            if (rigidbody.velocity != Vector3.zero)
                rigidbody.velocity = Vector3.zero;

            gameObject.transform.LookAt(m_Target.position);
            gameObject.transform.position += gameObject.transform.forward * ShotSpeed * Time.deltaTime;
            //Debug.Log((m_Target.position - gameObject.transform.position).magnitude);

            if ((m_Target.position - gameObject.transform.position).magnitude < 0.1f && m_LerpingToPlayer)
            {
                Reset();

                Cowgirl cg = (Cowgirl)m_Player;
                cg.HasTomahawk = true;

                if (m_Player.WeaponType == WeaponType.MELEE)
                    m_Player.MeleeWeapon.SetActive(true);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (m_LerpingToEnemy && collision.gameObject.tag != "Enemy")
        {
            return;
        }

        m_LerpingToEnemy = false;

        if (collision.gameObject.tag == "Enemy" && m_NumberOfEnemyBounces < 2)
        {
            AITakeDamageInterface enemy = collision.transform.root.GetComponent<AITakeDamageInterface>();

            if (enemy != null)
            {
                if (m_LastHitEnemy != enemy)
                {
                    m_LastHitEnemy = enemy;

                    if (PhotonNetwork.isMasterClient)
                        enemy.TakeDamage(m_ProjectileOwner, Mathf.RoundToInt(m_Damage * m_DamageMultiplier), m_StatusEffects, AIUtilits.GetCritMultiplier(collision.gameObject));
                }
                else
                {
                    //rigidbody.velocity = Vector3.zero;
                    //rigidbody.AddForce(-collision.contacts[0].normal * ShotSpeed, ForceMode.VelocityChange);
                    //return;
                }
            }

            m_NumberOfEnemyBounces++;

            if (m_NumberOfEnemyBounces < 2)
                BounceTomahawk(collision);
            else
                ReturnToPlayer();
        }
        else if (collision.gameObject.tag != "Enemy" && m_NumberOfSurfaceBounces < 2)
        {
            m_NumberOfSurfaceBounces++;
            SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "TomahawkImpact", collision.gameObject.transform.position);

            if (m_NumberOfSurfaceBounces < 2)
                BounceTomahawk(collision);
            else
                ReturnToPlayer();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        m_CollisionTimer += Time.deltaTime;

        if (m_CollisionTimer > 0.1f && !m_LerpingToEnemy)
        {
            if (collision.gameObject.tag != "Enemy" && m_NumberOfSurfaceBounces < 2)
            {
                m_NumberOfSurfaceBounces++;

                if (m_NumberOfSurfaceBounces < 2)
                    BounceTomahawk(collision);
                else
                    ReturnToPlayer();
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        m_CollisionTimer = 0.0f; 
    }

    private void Reset()
    {
        m_NumberOfEnemyBounces = 0;
        m_NumberOfSurfaceBounces = 0;
        m_DamageMultiplier = 1.0f;
        m_LerpingToEnemy = false;
        m_LerpingToPlayer = false;
        m_LastHitEnemy = null;
        gameObject.GetComponent<BoxCollider>().enabled = true;
        gameObject.SetActive(false);
    }

    private void ReturnToPlayer()
    {
        gameObject.GetComponent<BoxCollider>().enabled = false;
        m_Target = m_Player.transform;
        m_LerpingToPlayer = true;
    }

    private void BounceTomahawk(Collision collision)
    {
        rigidbody.velocity = Vector3.zero;

        Vector3 bounceDirection = Vector3.Project(-gameObject.transform.forward, collision.contacts[0].normal);

        RaycastHit[] contacts = Physics.SphereCastAll(gameObject.transform.position, 10.0f, bounceDirection, Mathf.Infinity, m_LayerMask);
        if (contacts.Length > 0)
        {
            float smallestAngle = float.MaxValue;

            foreach (RaycastHit enemy in contacts)
            {
                float angleBetween = Vector3.Angle(gameObject.transform.position, enemy.transform.position);
                if (angleBetween < smallestAngle)
                {
                    smallestAngle = angleBetween;

                    if (m_Target != enemy.transform && !enemy.collider.isTrigger && enemy.transform.gameObject != collision.gameObject)
                    {
                        m_Target = enemy.transform;
                        m_LerpingToEnemy = true;
                    }
                }
            }
        }

        if (!m_LerpingToEnemy)
        {
            Vector3 forceVector = bounceDirection.normalized * ShotSpeed;
            
            if (forceVector == Vector3.zero)
            {
                forceVector.x = 1.0f * ShotSpeed;
            }

            rigidbody.AddForce(forceVector, ForceMode.VelocityChange);
        }
    }
}
