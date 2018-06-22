//Writer: Liam
//Last Updated: Liam 12/28/2017
using UnityEngine;

namespace TheNegative.AI.Node
{
    //The AlwaysFailNode is a decorator which means that it should only have 1 child node. The purpose of this node is to return failed even if the node succeeded.
    //Since this node is a decorator that means it will only do logic on the first child node as it assumes it only has a single child
    public class AlwaysFailNode : ParentNode
    {
        public AlwaysFailNode(AI reference, string name) : base(reference, name) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (m_ChildNodes.Count < 1)
                Debug.Assert(true, "This decorator, AlwaysFailNode, doesn't have any childnodes. Please fix your error");

            m_ChildNodes[0].UpdateNodeBehaviour();

            return BehaviourState.Failed;
        }
    }
}
