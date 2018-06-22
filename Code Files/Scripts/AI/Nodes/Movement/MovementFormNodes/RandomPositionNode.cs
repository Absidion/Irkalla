using UnityEngine;

namespace TheNegative.AI.Node
{
    public class RandomPositionNode : MovementFormNode
    {
        private float m_RoomHeight = 0.0f;
        private float m_RoomWidth = 0.0f;
        private float m_RoomLength = 0.0f;

        public RandomPositionNode(AI reference, bool waitForMovementToFinish, float roomHeight, float roomWidth, float roomLength) : base(reference, waitForMovementToFinish)
        {
            m_RoomHeight = roomHeight;
            m_RoomLength = roomLength;
            m_RoomWidth = roomWidth;
        }

        public override void OnFirstTreeCall()
        {
            float randomXInRoom = Random.Range(0, m_RoomWidth);
            float randomZInRoom = Random.Range(0, m_RoomLength);

            Vector3 posInRoom = new Vector3(randomXInRoom, m_RoomHeight, randomZInRoom);
            posInRoom = m_AIReference.MyIslandRoom.transform.TransformPoint(posInRoom);

            Vector3 hitPoint = AIUtilits.CalcClosestPositionOnNavMeshBelowPos(posInRoom.x, posInRoom.z, posInRoom.y, m_AIReference.transform.position.y, 5.0f);

            if (!MathFunc.AlmostEquals(hitPoint, posInRoom))
                m_AIReference.Agent.SetDestination(hitPoint);

            base.OnFirstTreeCall();
        }
    }
}
