using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.AI.Node
{
    public class GroupOtherNode : MovementFormNode
    {
        public enum GroupOtherStyle { Style_Random, Style_First_In_List, Style_Last_In_List, Style_Closest }

        private float m_MinStoppingDistance = 0;          //The minimum stopping distance away from the group position that the AI will stop at. Used to calculate a range of max and min stopping distance.
        private float m_MaxStoppingDistance = 0;          //The maximum stopping distance away from the group position that the AI will stop at. Used to calculate a range of max and min stopping distance.
        private GroupOtherStyle m_GroupOtherStyle = GroupOtherStyle.Style_Closest;      //How the AI will choose who it will try to group with

        public GroupOtherNode(AI reference, float minStoppingDist, float maxStoppingDist, GroupOtherStyle groupingStyle) : base(reference, false)
        {
            m_MinStoppingDistance = minStoppingDist;
            m_MaxStoppingDistance = maxStoppingDist;
            m_GroupOtherStyle = groupingStyle;
        }

        public override void OnFirstTreeCall()
        {
            AI targetToGroupWith = null;

            if (m_AIReference.MyIslandRoom.EnemiesInRoom.Count > 1)
            {
                switch (m_GroupOtherStyle)
                {
                    //This will get a random enemy that isn't this one
                    case GroupOtherStyle.Style_Random:
                        targetToGroupWith = m_AIReference.MyIslandRoom.GetRandomEnemy(m_AIReference.transform);
                        break;

                    //This will get the first enemy in the list as the target to group with as long as it isn't this node's AI reference
                    case GroupOtherStyle.Style_First_In_List:
                        targetToGroupWith = m_AIReference.MyIslandRoom.EnemiesInRoom[0].transform.root == m_AIReference.transform.root ?
                            m_AIReference.MyIslandRoom.EnemiesInRoom[1] :
                            m_AIReference.MyIslandRoom.EnemiesInRoom[0];
                        break;

                    //This will get the last enemy in the list as the target to group with as long as it isn't this node's AI reference
                    case GroupOtherStyle.Style_Last_In_List:
                        int lastIndex = m_AIReference.MyIslandRoom.EnemiesInRoom.Count;

                        targetToGroupWith = m_AIReference.MyIslandRoom.EnemiesInRoom[lastIndex].transform.root == m_AIReference.transform.root ?
                            m_AIReference.MyIslandRoom.EnemiesInRoom[lastIndex - 1] :
                            m_AIReference.MyIslandRoom.EnemiesInRoom[lastIndex];
                        break;

                    //This will get the closest enemy and set them as the group up target
                    case GroupOtherStyle.Style_Closest:
                        float closestDistance = float.MaxValue;
                        foreach (AI ai in m_AIReference.MyIslandRoom.EnemiesInRoom)
                        {
                            float aiDist = (m_AIReference.transform.position - ai.transform.position).sqrMagnitude;

                            if (aiDist < closestDistance * closestDistance)
                            {
                                closestDistance = aiDist;
                                targetToGroupWith = ai;
                            }
                        }
                        break;
                }
            }

            if (targetToGroupWith != null)
            {
                m_AIReference.Agent.SetDestination(targetToGroupWith.transform.position);
            }

            base.OnFirstTreeCall();
        }
    }
}