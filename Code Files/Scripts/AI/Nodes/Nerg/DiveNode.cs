using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Writer: James Aiken   
//Date Created: 10/27/2017
//Date Last Modified: Josue 1/04/2017

namespace TheNegative.AI.Node
{
    public class DiveNode : Node
    {
        #region Variables/Constructor

        private float m_AttackCooldown = 5.0f;      //Cooldown for this Attack   
        private int m_Damage = 15;                  //Damage value for the Dive Attack
        private float m_AttackAngle = 45.0f;        //Angle used for how the Nerg moves upward then downward

        private float m_AttackTimer = 0.0f;         //the timer for how long since the last time this attack was used
        private Vector3 m_Target;                   //Target is needed to get direction the Nerg Dives in.

        private NergAI m_NergAIRef;                 //reference to nerg AI as we need a specific property from the nerg

        public DiveNode(AI reference, float cooldown, int damage, float angle) : base(reference)
        {
            m_AttackCooldown = cooldown;
            m_Damage = damage;
            m_AttackAngle = angle;
            m_NergAIRef = m_AIReference as NergAI;
        }

        #endregion

        #region Functions

        public override BehaviourState UpdateNodeBehaviour()
        {
            //This is a Stop-gap solution when you figure out what is causing the error
            if (m_NergAIRef == null)
                return BehaviourState.Failed;

            if (m_NergAIRef.CanAttack == true)
            {
                return BehaviourState.Running;
            }
            else if (m_AttackTimer >= m_AttackCooldown)
            {
                m_AttackTimer = 0.0f;
                return Dive();
            }
            else
            {
                m_AttackTimer += Time.deltaTime;
                return BehaviourState.Failed;
            }
        }

        private BehaviourState Dive()
        {
            m_AIReference.SetNavMeshAgent(false);                           //set navmesh agent to false so nerg can fly through the air
            m_AIReference.SetIsKinematic(false);                            //set to kinematic so force can affect nerg
            m_Target = m_AIReference.Target.transform.position;             //set target position
            m_AIReference.transform.LookAt(m_AIReference.Target.transform); //rotate towards target

            //offset the position of the AI before diving
            float yOffset = 0.3f;
            Vector3 newPosition = m_AIReference.transform.position;
            newPosition.y += yOffset;
            m_AIReference.transform.position = newPosition;
            Vector3 dir = m_Target - m_AIReference.transform.position;    // get target direction
            float height = dir.y;                                         // get height difference
            dir.y = 0;                                                    // retain only the horizontal direction
            float dist = dir.magnitude;                                   // get horizontal distance
            float a = m_AttackAngle * Mathf.Deg2Rad;                      // convert angle to radians
            dir.y = dist * Mathf.Tan(a);                                  // set dir to the elevation angle
            dist += height / Mathf.Tan(a);                                // correct for small height differences

            // calculate the velocity magnitude
            float vel = Mathf.Sqrt(dist * Physics.gravity.magnitude / Mathf.Sin(2 * a));
            m_AIReference.rigidbody.AddForce(vel * dir.normalized, ForceMode.VelocityChange);

            SoundManager.GetInstance().photonView.RPC("PlaySFXRandomizedPitchNetworked", PhotonTargets.All, "Nerg_Leap", m_AIReference.transform.position);

            m_AIReference.photonView.RPC("SetCanAttack", PhotonTargets.All, true);
            return BehaviourState.Running;
        }

        #endregion
    }
}