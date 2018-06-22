using UnityEngine;


namespace TheNegative.AI.Node
{
    public class SnipeDelayNode : Node
    {
        private NetworkLineRenderer m_SnipeLine;        //The snipe line that will be drawn to the target player
        private Transform m_TargetTransform;            //The transform of the location who's radius the players may not leave
        private float m_Distance;                       //The distance that the players may not exit
        private float m_BuildUp;                        //The amount of build up time that the node has
        private float m_Timer;                          //Timer used to increment the amount of time that the player has been outside of the range of the target transform

        public SnipeDelayNode(AI reference, Transform target, NetworkLineRenderer lineRenderer, float distance, float buildUp) : base(reference)
        {
            m_SnipeLine = lineRenderer;
            m_TargetTransform = target;
            m_Distance = distance;
            m_BuildUp = buildUp;
        }

        public override void OnFirstTreeCall()
        {
            base.OnFirstTreeCall();

            //enable the snipe line to draw
            m_SnipeLine.SetLineRendererEnabled(true);
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (m_TargetTransform == null)
                return BehaviourState.Failed;

            //check and see if anyone is outside of the distance from eri
            Player furthestPlayer = null;
            float furthestDistance = float.MinValue;          

            foreach (Player player in m_AIReference.PlayerList)
            {
                if (!player.CanBeTargeted)
                    continue;

                float distance = (player.transform.position - m_TargetTransform.position).magnitude;
                if (distance > m_Distance)
                {
                    if (distance > furthestDistance)
                    {
                        furthestPlayer = player;
                        furthestDistance = distance;
                    }
                }
            }

            //if no player was chosen then that means that nobody is outside of the radius where they can be shot, so return
            if (furthestPlayer == null)
            {
                m_SnipeLine.SetLineRendererEnabled(false);
                m_SnipeLine.RemoveTarget();
                return BehaviourState.Failed;
            }

            m_Timer += Time.deltaTime;
            //make sure that the second point on the line renderer is drawing on the player
            //m_SnipeLine.LineRenderer.SetPosition(1, furthestPlayer.transform.position);
            m_SnipeLine.SetTarget(furthestPlayer.PlayerNumber);
            
            if(m_Timer >= m_BuildUp)
            {
                m_AIReference.Target = furthestPlayer;
                m_SnipeLine.SetLineRendererEnabled(false);
                m_SnipeLine.RemoveTarget();
                return BehaviourState.Succeed;
            }

            return BehaviourState.Running;
        }


        public override void Reset()
        {
            base.Reset();
            m_AIReference.Target = null;
            m_Timer = 0.0f;
        }
    }
}