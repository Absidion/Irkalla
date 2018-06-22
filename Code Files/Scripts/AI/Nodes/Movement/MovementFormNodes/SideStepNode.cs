using UnityEngine;

namespace TheNegative.AI.Node
{
    public class SideStepNode : MovementFormNode
    {
        private float m_DirectionMultiplier = 1.0f;                 //Multipier for the direction that the ai will move in
        private float m_CurrentSideStepDirection = -1.0f;           //Changes every side step so that the AI isn't always moving right. Adds variance

        public SideStepNode(AI reference, bool waitForMovementToFinish, float directionMultiplier) : base(reference, waitForMovementToFinish)
        {
            m_DirectionMultiplier = directionMultiplier;
        }

        public override void OnFirstTreeCall()
        {
            Vector3 directionToMove = m_AIReference.transform.right * m_DirectionMultiplier * m_CurrentSideStepDirection;
            m_CurrentSideStepDirection = m_CurrentSideStepDirection < 0 ? 1.0f : -1.0f;

            directionToMove = AIUtilits.CalcClosestPositionOnNavMeshBelowPos(directionToMove + m_AIReference.transform.position);

            m_AIReference.Agent.SetDestination(directionToMove);

            base.OnFirstTreeCall();
        }
    }
}