using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.AI.Node
{
    public class GroupSameNode : MovementFormNode
    {
        private float m_MinStoppingDistance = 0;          //The minimum stopping distance away from the group position that the AI will stop at. Used to calculate a range of max and min stopping distance.
        private float m_MaxStoppingDistance = 0;          //The maximum stopping distance away from the group position that the AI will stop at. Used to calculate a range of max and min stopping distance.

        public GroupSameNode(AI reference, bool waitForMovementToFinish, float minStoppingDist, float maxStoppingDist) : base(reference, waitForMovementToFinish)
        {
        }

        public override void OnFirstTreeCall()
        {
            List<AI> similarAI = new List<AI>();

            foreach (AI ai in m_AIReference.MyIslandRoom.EnemiesInRoom)
            {
                //this will make sure that the AI are of the same type, whilst also making sure that the AI isn't getting a false idea that there are more AI of the
                //same type in the map, when it turns out it's just looking at itself.
                if ((ai.GetType() == m_AIReference.GetType()) && (ai.transform.root != m_AIReference.transform.root))
                {
                    similarAI.Add(ai);
                }
            }

            if (similarAI.Count > 0)
            {
                Vector3 directionToMeetingLocation = m_AIReference.transform.position;
                float distanceToMeetingLocation = 0.0f;

                foreach (AI ai in similarAI)
                {
                    //calculate the direction from this AI to all other AI in the room of the same type
                    directionToMeetingLocation += ai.transform.position - m_AIReference.transform.position;
                    //calculate the distance from this AI to all other AI in the room of the same type
                    distanceToMeetingLocation += (ai.transform.position - m_AIReference.transform.position).magnitude;
                }

                //average the distance to the AI
                distanceToMeetingLocation /= similarAI.Count;
                //set the AI reference's target location to be close to all of the other AI
                m_AIReference.Agent.SetDestination(directionToMeetingLocation * distanceToMeetingLocation);
                //set the stopping distance to refelect the range at which the AI will stop at
                m_AIReference.Agent.stoppingDistance = Random.Range(m_MinStoppingDistance, m_MaxStoppingDistance);
            }

            base.OnFirstTreeCall();
        }
    }
}