using UnityEngine;

namespace TheNegative.AI.Node
{
    public class ChooseRandomChildNode : ParentNode
    {
        private int m_RandomIndex = 0;              //The random index of the child node that will be acted upon

        public ChooseRandomChildNode(AI reference, string name, params Node[] children) : base(reference, name, children)
        {
            m_RandomIndex = Random.Range(0, m_ChildNodes.Count);
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            BehaviourState result = m_ChildNodes[m_RandomIndex].UpdateNodeBehaviour();

            if(result == BehaviourState.Succeed)
            {
                m_RandomIndex = Random.Range(0, m_ChildNodes.Count);
            }

            return result;
        }
    }
}