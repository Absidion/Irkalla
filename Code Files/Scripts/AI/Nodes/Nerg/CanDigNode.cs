using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.AI.Node
{
    public class CanDigNode : Node
    {
        MasterNergAI masterNergRef;

        public CanDigNode(AI reference) : base(reference)
        {
            masterNergRef = reference as MasterNergAI;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (!masterNergRef.HasDug)
            {
                masterNergRef.HasDug = true;
                return BehaviourState.Succeed;
            }
            else
                return BehaviourState.Failed;
        }
    }
}
