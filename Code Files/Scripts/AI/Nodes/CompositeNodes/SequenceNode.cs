//Writer: Liam
//Last Updated: Liam 12/28/2017

namespace TheNegative.AI.Node
{
    //This node will activate all of it's nodes in sequence until either a) the sequence is completed or b) a node in the sequence fails
    public class SequenceNode : ParentNode
    {
        private int m_SequenceLocation = 0;            //the location that the tree left the sequence in before opperating on a running node in this sequence
        
        //constructor
        public SequenceNode(AI reference, string name) : base(reference, name) { }
        public SequenceNode(AI reference, string name, params Node[] nodes) : base(reference, name, nodes) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            for (int i = m_SequenceLocation; i < m_ChildNodes.Count; i++)
            {                
                if (!m_ChildNodes[i].FirstCallActivated)
                    m_ChildNodes[i].OnFirstTreeCall();

                BehaviourState nodeState = m_ChildNodes[i].UpdateNodeBehaviour();

                switch (nodeState)
                {
                    //if the nodeState has failed it means the node and therefore the sequence have failed
                    case BehaviourState.Failed:
                        Reset();
                        return BehaviourState.Failed;

                    //if the nodeState has succeeded then we move to the next node in the seqeuence
                    case BehaviourState.Succeed:
                        continue;

                        //in the situation were the node is running then that means that there is still more to do in the sequence.
                        //we need to remember the index of the current child node in the sequence so we may continue from the node and then return running.
                    case BehaviourState.Running:
                        m_SequenceLocation = i;
                        return BehaviourState.Running;
                }
            }

            //if we exit to this location it means that the sequence is finished and has succeeded
            Reset();
            return BehaviourState.Succeed;
        }

        //reset all child values and the sequence location to -1
        public override void Reset()
        {
            base.Reset();
            m_SequenceLocation = 0;
        }
    }
}