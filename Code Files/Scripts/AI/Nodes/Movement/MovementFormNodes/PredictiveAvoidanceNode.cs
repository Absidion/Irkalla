using UnityEngine;
using UnityEngine.AI;

namespace TheNegative.AI.Node
{
    public class PredictiveAvoidanceNode : MovementFormNode
    {
        private bool m_UseEraticAvoidance = false;      //If eratic avoidance is checked to true then the AI will randomly choose to move left or right when determining to move left or right of the player
        private float m_DirectionMultiplier = 1.0f;     //The multiplier for how far left or right the AI will move
        private float m_SampleDistance = 5.0f;          //How large the sample distance should be when the AI is attempting to move left or right to avoid the shot

        public PredictiveAvoidanceNode(AI reference, bool waitForMovementToFinish, bool useEraticAvoidence, float directionMultiplier, float sampleDistance) : base(reference, waitForMovementToFinish)
        {
            m_UseEraticAvoidance = useEraticAvoidence;
            m_DirectionMultiplier = directionMultiplier;
            m_SampleDistance = sampleDistance;
        }

        public override void OnFirstTreeCall()
        {
            Player attackingPlayer = null;

            foreach (Player player in m_AIReference.PlayerList)
            {
                if (player.IsAttacking || player.IsUsingAbility)
                {
                    //if there is currently no value saved into attackingPlayer then let's save one into there
                    if (attackingPlayer == null)
                        attackingPlayer = player;
                    //else there is already a value in the attackingPlayer value so the AI must determine who is the bigger threat and attempt to avoid them
                    else
                    {
                        attackingPlayer = (attackingPlayer.RangedDamage < player.RangedDamage) ? player : attackingPlayer;
                    }
                }
            }

            //next if there is an attacking player then the AI needs to figure out how to avoid them
            if (attackingPlayer != null)
            {
                //grab the right vector of the attackingPlayer and move the AI in that direction
                Vector3 directionToMove = attackingPlayer.transform.right;
                //if the AI should use eratic avoidance then it will be calculated randomly here
                directionToMove *= m_UseEraticAvoidance ? (Random.Range(1, 2) == 1 ? -1.0f : 1.0f) : 1.0f;
                //next multiply the directionToMove by the multiplier passed in on start up
                directionToMove *= m_DirectionMultiplier;
                //finally get the AI to attempt to see if the destination is a valid one
                NavMeshHit testHit;
                //first attempt to move in the calculated direction
                if(NavMesh.SamplePosition(directionToMove + m_AIReference.transform.position, out testHit, m_SampleDistance, NavMesh.AllAreas))
                {
                    m_AIReference.Agent.SetDestination(testHit.position);
                }     
                //if the first attempt fails then try to do it in the other direction
                else if(NavMesh.SamplePosition((directionToMove * -1) + m_AIReference.transform.position, out testHit, m_SampleDistance, NavMesh.AllAreas))
                {
                    m_AIReference.Agent.SetDestination(testHit.position);
                }
            }

            base.OnFirstTreeCall();
        }
    }
}