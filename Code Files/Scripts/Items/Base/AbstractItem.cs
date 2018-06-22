using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.Items
{
    [Serializable]
    public abstract class AbstractItem
    {
        protected bool m_NetworkAffect = false;
        protected bool m_NetworkVisual = false;
        protected string m_Name = string.Empty;
        protected string m_Description = string.Empty;
        protected ItemType m_ItemType;
        protected string m_SpriteName = "Items/Sprites/";
        protected string m_Tooltip = string.Empty;

        public bool NetworkAffect { get { return m_NetworkAffect; } set { m_NetworkAffect = value; } }
        public bool NetworkVisual { get { return m_NetworkVisual; } set { m_NetworkVisual = value; } }
        public string Name { get { return m_Name; } set { m_Name = value; } }
        public string Description { get { return m_Description; } set { m_Description = value; } }
        public ItemType ItemType { get { return m_ItemType; } set { m_ItemType = value; } }
        public string SpriteName { get { return m_SpriteName; } set { m_SpriteName = value; } }
        public string Tooltip { get { return m_Tooltip; } set { m_Tooltip = value; } }

        public virtual void UpdateItem() { }
        public virtual void UpdateNetworkAffect() { }
        public virtual void UpdateNetworkVisual() { }
        public virtual bool CanObtainItem(Player player) { return true; }
        public abstract void ActivateItem(Player player);
        
    }

    public enum ItemType
    {
        PickUp,
        ActiveTimer,
        ActiveRoom,
        Passive,
        NULL
    }
}
