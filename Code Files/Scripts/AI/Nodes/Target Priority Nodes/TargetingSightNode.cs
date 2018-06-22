using System.Collections.Generic;
using UnityEngine;

//Writer: Liam
//Last Updated: Liam 12/30/2017

namespace TheNegative.AI.Node
{
    //
    public class TargetingSightNode : TargetingNode
    {        
        private int m_SightLayerMask = 0;               //The sight layer mask
        private List<Player> m_VisablePlayers = null;   //Visable player list
        private bool m_IgnoreFailure = false;           //If ignore failure is enabled then that means that even if the raycast fails the sequence can continue

        public TargetingSightNode(AI reference, int score, bool ignoreFailure = false) : base(reference, score)
        {
            m_SightLayerMask = ~(LayerMask.GetMask("Room") | LayerMask.GetMask("Enemy"));
            m_VisablePlayers = new List<Player>();
            m_IgnoreFailure = ignoreFailure;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            //if we are using sight as a tool of detection then the AI needs to be able to see something for it to attack. This bool will help with this check
            bool hasSeenPlayer = false;

            //clear the list of visable players from the list of visable players
            m_VisablePlayers.Clear();

            foreach (Player player in m_AIReference.PlayerList)
            {
                RaycastHit hitData;
                Vector3 directionToPlayer = player.EyeLocation.transform.position - m_AIReference.transform.position;

                if (Physics.SphereCast(m_AIReference.transform.position, 0.1f, directionToPlayer.normalized, out hitData, float.MaxValue, m_SightLayerMask))
                {
                    //Uncomment if you want to see the line drawn in a sight detection check
                    //Debug.DrawLine(m_AIReference.transform.position, hitData.point);

                    //check to make sure we collided 
                    if (hitData.collider.tag == ("Player"))
                    {                        
                        m_AIReference.Scores[player.PlayerNumber] += m_Score;
                        m_VisablePlayers.Add(player);
                        hasSeenPlayer = true;
                    }
                }
            }
                        
            //if we've seen a player then we're done and can move on to the next check
            if (hasSeenPlayer || m_IgnoreFailure)
            {
                return BehaviourState.Succeed;
            }
            //if we haven't seen a player then the sequence that contains this should fail
            else
            {
                return BehaviourState.Failed;
            }
        }

        public List<Player> VisablePlayers { get { return m_VisablePlayers; } set { m_VisablePlayers = value; } }
    }
}