using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Writer: Liam
//Last Updated: 12/17/2017

namespace TheNegative.AI.Node
{
    public class GustNode : Node
    {
        private float m_Cooldown = 15.0f;                                          //the skill cooldown
        private float m_MaxDistance = 30.0f;                                       //the skill's maximum distance that it can go
        private float m_AttackDuration = 10.0f;                                    //the duration of the AI's attack
        private float m_WindForce = 2.0f;                                          //the amount of force that the wind applies
        private float m_MasterNergSpawnPerc = 5.0f;                                //the percentage chance that instead of regular debris it's a master nerg. Value is out of 100
        private int m_AmountOfDebris = 5;                                          //the amount of debris that will be thrown out during the attack
        private GameObject m_DebrisPrefab;                                         //the debris that will be spawned over the network
               
        private int m_LayerMask;                                                 //AI's layer mask

        private float m_CooldownTimer = 0.0f;                                    //how long the skill has been active
        private float m_AttackTimer = 0.0f;                                      //the timer for telling how long the attack has been going on
        private float m_TimeSinceLastDebris = 0.0f;                              //how long since debris was last thrown out

        private Vector3 m_StartPosFromAI = Vector3.zero;                         //the position that the attack started from relative to the AI
        private Vector3 m_GustCenterLocation = Vector3.zero;                     //the center location of the gust hit box
        private Vector3 m_Direction = Vector3.zero;                              //the direction the wind is blowing
        private Vector3 m_HitBoxSize = Vector3.zero;                             //the size of the gust hitbox
        private Quaternion m_GustRotation;                                       //the rotation of the gust's hitbox

        private const string m_DebrisPoolName = "DebrisPool";

        public GustNode(AI reference, GameObject debrisPrefab) : base(reference)
        {
            m_DebrisPrefab = debrisPrefab;

            m_HitBoxSize.x = m_MaxDistance;
            m_HitBoxSize.y = m_MaxDistance;
            m_HitBoxSize.z = m_MaxDistance;

            //init the layer mask
            m_LayerMask = LayerMask.GetMask("Player");

            //create a new pool for the debris
            ObjectPoolManager.Instance.CreateNetworkPoolWithName(m_DebrisPoolName, "Attacks/" + m_DebrisPrefab.name, false);
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (m_CooldownTimer > m_Cooldown)
            {
                return ApplyGustAffect();
            }
            else
            {
                m_CooldownTimer += Time.deltaTime;

                if(m_CooldownTimer > m_Cooldown)
                {
                    CalculateGustLocation();
                }                
            }

            return BehaviourState.Failed;
        }

        //Applys the gust effects continuously until complete
        private BehaviourState ApplyGustAffect()
        {
            //first we must check if any player is in the collision box of the AI's attack
            Collider[] collisions = Physics.OverlapBox(m_GustCenterLocation, m_HitBoxSize * 0.5f, m_GustRotation, m_LayerMask);

            //loop through the colliders and check apply force to the object
            foreach (Collider collide in collisions)
            {
                Player player = collide.GetComponent<Player>();

                if (player != null)
                {
                    Vector3 force = m_WindForce * m_Direction.normalized;
                    force.y = 0.0f;
                    player.ApplyForceOverNetwork(force, ForceMode.Impulse);
                }
            }

            //check if we will spawn random debris this frame
            if (m_TimeSinceLastDebris >= (m_AttackDuration / m_AmountOfDebris))
            {
                float randomValue = UnityEngine.Random.Range(0.0f, 100.0f);

                //create a random location for the debris to be located at
                Vector3 randomStartLoc = m_AIReference.transform.position;
                Vector3 debrisDirection = (m_StartPosFromAI - m_AIReference.transform.position).normalized;

                if (randomValue < m_MasterNergSpawnPerc)
                {
                    //spawn master nerg
                    GameObject obj = PhotonNetwork.Instantiate("MasterNerg", randomStartLoc, Quaternion.identity, 0);
                    obj.GetComponent<AI>().SetActive(true, randomStartLoc, Quaternion.identity.eulerAngles, debrisDirection, 5);
                }
                else
                {
                    GameObject obj = ObjectPoolManager.Instance.GetObjectFromNetworkPool(m_DebrisPoolName);

                    if (obj != null)
                    {
                        //activate the object projectile single target scripted object
                        obj.GetComponent<ObjectProjectileSingleTarget>().photonView.RPC("FireProjectile", PhotonTargets.All, randomStartLoc, debrisDirection, int.Parse(m_AIReference.name), m_LayerMask, 5, null);
                    }
                }

                m_TimeSinceLastDebris = 0.0f;
            }
            //otherwise increment the timer and return
            else
            {
                m_TimeSinceLastDebris += Time.deltaTime;
            }

            if (m_AttackTimer >= m_AttackDuration)
            {
                return BehaviourState.Succeed;
            }
            else
            {
                m_AttackTimer += Time.deltaTime;
            }

            return BehaviourState.Running;
        }

        private void CalculateGustLocation()
        {
            //calcaulte the direction to the target
            m_Direction = m_AIReference.Target.transform.position - m_AIReference.transform.position;
            //set the direction's y position to be on the same plane as the target
            m_Direction.y = m_AIReference.Target.transform.position.y;
            //set the start location of the gust
            m_StartPosFromAI = m_AIReference.Target.transform.position;
            m_StartPosFromAI.y = m_Direction.y;

            //calculate the center position of the attack
            Vector3 normalizedDir = m_Direction.normalized;
            //divid the maxdistance by two and multiply it by the normalized direction. Add it on to the direction value to get the center 
            m_GustCenterLocation = (normalizedDir * (m_MaxDistance * 0.5f)) + m_AIReference.transform.position;

            m_GustRotation = Quaternion.LookRotation(m_Direction, m_AIReference.transform.up);
        }

        public override void Reset()
        {
            m_AttackTimer = 0.0f;
            m_TimeSinceLastDebris = 0.0f;
        }
    }
}
