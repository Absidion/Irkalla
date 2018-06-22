using UnityEngine;

namespace TheNegative.AI.Node
{
    public class LerpToTargetNode : Node
    {
        private float m_DirectionMultiplier = 0.0f;                 //The multiplier for the direction to be multiplied by in order to move it in that direction

        private Vector3 m_Destination = Vector3.zero;               //The end destination of the lerp
        private float m_LerpTime = 0.0f;                            //The time that the lerp started        
        private float m_Speed = 0.0f;                               //The speed at which the target will be lerped
        private float m_Distance = 0.0f;                            //The distance from start position and end position

        public LerpToTargetNode(AI reference, float lerpTime, float directionMultiplier, float speed) : base(reference)
        {
            m_DirectionMultiplier = directionMultiplier;

            m_LerpTime = lerpTime;

            m_Destination = Vector3.zero;
            m_Speed = speed;
        }

        public override void OnFirstTreeCall()
        {
            base.OnFirstTreeCall();
            //save the start time of the lerp
            m_LerpTime = Time.time;

            Vector3 playerTargetLocation = m_AIReference.Target.transform.position;
            playerTargetLocation.y -= m_AIReference.Target.HalfHeight;

            Vector3 playerToAI = playerTargetLocation - m_AIReference.transform.position;
            
            //calulate the destination
            m_Destination = playerToAI.normalized * playerToAI.magnitude + m_AIReference.transform.position;
            //calculate the distance between the lerp location and the final position
            m_Distance = (m_AIReference.transform.position - m_Destination).magnitude;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            //do a simple lerp
            float timeElapsed = (Time.time - m_LerpTime) * m_Speed;
            float percent = timeElapsed / m_Distance;
            m_AIReference.transform.position = Vector3.Lerp(m_AIReference.transform.position, m_Destination, percent);

            if (percent > 1.0f)
                percent = 2.0f;

            //if the lerped value is approximatly 1 then that means that the lerp is done
            return (percent > 1.0f) ? BehaviourState.Succeed : BehaviourState.Running;
        }
    }
}