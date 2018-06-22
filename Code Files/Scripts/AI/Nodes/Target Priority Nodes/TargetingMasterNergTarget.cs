using UnityEngine;

//Author: Josue
//Last edited: Josue 1/04/2017

namespace TheNegative.AI.Node
{
    public class TargetingMasterNergTarget : TargetingNode
    {
        private AI m_MasterNergReference = null;    //reference to any master nerg in the room
        private bool m_MasterSearchedFor = false;   //gets set after a search has been done for the master nerg

        public TargetingMasterNergTarget(AI reference, int score) : base(reference, score)
        {
            
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            //if there is a master nerg in the room and they have a target, add score to that player
            if (m_MasterNergReference != null)
            {
                if (m_MasterNergReference.Target != null)
                {
                    m_AIReference.Scores[m_MasterNergReference.Target.PlayerNumber] += m_Score;
                }
            }
            else
            {
                if (m_AIReference.MyIslandRoom != null && !m_MasterSearchedFor)
                {
                    //loop through the list of enemies looking for a master nerg
                    for (int i = 0; i < m_AIReference.MyIslandRoom.EnemiesInRoom.Count; i++)
                    {
                        GameObject enemy = m_AIReference.MyIslandRoom.EnemiesInRoom[i].gameObject;

                        if (enemy.name.Contains("MasterNerg"))
                        {
                            m_MasterNergReference = m_AIReference.MyIslandRoom.EnemiesInRoom[i];
                        }
                    }

                    m_MasterSearchedFor = true;
                }
            }

            return BehaviourState.Succeed;
        }
    }
}