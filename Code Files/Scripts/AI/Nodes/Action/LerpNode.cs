using UnityEngine;

namespace TheNegative.AI.Node
{
    public class LerpNode : Node
    {
        private Vector3 m_Direction = Vector3.zero;                 //The direction that you want to the AI to lerp in
        private float m_DirectionMultiplier = 0.0f;                 //The multiplier for the direction to be multiplied by in order to move it in that direction
        
        private Vector3 m_Destination = Vector3.zero;               //The end destination of the lerp
        private float m_LerpTime = 0.0f;                            //The time that the lerp started        
        private float m_Speed = 0.0f;                               //The speed at which the target will be lerped
        private float m_Distance = 0.0f;                            //The distance from start position and end position

        public LerpNode(AI reference, Vector3 direction, float lerpTime, float directionMultiplier, float speed) : base(reference)
        {
            m_Direction = direction;
            m_DirectionMultiplier = directionMultiplier;

            m_LerpTime = lerpTime;
            //calculate the lerp destination
            m_Destination = m_Direction * m_DirectionMultiplier;
            m_Speed = speed;
        }

        public override void OnFirstTreeCall()
        {
            base.OnFirstTreeCall();
            //save the start time of the lerp
            m_LerpTime = Time.time;
            //calulate the destination
            m_Destination = (m_Direction * m_DirectionMultiplier) + m_AIReference.transform.position;
            //calculate the distance between the lerp location and the final position
            m_Distance = (m_AIReference.transform.position - m_Destination).magnitude;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            //do a simple lerp
            float timeElapsed = (Time.time - m_LerpTime) * m_Speed;
            float percent = timeElapsed / m_Distance;
            m_AIReference.transform.position = Vector3.Lerp(m_AIReference.transform.position, m_Destination, percent);

            //if the lerped value is approximatly 1 then that means that the lerp is done
            return (percent > 1.0f) ? BehaviourState.Succeed : BehaviourState.Running;
        }
    }
}