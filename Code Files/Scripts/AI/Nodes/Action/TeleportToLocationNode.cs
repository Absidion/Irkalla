using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.AI.Node
{
    public class TeleportToLocationNode : Node
    {
        private Transform m_Location;

        public TeleportToLocationNode(AI reference, Transform location) : base(reference)
        {
            m_Location = location;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            throw new NotImplementedException();
        }
    }
}
