using System.Collections.Generic;

namespace TheNegative.AI.Node
{
    //Class will run every single child node until one of the child node succeeds
    public class RunUntilSuceed : ParentNode
    {
        public RunUntilSuceed(AI reference, string name) : base(reference, name) { }
        public RunUntilSuceed(AI reference, string name, params Node[] childrenNodes) : base(reference, name, childrenNodes) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            BehaviourState childrenStates = BehaviourState.Running;

            foreach(Node node in m_ChildNodes)
            {
                if (!node.FirstCallActivated)
                    node.OnFirstTreeCall();

                //if at any point any state when updated is sucessful then the turnary below will be sure to check to make sure that it was
                //sucessful. If tempState doesn't equal sucessful then let it remain at childrenStates value
                BehaviourState tempState = node.UpdateNodeBehaviour();
                childrenStates = (tempState == BehaviourState.Succeed) ? tempState : childrenStates;                 
            }

            if(childrenStates == BehaviourState.Succeed)
            {
                base.Reset();
            }

            return childrenStates;
        }
    }
}