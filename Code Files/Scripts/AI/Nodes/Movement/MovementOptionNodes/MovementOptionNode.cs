using System.Collections.Generic;

namespace TheNegative.AI.Node
{
    public abstract class MovementOptionNode : Node
    {
        protected List<MovementFormNode> m_ChildNodes;

        public MovementOptionNode(AI reference, params MovementFormNode[] childNodes) : base(reference)
        {
            m_ChildNodes = new List<MovementFormNode>();

            foreach (MovementFormNode movementNode in childNodes)
            {
                m_ChildNodes.Add(movementNode);
            }
        }

        public override void Reset()
        {
            base.Reset();
            foreach(Node node in m_ChildNodes)
            {
                node.Reset();
            }
        }
    }
}