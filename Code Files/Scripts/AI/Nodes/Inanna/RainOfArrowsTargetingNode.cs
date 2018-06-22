using System;
using UnityEngine;

namespace TheNegative.AI.Node
{
    public class RainOfArrowsTargetingNode : Node
    {
        private float m_Diameter = 0;           //The diameter of the Arrow Rain attack 

        public RainOfArrowsTargetingNode(AI reference, float diameter) : base(reference)
        {
            m_Diameter = diameter;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (m_AIReference.PlayerList.Count > 1)
            {
                if (!m_AIReference.PlayerList[0].Health.IsDead && !m_AIReference.PlayerList[1].Health.IsDead)
                {
                    //get the direction vector from player 1 to player 2
                    Vector3 player1ToPlayer2Dir = m_AIReference.PlayerList[0].transform.position - m_AIReference.PlayerList[1].transform.position;

                    //check to see if the distance between the players is enough for inanna to use the arrow rain to make it as effective as possible
                    if (player1ToPlayer2Dir.magnitude <= m_Diameter)
                    {
                        //divid by 2 in order to get half the direction vector
                        float halfDst = player1ToPlayer2Dir.magnitude / 2.0f;

                        //Add the half direction vector onto the player1ToPlayer2Dir to get the vector3 position between both players
                        Vector3 targetLocation = m_AIReference.PlayerList[0].transform.position - player1ToPlayer2Dir.normalized * halfDst;
                        //set inanna's arrow rain target position
                        ((InannaAI)m_AIReference).ArrowRainTargetPos = targetLocation;

                        return BehaviourState.Succeed;
                    }
                }                
            }

            //next section will handle choosing the target in the case that there is either
            //A) 1 player
            //B) 1 player is dead in the list and it needs to be looked over
            //C) both players are alive however she cannot target both at once and she just needs to pick the best choice for her target
            Vector3 targetPosition = Vector3.zero;
            int playerTarget = -1;
            int lowestPlayerHealth = int.MaxValue;

            foreach(Player player in m_AIReference.PlayerList)
            {
                if(!player.Health.IsDead)
                {
                    if(player.Health.HP < lowestPlayerHealth)
                    {
                        playerTarget = player.PlayerNumber;
                        lowestPlayerHealth = player.Health.HP;
                        targetPosition = player.transform.position;
                    }
                }
            }

            //save the position of the arrow rain attack
            ((InannaAI)m_AIReference).ArrowRainTargetPos = targetPosition;

            return BehaviourState.Succeed;
        }
    }
}