using System;
using System.Collections;
using UnityEngine;

namespace TheNegative.AI.Node
{
    public class GroundSlamNode : Node
    {
        private int m_AttackDamage = 10;                        //the amount of damage that the attack does
        private float m_GroundSlamWidth = 7.0f;                 //the radius at which the attack will get used
        private float m_GroundSlamHeight = 3.0f;
        private int m_NumberOfSpawnedRocks = 5;                 //the number of rocks that will fall during the falling section of the attack
        private GameObject m_FallingRockPrefab;                 //the rock prefab
        private Transform m_RockSpawnLocation;                  //spawn location of rocks

        private const string m_FallingRockPoolName = "FallingRocks";


        public GroundSlamNode(AI reference, GameObject flyingRockPrefab, Transform rockSpawnLocation) : base(reference)
        {
            m_FallingRockPrefab = flyingRockPrefab;
            m_RockSpawnLocation = rockSpawnLocation;
            ObjectPoolManager.Instance.CreateNetworkPoolWithName(m_FallingRockPoolName, "Attacks/" + m_FallingRockPrefab.name, 10, 100, false);
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            return ActivateSlam();
        }

        public BehaviourState ActivateSlam()
        {
            //get spawn location slightly offset from 
            Vector3 spawnLocation = m_AIReference.transform.position;
            spawnLocation += m_RockSpawnLocation.right * 1.0f;
            spawnLocation.y += 0.5f;

            //set scale of hit box 
            Vector3 colliderScale = new Vector3(m_GroundSlamWidth, m_GroundSlamHeight, 1.0f);

            //create hit box and if any players are caught in it, deal damage
            Collider[] colliders = Physics.OverlapBox(m_RockSpawnLocation.position, colliderScale, m_RockSpawnLocation.rotation, m_AIReference.TargetLayerMask);
            if (colliders.Length > 0)
            {
                foreach (Collider collider in colliders)
                {
                    Player p = collider.GetComponent<Player>();

                    if (p != null)
                    {
                        Status[] statuses = { Status.Stun };
                        p.photonView.RPC("TakeDamage", PhotonTargets.All, m_AttackDamage, m_AIReference.transform.position ,statuses);
                    }
                }
            }

            //spawn rocks from location and set them flying in random directions
            for (int i = 0; i < m_NumberOfSpawnedRocks; i++)
            {
                GameObject flyingRock = ObjectPoolManager.Instance.GetObjectFromNetworkPool(m_FallingRockPoolName);
                SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "GolemGroundSlamImpact", m_AIReference.transform.position);
                if (flyingRock != null)
                {
                    //set max distance/height of destination of point and randomly pick between 1 and max
                    float maxDistance = 7.0f;
                    float maxHeight = 7.0f;

                    Vector3 destinationPoint = m_AIReference.transform.position;
                    destinationPoint += m_AIReference.transform.forward * UnityEngine.Random.Range(3.0f, maxDistance);
                    destinationPoint += m_AIReference.transform.up * UnityEngine.Random.Range(3.0f, maxHeight);

                    Vector3 dir = (destinationPoint - spawnLocation).normalized;    //get direction to destination point
                    float angle = UnityEngine.Random.Range(-30.0f, 30.0f);          //get random angle to rotate direction by between 60 degrees radial area
                    dir = Quaternion.Euler(0.0f, angle, 0.0f) * dir;                //rotate original direction by new random angle
                    float force = 10.0f;                                            //get random force to apply to projectile

                    Status[] statuses = { Status.Stun };
                    flyingRock.GetPhotonView().RPC("FireProjectile", PhotonTargets.All, spawnLocation, dir, -1, m_AIReference.TargetLayerMask, m_AttackDamage / 2, force, statuses);
                }
            }

            return BehaviourState.Succeed;
        }
    }
}