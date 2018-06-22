using UnityEngine;

//Author: Josue
//Last edited: Josue 1/04/2018

namespace TheNegative.AI.Node
{
    public class ShootProjectileNode : Node
    {
        #region Variables/Constructor

        protected int m_Damage = 5;                     //how much damage the spit will do
        protected GameObject m_ProjectilePrefab;        //prefab which will be instantiated in object pool
        protected GameObject m_FireLocation;            //start location of the projectile
        protected string m_PoolName = string.Empty;     //name of the pool that will hold this type of projectile
        protected int m_InitialPoolSize = 5;            //how many projectiles will start off in the pool

        public ShootProjectileNode(AI reference, int damage, GameObject prefab, GameObject fireLocation,
                                    string poolName, int poolSize) : base(reference)
        {
            m_Damage = damage;
            m_ProjectilePrefab = prefab;
            m_FireLocation = fireLocation;
            m_PoolName = poolName;
            m_InitialPoolSize = poolSize;
            ObjectPoolManager.Instance.CreateNetworkPoolWithName(m_PoolName, "Attacks/" + m_ProjectilePrefab.name, m_InitialPoolSize, m_InitialPoolSize * 4, false);
        }

        #endregion

        #region Functions
        public override BehaviourState UpdateNodeBehaviour()
        {
            return ShootProjectile();
        }

        private BehaviourState ShootProjectile()
        {
            //turn to look at the enemy
            m_AIReference.transform.LookAt(m_AIReference.Target.transform);

            if (m_AIReference.name.Contains("Nerg"))
            {
                SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "NergSpit", m_AIReference.transform.position);
            }

            //grab projectile from pool to fire
            GameObject projectile = ObjectPoolManager.Instance.GetObjectFromNetworkPool(m_PoolName);

            if (projectile != null)
            {
                //calculate direction from the AI position to target position
                Vector3 direction = (m_AIReference.Target.transform.position - m_FireLocation.transform.position).normalized;

                //get any elemental status effects from the list and shoot projectile with RPC
                Status[] statuses = m_AIReference.ElementalDamage.ToArray();
                projectile.GetPhotonView().RPC("FireProjectile", PhotonTargets.All, m_FireLocation.transform.position, direction, -1, m_AIReference.TargetLayerMask, m_Damage, statuses);
            }

            return BehaviourState.Succeed;
        }
        #endregion
    }
}