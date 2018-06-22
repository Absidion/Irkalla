//Writer: Liam
//Last Updated: Liam 12/28/2017
using UnityEngine;

namespace TheNegative.AI.Node
{
    //An InverterNode is a decorator which means that it should only have 1 child node. The purpose of this node is to change succeed to failed and failed to succeed. Running doesn't change
    //Since this node is a decorator that means it will only do logic on the first child node as it assumes it only has a single child
    public class InverterNode : ParentNode
    {
        public InverterNode(AI reference, string name) : base(reference, name){ }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (m_ChildNodes.Count < 1)
                Debug.Assert(true, "This decorator, InverterNode, doesn't have any childnodes. Please fix your error");

            BehaviourState nodeState = m_ChildNodes[0].UpdateNodeBehaviour();

            switch (nodeState)
            {
                //if the child node has failed, then return succeed
                case BehaviourState.Failed:
                    return BehaviourState.Succeed;

                //if the child node has succeeded, then return failed
                case BehaviourState.Succeed:
                    return BehaviourState.Failed;

                //by default if the node didn't succeed or fail it must be running so return running
                default:
                    return BehaviourState.Running;
            }            
        }
    }
}