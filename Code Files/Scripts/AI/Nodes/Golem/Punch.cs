using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Author: Daniel
//Last edited: 11/09/2017

namespace TheNegative.AI.Node
{
    public class PunchNode : Node
    {
        #region Private Members
        private float m_Knockback = 6.0f;             //The amount of knockback the punch delivers
        private int m_Damage = 15;                    //The Damage the Punch delivers
        private float m_AttackRange = 0.0f;           //distance from the target at which the punch can be activated

        private FistCollider m_Fist;                  //references to punch collider and 
        private BoxCollider m_PunchHitBox;

        private bool m_AnimationFinished = false;     //gets set true when animation is finished and node can suceed
        private GolemAI m_GolemAI = null;

        public bool AnimationFinished { get { return m_AnimationFinished; } set { m_AnimationFinished = value; } }
        #endregion

        #region Public Methods
        public PunchNode(AI reference, float knockback, int damage, float attackRange, BoxCollider punchHitBox, FistCollider fist) : base(reference)
        {
            //Set up the values
            m_Knockback = knockback;
            m_Damage = damage;
            m_PunchHitBox = punchHitBox;
            m_AttackRange = attackRange;
            m_Fist = fist;
            m_Fist.Damage = damage;
            m_Fist.ElementalDamage = m_AIReference.ElementalDamage.ToArray();
            m_GolemAI = m_AIReference as GolemAI;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            //if (m_GolemAI.IsPunching)
            //{
            //    return BehaviourState.Running;
            //}
            //else if (m_AnimationFinished)
            //{
            //    m_AnimationFinished = false;
            //    return BehaviourState.Succeed;
            //}
            //else if (Vector3.Distance(m_AIReference.transform.position, m_AIReference.Target.transform.position) <= m_AttackRange)
            //{
            //    m_GolemAI.IsPunching = true;
            //    m_Fist.CanDamage = true;
            //    return BehaviourState.Running;
            //}

            return BehaviourState.Failed;
        }
        #endregion
    }
}