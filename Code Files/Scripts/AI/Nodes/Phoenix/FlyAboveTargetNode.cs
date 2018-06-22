using UnityEngine;

//Writer: Liam
//Last Updated: 12/17/2017

namespace TheNegative.AI.Node
{
    public class FlyAboveTargetNode : Node
    {
        private float m_AboveTargetAmount = 10.0f;                 //the amount above the target that the AI should be
        private float m_TargetDistance = 10.0f;                    //the ideal distance away from the target that the AI should be      
        private float m_YCosOffset = 0.5f;                         //the offset on the Y axis of the bird moving

        private float m_DegreePerSec = -1;                      //the amount of degrees the object rotates per second
        private float m_RandomOffsetX = -1;                     //the random offset that the AI will be off of the X-Axis
        private float m_RandomOffsetZ = -1;                     //the random offset that the AI will be off of the Z-Axis

        public FlyAboveTargetNode(AI reference) : base(reference)
        {
            m_DegreePerSec = Random.Range(10.0f, 30.0f);
            m_RandomOffsetX = Random.Range(-2, 2);
            m_RandomOffsetZ = Random.Range(-2, 2);
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            m_DegreePerSec += Time.deltaTime;

            //get the direction from the player to the AI
            Vector3 direction = m_AIReference.transform.position - m_AIReference.Target.transform.position;

            //calculate base on the direction where the AI should be the required distance away from the target
            Vector3 idealLocation = direction.normalized + m_AIReference.Target.transform.position;
            idealLocation.x += (m_TargetDistance + m_RandomOffsetX) * Mathf.Cos(m_DegreePerSec);
            idealLocation.y += m_AboveTargetAmount + (Mathf.Cos(Time.time) * m_YCosOffset);
            idealLocation.z += (m_TargetDistance + m_RandomOffsetZ) * Mathf.Sin(m_DegreePerSec);

            m_AIReference.transform.position = idealLocation;

            //cross product of the direction to the player and the AI's up vector to make a perpedicular vector to rep the AI's rotation
            Vector3 rotation = Vector3.Cross(direction, Vector3.up);
            m_AIReference.transform.rotation = Quaternion.LookRotation(rotation,Vector3.up);

            return BehaviourState.Succeed;
        }
    }
}