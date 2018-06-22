using System;
using UnityEngine;

//Author: Josue
//Last edited: 1/04/2017

namespace TheNegative.AI.Node
{
    public class RockThrowNode : Node
    {
        #region Variables/Properties/Constructor

        private GameObject m_RockPrefab;                   //prefab for rock throw
        private GameObject m_FireLocation;                 //where the rock will fir from
        private int m_Damage = 1;                          //how much damage the rock does

        private const string m_RockProjectile = "RockProjectile";

        public RockThrowNode(AI reference, GameObject prefab, GameObject fireLocation, int damage) : base(reference)
        {
            m_RockPrefab = prefab;
            m_FireLocation = fireLocation;
            m_Damage = damage;

            ObjectPoolManager.Instance.CreateNetworkPoolWithName(m_RockProjectile, "Attacks/" + m_RockPrefab.name, 5, 20, false);
        }

        #endregion

        #region Functions
        public override BehaviourState UpdateNodeBehaviour()
        {
            return ThrowRock();
        }

        private BehaviourState ThrowRock()
        {
            SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "GolemRockThrow", m_AIReference.transform.position);
            Vector3 target = new Vector3(m_AIReference.Target.transform.position.x, m_AIReference.transform.position.y, m_AIReference.Target.transform.position.z);
            m_AIReference.transform.LookAt(target); //rotate towards the target

            //grab projectile from pool to fire
            GameObject projectile = ObjectPoolManager.Instance.GetObjectFromNetworkPool(m_RockProjectile);

            if (projectile != null)
            {
                Vector3 direction = (m_AIReference.Target.transform.position - m_FireLocation.transform.position).normalized;

                //set up array with statuses to apply including stun
                int elementListCount = m_AIReference.ElementalDamage.Count;                      //get size of elemental types list
                Status[] statuses = new Status[elementListCount + 1];                            //create new array with size of the AI status count + 1 to add stun

                Array.Copy(m_AIReference.ElementalDamage.ToArray(), statuses, elementListCount); //copy the element type list as an array into new array
                statuses[elementListCount] = Status.Stun;                                        //add stun to the list of statuses to be applied

                //call projectile RPC to fire the rock at target
                projectile.GetPhotonView().RPC("FireProjectile", PhotonTargets.All, m_FireLocation.transform.position, direction, -1, m_AIReference.TargetLayerMask, m_Damage, statuses);
            }

            return BehaviourState.Succeed;
        }

        #endregion
    }
}