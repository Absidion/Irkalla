using UnityEngine;

namespace TheNegative.AI.Node
{
    public class TimeAwayFromCenterNode : Node
    {
        private Vector3 m_CenterOfRoom = Vector3.zero;              //The location of the center of the room
        private float m_CenterSafeSpace = 0.0f;                     //The safe space around the center of the room
        private float m_MaxWaitTime = 0.0f;                         //The maximum time that must pass before this node succeeds
        private float m_Timer = 0.0f;                               //A timer used to record all of the time that has passed while the player is inside of the CenterSafeSpace

        public TimeAwayFromCenterNode(AI reference, Vector3 centerOfRoom, float centerSafeSpace, float maxWaitTime) : base(reference)
        {
            m_CenterOfRoom = centerOfRoom;
            m_CenterSafeSpace = centerSafeSpace;
            m_MaxWaitTime = maxWaitTime;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            //Calculate the distance from the center of the room
            float distanceFromCenter = (m_AIReference.transform.position - m_CenterOfRoom).magnitude;

            //if the distance from the center is greater then the safe space begin to increment the timer
            if(distanceFromCenter > m_CenterSafeSpace)
            {
                m_Timer += Time.deltaTime;
                //if to much time has passed then the node will return succeed
                if(m_Timer >= m_MaxWaitTime)
                {
                    m_Timer = 0.0f;
                    return BehaviourState.Succeed;
                }
            }
            else
            {
                m_Timer = 0.0f;
            }

            return BehaviourState.Failed;
        }
    }
}