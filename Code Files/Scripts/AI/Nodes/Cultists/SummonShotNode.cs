using System;
using UnityEngine;

//Author: Josue
//Last edited: 1/04/2018

namespace TheNegative.AI.Node
{
    public class SummonShotNode : ShootProjectileNode
    {
        public SummonShotNode(AI reference, int damage, GameObject prefab, GameObject fireLocation, string poolName, int poolSize): 
            base(reference, damage, prefab, fireLocation, poolName, poolSize)
        {

        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            return ShootSummonProjectile();
        }

        private BehaviourState ShootSummonProjectile()
        {
            //turn to look at enemy
            Vector3 target = new Vector3(m_AIReference.Target.transform.position.x, m_AIReference.transform.position.y, m_AIReference.Target.transform.position.z);
            m_AIReference.transform.LookAt(target); //rotate towards the target

            SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "WorshipperShoot", m_AIReference.transform.position);

            //grab projectile from pool to fire
            GameObject projectile = ObjectPoolManager.Instance.GetObjectFromNetworkPool(m_PoolName);

            if (projectile != null)
            {
                Vector3 direction = (m_AIReference.Target.transform.position - m_FireLocation.transform.position).normalized;

                //get the enemy number from its name by parsing the string
                int ownerNumber = -1;
                Int32.TryParse(m_AIReference.gameObject.name, out ownerNumber);

                //add wall/stairs/door to layer mask
                //using a bitwise operation to add these to the layer mask as the default enemy layer mask only contains Player 
                int layerMask = m_AIReference.TargetLayerMask;
                layerMask |= (1 << LayerMask.NameToLayer("Default"));
                layerMask |= (1 << LayerMask.NameToLayer("Stairs"));
                layerMask |= (1 << LayerMask.NameToLayer("Door"));
                layerMask |= (1 << LayerMask.NameToLayer("MapGeometry"));

                //get any elemental status effects from the list and shoot projectile with RPC
                Status[] statuses = m_AIReference.ElementalDamage.ToArray();
                projectile.GetPhotonView().RPC("FireProjectile", PhotonTargets.All, m_FireLocation.transform.position, direction, ownerNumber, layerMask, m_Damage, m_AIReference.name,statuses);
            }

            return BehaviourState.Succeed;
        }
    }
}
