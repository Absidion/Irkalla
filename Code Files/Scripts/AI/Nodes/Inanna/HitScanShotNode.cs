using UnityEngine;

namespace TheNegative.AI.Node
{
    public class HitScanShotNode : Node
    {
        private HitScanSingleTarget m_HitScanShot;          //Reference to the HitScanSingleTarget object attached to the AI that is used to fire the shot
        private Transform m_ShootLocation;                  //The location that the shot is coming from
        private int m_Damage = 0;                           //The amount of damage that the attack does
        private float m_Delay = 0.0f;                       //The amount of delay before the attack is fired
        private float m_DelayTimer = 0.0f;                  //The timer that increments as time moves forward

        public HitScanShotNode(AI reference, Transform shootLocation, HitScanSingleTarget hitScanShot, int damage, float delay) : base(reference)
        {
            m_ShootLocation = shootLocation;
            m_HitScanShot = hitScanShot;
            m_Damage = damage;
            m_Delay = delay;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            m_DelayTimer += Time.deltaTime;
            if (m_DelayTimer >= m_Delay)
            {
                //once the delay is over the shot will be fired
                Vector3 direction = m_AIReference.Target.transform.position - m_ShootLocation.position;

                m_HitScanShot.gameObject.GetPhotonView().RPC("FireSingleTargetEnemyHitscan", PhotonTargets.All,
                    m_ShootLocation.position,
                    direction.normalized,
                    m_Damage,
                    -1,
                    direction.magnitude * 2,
                    direction.magnitude * 2,
                    m_AIReference.IgnoreLayerMask,
                    m_AIReference.TargetLayerMask,
                    m_ShootLocation.position);

                return BehaviourState.Succeed;
            }

            return BehaviourState.Running;
        }

    }
}
