using UnityEngine;

//Writer: Liam
//Last Updated: Liam 12/30/2017

namespace TheNegative.AI.Node
{
    public class KamikazeNode : Node
    {
        private float m_CooldownTime = 20.0f;         //the cooldown of the Kamikaze attack
        private float m_KamikazeSpeed = 10.0f;         //the speed of the kamikaze attack
        private float m_ExplosionRadius = 2.0f;        //the raidus of the explosion
        private float m_ExplosionPushBack = 10.0f;     //explosion push back
        private int m_Damage = 15;                     //the damage the attack deals

        private float m_Timer = 0.0f;                //how long since the Kamikaze attack has been activated
        private Vector3 m_TargetLocation;            //the target's transform               

        public KamikazeNode(AI reference) : base(reference) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if(m_Timer > m_CooldownTime)
            {
                return DiveBombTarget();
            }
            else
            {
                m_Timer += Time.deltaTime;

                //save the position into the target location variable 
                m_TargetLocation = m_AIReference.Target.transform.position;
            }

            return BehaviourState.Failed;
        }

        private BehaviourState DiveBombTarget()
        {
            //if the AI is moving towards it's target location
            if ((m_AIReference.transform.position - m_TargetLocation).magnitude >= MathFunc.LargeEpsilon)
            {
                m_AIReference.transform.position += MathFunc.MoveToward(m_AIReference.transform.position, m_TargetLocation, m_KamikazeSpeed);

                return BehaviourState.Running;
            }
            //if it is close enough to the location that it can explode, then go for it
            else
            {
                Collider[] colliders = Physics.OverlapSphere(m_AIReference.transform.position, m_ExplosionRadius * 0.5f, m_AIReference.TargetLayerMask);

                foreach (Collider collider in colliders)
                {
                    Player p = collider.GetComponent<Player>();

                    if (p != null)
                    {
                        Status[] statuses = { Status.Burn };
                        p.photonView.RPC("TakeDamage", PhotonTargets.All, m_Damage, statuses);
                        p.ApplyForceOverNetwork((p.transform.position - m_AIReference.transform.position).normalized * m_ExplosionPushBack, ForceMode.Impulse);
                    }
                }

                //instantiate the explosion over the network
                GameObject explosion = PhotonNetwork.Instantiate("ExplosionRenderer", m_AIReference.transform.position, m_AIReference.transform.rotation, 0);
                ExplosionEffect effect = explosion.GetComponent<ExplosionEffect>();

                //set the required activation stuff over the network
                effect.photonView.RPC("ActivateExplosion", PhotonTargets.All, m_ExplosionRadius);
                //set the health value of the phoenix to be -1 because the kamikaze attack kills the AI
                m_AIReference.Health.HP = -1;

                //reset the cooldown timer here
                m_Timer = 0.0f;
                return BehaviourState.Succeed;
            }            
        }
    }
}
