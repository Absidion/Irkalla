//Writer: Liam
//Last Updated: Liam 12/28/2017

namespace TheNegative.AI.Node
{
    //this node will act upon all of its children until a successful action has been taken. If all of its children fail then that means the selector has failed to find an appropriate node
    public class SelectorNode : ParentNode
    {
        private int m_SelectorIndex = 0;           //the location that the tree left the selector at  before opperating on a running node

        //constructor
        public SelectorNode(AI reference, string name) : base(reference, name) { }
        public SelectorNode(AI reference, string name, params Node[] nodes) : base(reference, name, nodes) { }


        public override BehaviourState UpdateNodeBehaviour()
        {
            for(int i = m_SelectorIndex; i < m_ChildNodes.Count; i++)
            {
                if (!m_ChildNodes[i].FirstCallActivated)
                    m_ChildNodes[i].OnFirstTreeCall();

                BehaviourState nodeState = m_ChildNodes[i].UpdateNodeBehaviour();
                              
                switch (nodeState)
                {
                    //if the current child node has failed then we must move onto and try the next node
                    case BehaviourState.Failed:
                        continue;

                    //if the current child node has been successful then we return succeed
                    case BehaviourState.Succeed:
                        Reset();
                        return BehaviourState.Succeed;

                    //in the case that the child node in the selector is running we save that index so that next time we search through we start at the running index
                    case BehaviourState.Running:
                        m_SelectorIndex = i;                        
                        return BehaviourState.Running;
                }
            }

            //if we escape the loop that means that no successful node to act upon was found and therefore the selector node has failed
            Reset();
            return BehaviourState.Failed;
        }

        public override void Reset()
        {
            base.Reset();
            m_SelectorIndex = 0;
        }
    }
}