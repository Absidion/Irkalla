using System;
using System.Collections.Generic;
using UnityEngine;
using TheNegative.Items;

[Serializable]
public class Journal
{
    [SerializeField]
    private List<AbstractPassive> m_PassiveItems = new List<AbstractPassive>();
    [SerializeField]
    private AbstractItem m_ActiveItem;
    [SerializeField]
    private Player m_MyPlayer;

    public Journal(Player myPlayer)
    {
        m_MyPlayer = myPlayer;        
    }

    public void UpdatePassiveItems()
    {
        for (int i = 0; i < m_PassiveItems.Count; i++)
        {
            if (m_PassiveItems[i].NeedsUpdate)
            {
                m_PassiveItems[i].UpdateItem();               
            }
        }
    }

    public void UpdatePassiveItemsNetwork()
    {
        for (int i = 0; i < m_PassiveItems.Count; i++)
        {
            if (m_PassiveItems[i].NetworkAffect)
            {
                m_PassiveItems[i].UpdateNetworkAffect();
            }

            if (m_PassiveItems[i].NetworkVisual)
            {
                m_PassiveItems[i].UpdateNetworkVisual();
            }
        }
    }

    public void UpdateActiveItem()
    {
        if (m_ActiveItem == null)
            return;

        if (m_ActiveItem.ItemType == ItemType.ActiveRoom)
        {
            if ((m_ActiveItem as AbstractActiveRoomBased).RoomCooldown == 0)
            {
                if (CharacterController.GetUseItemDown())
                {
                    m_ActiveItem.UpdateItem();
                }
            }
        }
        else if (m_ActiveItem.ItemType == ItemType.ActiveTimer)
        {
            if ((m_ActiveItem as AbstractActiveTimerBased).IsActivated)
            {
                m_ActiveItem.UpdateItem();
            }
            else if ((m_ActiveItem as AbstractActiveTimerBased).Timer > (m_ActiveItem as AbstractActiveTimerBased).CooldownTime)
            {
                if (CharacterController.GetUseItemDown())
                {
                    m_ActiveItem.UpdateItem();
                }
            }
            else
            {
                (m_ActiveItem as AbstractActiveTimerBased).Timer += Time.deltaTime;
            }
        }

    }

    public void UpdateActiveItemNetwork()
    {
        if (m_ActiveItem == null)
            return;

        if (m_ActiveItem.ItemType == ItemType.ActiveRoom)
        {
            if ((m_ActiveItem as AbstractActiveRoomBased).IsActivated)
            {
                ActiveItemNetworkUpdate();
            }
        }
        else if (m_ActiveItem.ItemType == ItemType.ActiveTimer)
        {            
            if ((m_ActiveItem as AbstractActiveTimerBased).IsActivated)
            {
                ActiveItemNetworkUpdate();
            }
        }
    }

    public void AddPassiveItemToJournal(AbstractPassive passiveItem)
    {
        passiveItem.ActivateItem(m_MyPlayer);
        m_PassiveItems.Add(passiveItem);

        if (m_MyPlayer.photonView.isMine)
        {
            PlayerHUDManager.instance.ActivateItemNotification(false);
            JournalUIManager.instance.AddElementToViewport(passiveItem.Name, passiveItem.Tooltip, passiveItem.SpriteName);
        }
    }

    public void AquireNewActiveItem(AbstractItem activeItem)
    {
        activeItem.ActivateItem(m_MyPlayer);
        m_ActiveItem = activeItem;

        if (m_MyPlayer.photonView.isMine)
        {            
            PlayerHUDManager.instance.ActivateItemNotification(true);
            JournalUIManager.instance.AddElementToViewport(activeItem.Name, activeItem.Tooltip, activeItem.SpriteName);
        }
    }

    private void ActiveItemNetworkUpdate()
    {
        if (m_ActiveItem.NetworkAffect)
        {
            m_ActiveItem.UpdateNetworkAffect();
        }

        if (m_ActiveItem.NetworkVisual)
        {
            m_ActiveItem.UpdateNetworkVisual();
        }
    }

    public void DecreaseActiveRoomBasedCooldown()
    {
        if (m_ActiveItem == null)
            return;

        //make sure the item is of type active room based item
        if(m_ActiveItem.ItemType == ItemType.ActiveRoom)
        {
            (m_ActiveItem as AbstractActiveRoomBased).DecrementRoomCooldown();
        }
    }

    public List<AbstractPassive> PassiveItems { get { return m_PassiveItems; } }
    public AbstractItem ActiveItem
    {
        get
        {
            if (m_ActiveItem.ItemType == ItemType.ActiveRoom)
                return m_ActiveItem as AbstractActiveRoomBased;

            if (m_ActiveItem.ItemType == ItemType.ActiveTimer)
                return m_ActiveItem as AbstractActiveTimerBased;

            return null;
        }
    }
}
