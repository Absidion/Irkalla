using System.Collections.Generic;

//Writer: Liam
//Last Updated: Liam 12/28/2017

namespace TheNegative.AI.Node
{
    //this is an abstract node meant to keep track of child nodes. 
    public abstract class ParentNode : Node
    {
        protected List<Node> m_ChildNodes;              //a list of all the child nodes that this node has
        protected string m_Name;                        //the name of this child parent set
              
        public ParentNode(AI reference, string name) : base(reference)
        {
            m_Name = name;
            m_ChildNodes = new List<Node>();
        }

        public ParentNode(AI reference, string name, params Node[] nodes) : base(reference)
        {
            m_Name = name;
            m_ChildNodes = new List<Node>();

            foreach(Node node in nodes)
            {
                m_ChildNodes.Add(node);
            }
        }

        //calls Init() on all child nodes of this object
        public override void FixedUpdate()
        {
            foreach(Node child in m_ChildNodes)
            {
                child.FixedUpdate();
            }
        }

        //calls reset on all child nodes
        public override void Reset()
        {
            foreach (Node child in m_ChildNodes)
            {
                child.Reset();
            }
        }

        //calls init on all child nodes
        public override void Init()
        {
            foreach(Node child in m_ChildNodes)
            {
                child.Init();
            }
        }

        //calls stop on all child nodes to permently stop and actions it is currently taking
        public override void Stop()
        {
            foreach(Node child in m_ChildNodes)
            {
                child.Stop();
            }
        }

        //calls all the child node's late update
        public override void LateUpdate()
        {
            foreach(Node child in m_ChildNodes)
            {
                child.LateUpdate();
            }
        }

        //adds children into the list of childnodes
        public void AddChildren(params Node[] children)
        {
            foreach (Node child in children)
            {
                m_ChildNodes.Add(child);
            }
        }

        public string Name { get { return m_Name; } }
    }
}
