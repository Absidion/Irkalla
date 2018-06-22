namespace TheNegative.AI.Node
{
    public class TargetingDistanceNode : TargetingNode
    {        
        private TargetingSightNode m_TargetSightNode;           //The target sight node reference here should be used if the AI intends to use sight as a detection tool

        public TargetingDistanceNode(AI reference, int score) : base(reference, score) { }        
        public TargetingDistanceNode(AI reference, int score, TargetingSightNode sightNode) : base(reference, score)
        {
            m_TargetSightNode = sightNode;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            float closestPlayerDist = float.MaxValue;
            int closestPlayerNumber = -1;

            foreach (Player player in m_AIReference.PlayerList)
            {
                if (m_TargetSightNode != null)
                {
                    if (m_TargetSightNode.VisablePlayers.Contains(player) == false)
                        continue;
                }

                //calculate the distance between the potential target and the AI
                float distance = (player.transform.position - m_AIReference.transform.position).magnitude;

                //if the distance is less then that of the AI's detection range that means we add the distance score onto the AI's score dictionary
                if ((distance < m_AIReference.DetectionRange) && (distance < closestPlayerDist))
                {
                    closestPlayerDist = distance;
                    closestPlayerNumber = player.PlayerNumber;
                }
            }

            //if the closest player isn't null then that means that a player was found that we can target, so add the distance range onto the score
            if(closestPlayerNumber != -1)
            {
                m_AIReference.Scores[closestPlayerNumber] += m_Score;
            }

            return BehaviourState.Succeed;
        }
    }
}