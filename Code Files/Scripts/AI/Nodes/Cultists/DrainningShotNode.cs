using UnityEngine;

namespace TheNegative.AI.Node
{
    public class DrainningShotNode : ShootProjectileNode
    { 
        public DrainningShotNode(AI reference, int damage, GameObject prefab, GameObject fireLocation, string poolName, int poolSize) : base(reference, damage, prefab, fireLocation, poolName, poolSize) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            return ShootDrainProjectile();                
        }

        private BehaviourState ShootDrainProjectile()
        {
            //make the AI face the target
            Vector3 target = new Vector3(m_AIReference.Target.transform.position.x, m_AIReference.transform.position.y, m_AIReference.transform.position.z);
            m_AIReference.transform.LookAt(target);

            //grab the drainning shot projectile from the object pool
            GameObject drainProjectile = ObjectPoolManager.Instance.GetObjectFromNetworkPool(m_PoolName);

            if(drainProjectile != null)
            {
                ObjectProjectileDrainningShot drainningShot = drainProjectile.GetComponent<ObjectProjectileDrainningShot>();
                drainningShot.AcolyteRef = (AcolyteAI)m_AIReference;

                Vector3 direction = (m_AIReference.Target.transform.position - m_FireLocation.transform.position).normalized;

                //get the enemy number from its name by parsing the string
                int ownerNumber = -1;
                int.TryParse(m_AIReference.name, out ownerNumber);

                //add mapgeometry to the layer mask
                int layerMask = m_AIReference.TargetLayerMask;
                layerMask |= (1 << LayerMask.NameToLayer("MapGeometry"));

                //get any element status effects from the list and shoot projectile with RPC
                Status[] statuses = m_AIReference.ElementalDamage.ToArray();
                drainProjectile.GetPhotonView().RPC("FireProjectile", PhotonTargets.All, m_FireLocation.transform.position, direction, ownerNumber, layerMask, m_Damage, m_AIReference.name, statuses);
            }

            return BehaviourState.Succeed;
        }
    }

}