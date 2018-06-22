using UnityEngine;

namespace TheNegative.AI.Node
{
    public class BackAwayNode : MovementFormNode
    {
        private float m_BackAwayMultiplier = 0.0f;

        public BackAwayNode(AI reference, bool waitForMovementToFinish, float multiplier) : base(reference, waitForMovementToFinish)
        {
            m_BackAwayMultiplier = multiplier;
        }

        public override void OnFirstTreeCall()
        {          
            Player closestPlayer = null;
            float closestDistance = float.MaxValue;

            foreach (Player player in m_AIReference.PlayerList)
            {
                float distanceAIToPlayer = (player.transform.position - m_AIReference.transform.position).magnitude;

                //calculate the direction of the current iterated player to the AI
                if (distanceAIToPlayer < closestDistance)
                {
                    //if the player is closer then the previous distance then be sure to remember all of that information
                    closestDistance = distanceAIToPlayer;
                    closestPlayer = player;
                }
            }

            if (closestPlayer != null)
            {
                Vector3 directionToTravel = (closestPlayer.transform.position - m_AIReference.transform.position).normalized;
                
                Vector3 idealPosition = AIUtilits.CalcClosestPositionOnNavMeshBelowPos(m_AIReference.transform.position - (directionToTravel * m_BackAwayMultiplier));
                m_AIReference.Agent.SetDestination(idealPosition);
            }

            base.OnFirstTreeCall();
        }
    }
}

