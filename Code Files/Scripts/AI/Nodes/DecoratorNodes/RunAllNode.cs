//Writer: Liam
//Last Updated: Liam 1/2/2018

using System;

namespace TheNegative.AI.Node
{
    public class RunAllNode : ParentNode
    {
        public RunAllNode(AI reference, string name) : base(reference, name) { }
        public RunAllNode(AI reference, string name, params Node[] nodes) : base(reference, name, nodes) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            foreach(Node node in m_ChildNodes)
            {
                node.UpdateNodeBehaviour();
            }

            return BehaviourState.Succeed;
        }
    }
}