using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.AI.Node
{
    public class InannaMoveTowerNode : Node
    {
        private List<Transform> m_TowerLandingSpots;            //A list of tower landing spots that Inanna may land on after jumping

        public InannaMoveTowerNode(AI reference, Transform[] landingSpots) : base(reference)
        {
            m_TowerLandingSpots = new List<Transform>();

            foreach(Transform t in landingSpots)
            {
                m_TowerLandingSpots.Add(t);
            }
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (m_AIReference.Agent.enabled)
                m_AIReference.SetNavMeshAgent(false);

            //choose a random index from the tower landing spots list to place Inanna at
            int randomIndex = UnityEngine.Random.Range(0, m_TowerLandingSpots.Count);
            Transform currentTarget = m_TowerLandingSpots[randomIndex];
            //remove the transform from the list that way it cannot be chosen again
            m_TowerLandingSpots.Remove(currentTarget);

            //the new location will be equal to Inanna's height after the jump into the air as well as the x and z location of the transform currentTarget position
            Vector3 newLocation = new Vector3(currentTarget.position.x, m_AIReference.transform.position.y, currentTarget.position.z);
            
            m_AIReference.transform.position = newLocation;

            return BehaviourState.Succeed;
        }
    }
}