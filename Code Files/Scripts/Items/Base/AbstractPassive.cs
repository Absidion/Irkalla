using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.Items
{
    [Serializable]
    public abstract class AbstractPassive : AbstractItem
    {
        protected bool m_NeedsUpdate;

        public AbstractPassive()
        {
            m_NeedsUpdate = false;
            m_ItemType = ItemType.Passive;
        }

        public bool NeedsUpdate { get { return m_NeedsUpdate; } set { m_NeedsUpdate = value; } }
    }
}
