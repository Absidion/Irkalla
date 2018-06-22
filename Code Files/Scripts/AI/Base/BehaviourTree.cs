using UnityEngine;

//Writer: Liam
//Last Update: 12/29/2017

namespace TheNegative.AI.Node
{
    public class BehaviourTree
    {
        private AI m_AIReference;                   //A reference to the AI that this tree is being used
        private SelectorNode m_Root;                //The root node of the behaviour tree

        public BehaviourTree(AI reference)
        {
            m_AIReference = reference;           
        }

        public void UpdateBehaviourTree()
        {
            if(m_Root != null)
            {
                m_Root.UpdateNodeBehaviour();
            }
        }

        public void LateUpdateBehaviourTree()
        {
            if (m_Root != null)
            {
                m_Root.LateUpdate();
            }
        }

        public void FixedUpdateBehaviourTree()
        {
            if (m_Root != null)
            {
                m_Root.FixedUpdate();
            }

        }

        public void Finish()
        {
            m_Root.Stop();
        }

        public SelectorNode RootNode
        {
            get
            {
                return m_Root;
            }
            set
            {
                m_Root = value;
            }
        }
    }
}