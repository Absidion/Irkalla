using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.AI.Node
{
    public class DelegateNode : Node
    {
        public delegate void Delegate(params object[] args);    //delegat type that can take in any function with any number/type of arguments
        private Delegate m_UniqueFunction = null;               //stores the function that will be called
        private object[] m_Arguments = null;                    //stores the list of arguments for the function that will be called

        public DelegateNode(AI reference, Delegate d, params object[] args) : base(reference)
        {
            m_UniqueFunction = d;
            m_Arguments = args;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            m_UniqueFunction(m_Arguments);
            return BehaviourState.Succeed;
        }
    }
}
