using UnityEngine;

//Writer: Liam
//Last Updated: Liam 12/30/2017

namespace TheNegative.AI.Node
{
    public class CalculateTargetNode : Node
    {
        public CalculateTargetNode(AI reference) : base(reference) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            int highestTargetScore = int.MinValue;
            int targetsPlayerIndex = -1;

            for (int i = 0; i < m_AIReference.PlayerList.Count; i++)
            {
                //if the player we are iterating through is in fact alive and is in our room then we can assess whether or not they are the AI's target this frame
                if ((m_AIReference.PlayerList[i].Health.IsDead == false) && (m_AIReference.MyIslandRoom.PlayersInRoom.Contains(m_AIReference.PlayerList[i]) == true))
                {
                    //check to see if the player's target score is greater then the previous one and if it is set the relevant data
                    if (m_AIReference.Scores[m_AIReference.PlayerList[i].PlayerNumber] > highestTargetScore)
                    {
                        highestTargetScore = m_AIReference.Scores[m_AIReference.PlayerList[i].PlayerNumber];
                        targetsPlayerIndex = i;
                    }
                    //if the value wasn't greater check to see if they're equal and if they are pick a random player to become our target
                    else if (m_AIReference.Scores[m_AIReference.PlayerList[i].PlayerNumber] == highestTargetScore)
                    {
                        float randomNumber = Random.value;

                        if (randomNumber < 0.5)
                        {
                            targetsPlayerIndex = 0;
                        }
                        else
                        {
                            targetsPlayerIndex = 1;
                        }
                    }
                }
            }

            //if the targets player index is valid then assign it and return succeed, otherwise return false
            if (targetsPlayerIndex >= 0)
            {
                m_AIReference.Target = m_AIReference.PlayerList[targetsPlayerIndex];
                return BehaviourState.Succeed;
            }
            else
            {
                return BehaviourState.Failed;
            }
        }
    }
}
