using UnityEngine;

namespace TheNegative.Items
{
    static public class PickupDetails
    {
        #region Droppable items percentages
        private const float m_HeartPercentage = 25.0f;
        private const float m_ArmorPercentage = 40.0f;
        private const float m_ChestPercentage = 45.0f;
        private const float m_CoinPercentage = 78.0f;
        private const float m_CoinPilePercentage = 91.75f;
        private const float m_AmethystPercentage = 94.5f;
        private const float m_EmeraldPercentage = 97.25f;
        private const float m_RubyPercentage = 100.0f;
        #endregion

        #region Actual item drop values
        private const float m_HeartDropValue = 25.0f;
        private const float m_ArmorDropValue = 15.0f;
        private const float m_ChestDropValue = 5.0f;
        private const float m_CoinDropValue = 33.0f;
        private const float m_CoinPileDropValue = 13.75f;
        private const float m_AmethystDropValue = 2.75f;
        private const float m_EmeraldDropValue = 2.75f;
        private const float m_RubyDropValue = 2.75f;
        #endregion
        
        static public DroppableItems GetRandomNonChestItem()
        {
            float randomValue = Random.Range(0, 95.0f);

            DroppableItems droppedItem = DroppableItems.Ruby;

            //set the object getting dropped to be heart
            if (randomValue >= 0.0f && randomValue < m_HeartPercentage)
            {
                droppedItem = DroppableItems.Heart;
            }
            //set the object getting dropped to be armor
            else if (randomValue >= m_HeartPercentage && randomValue < m_ArmorPercentage)
            {
                droppedItem = DroppableItems.Armor;
            }
            //set the object getting dropped to be chest
            else if (randomValue >= m_ArmorPercentage && randomValue < m_CoinPercentage)
            {
                droppedItem = DroppableItems.Coin;
            }
            //set the object getting dropped to be coin pile
            else if (randomValue >= m_CoinPercentage && randomValue < m_CoinPilePercentage)
            {
                droppedItem = DroppableItems.CoinPile;
            }
            //set the object getting dropped to be amethyst
            else if (randomValue >= m_CoinPilePercentage && randomValue < m_AmethystPercentage)
            {
                droppedItem = DroppableItems.Amethyst;
            }
            //set the object getting dropped to be emerald
            else if (randomValue >= m_AmethystPercentage && randomValue < m_EmeraldPercentage)
            {
                droppedItem = DroppableItems.Emerald;
            }
            //set the object getting dropped to be ruby
            else if (randomValue >= m_EmeraldPercentage && randomValue < m_RubyPercentage)
            {
                droppedItem = DroppableItems.Ruby;
            }

            return droppedItem;
        }

        static public DroppableItems GetRandomItem()
        {
            float randomValue = Random.Range(0, 100.0f);

            DroppableItems droppedItem = DroppableItems.Ruby;

            //set the object getting dropped to be heart
            if (randomValue >= 0.0f && randomValue < m_HeartPercentage)
            {
                droppedItem = DroppableItems.Chest;
            }
            //set the object getting dropped to be armor
            else if (randomValue >= m_HeartPercentage && randomValue < m_ArmorPercentage)
            {
                droppedItem = DroppableItems.Armor;
            }
            //set the object getting dropped to be chest
            else if (randomValue >= m_ArmorPercentage && randomValue < m_ChestPercentage)
            {
                //CHANGE BACK
                droppedItem = DroppableItems.Chest;
            }
            //set the object getting dropped to be coin
            else if (randomValue >= m_ChestDropValue && randomValue < m_CoinPercentage)
            {
                droppedItem = DroppableItems.Coin;
            }
            //set the object getting dropped to be coin pile
            else if (randomValue >= m_CoinPercentage && randomValue < m_CoinPilePercentage)
            {
                droppedItem = DroppableItems.CoinPile;
            }
            //set the object getting dropped to be amethyst
            else if (randomValue >= m_CoinPilePercentage && randomValue < m_AmethystPercentage)
            {
                droppedItem = DroppableItems.Amethyst;
            }
            //set the object getting dropped to be emerald
            else if (randomValue >= m_AmethystPercentage && randomValue < m_EmeraldPercentage)
            {
                droppedItem = DroppableItems.Emerald;
            }
            //set the object getting dropped to be ruby
            else if (randomValue >= m_EmeraldPercentage && randomValue < m_RubyPercentage)
            {
                droppedItem = DroppableItems.Ruby;
            }

            return droppedItem;
        }
        
        static public string GetDroppableItemAssembly(DroppableItems itemToGetAssemblyOf)
        {
            string assembly = string.Empty;

            switch (itemToGetAssemblyOf)
            {
                case DroppableItems.Heart:
                    assembly = "TheNegative.Items.HeartPickUp";
                    break;
                case DroppableItems.Armor:
                    assembly = "TheNegative.Items.ArmorPickUp";
                    break;
                case DroppableItems.Coin:
                    assembly = "TheNegative.Items.CoinPickUp";
                    break;
                case DroppableItems.CoinPile:
                    assembly = "TheNegative.Items.CoinPilePickUp";
                    break;
                case DroppableItems.Amethyst:
                    assembly = "TheNegative.Items.AmethystPickUp";
                    break;
                case DroppableItems.Emerald:
                    assembly = "TheNegative.Items.EmeraldPickUp";
                    break;
                case DroppableItems.Ruby:
                    assembly = "TheNegative.Items.RubyPickUp";
                    break;
                default:
                    break;
            }

            return assembly;
        }

        //gets the drop chance of an item passed in
        static public float GetDroppableItemPercentage(DroppableItems itemToFindValueFrom)
        {
            switch (itemToFindValueFrom)
            {
                case DroppableItems.Heart:
                    return m_HeartDropValue;

                case DroppableItems.Armor:
                    return m_ArmorDropValue;

                case DroppableItems.Chest:
                    return m_ChestDropValue;

                case DroppableItems.Coin:
                    return m_CoinDropValue;

                case DroppableItems.CoinPile:
                    return m_CoinPileDropValue;

                case DroppableItems.Amethyst:
                    return m_AmethystDropValue;

                case DroppableItems.Emerald:
                    return m_EmeraldDropValue;

                case DroppableItems.Ruby:
                    return m_RubyDropValue;

                default:
                    return -1;
            }
        }

        static public DroppableItems GetDroppableItemFromRoom(RoomType room, float playerLuck)
        {
            //default it to null because certain rooms cannot drop items
            DroppableItems itemToReturn = DroppableItems.NULL;

            float randomValue = UnityEngine.Random.Range(0.0f, 1.0f);

            switch (room)
            {
                //In a small room the only things that can drop are: Coins 80% and Hearts 80%
                case RoomType.Small:
                    if (randomValue < (0.8f - (playerLuck * 0.1f)))
                        itemToReturn = DroppableItems.Coin;
                    else
                        itemToReturn = DroppableItems.Heart;
                    break;

                //In a medium room the only things that can drop are: Heart 40%, Armor 40% and CoinPile 20% 
                case RoomType.Medium:
                    if (randomValue >= 0.0f && randomValue < (0.4f - (playerLuck * 0.1f)))
                        itemToReturn = DroppableItems.Heart;
                    else if (randomValue >= (0.4f - (playerLuck * 0.1f)) && randomValue < (0.8f - (playerLuck * 0.1f)))
                        itemToReturn = DroppableItems.Armor;
                    else
                        itemToReturn = DroppableItems.CoinPile;
                    break;

                //In a large room the only things that can drop are: Armor 25%, Heart 25%, CoinPile 10%, Ruby 10%, Emerald 10%, Amethyst 10% and Chest 10%
                case RoomType.Large:
                    if (randomValue >= 0.0f && randomValue < (0.25f - (playerLuck * 0.1f)))
                        itemToReturn = DroppableItems.Armor;

                    else if (randomValue >= (0.25f - (playerLuck * 0.1f)) && randomValue < (0.5f - (playerLuck * 0.1f)))
                        itemToReturn = DroppableItems.Heart;

                    else if (randomValue >= (0.5f - (playerLuck * 0.1f)) && randomValue < (0.6f - (playerLuck * 0.1f)))
                        itemToReturn = DroppableItems.Amethyst;

                    else if (randomValue >= (0.6f - (playerLuck * 0.1f)) && randomValue < (0.7f - (playerLuck * 0.1f)))
                        itemToReturn = DroppableItems.CoinPile;

                    else if (randomValue >= (0.7f - (playerLuck * 0.1f)) && randomValue < (0.8f - (playerLuck * 0.1f)))
                        itemToReturn = DroppableItems.Emerald;

                    else if (randomValue >= (0.8f - (playerLuck * 0.1f)) && randomValue < (0.9f - (playerLuck * 0.1f)))
                        itemToReturn = DroppableItems.Ruby;

                    else
                        itemToReturn = DroppableItems.Chest;

                    break;

                case RoomType.ExtraLarge:
                    itemToReturn = DroppableItems.Chest;
                    break;
            }

            return itemToReturn;
        }
    }

    public enum DroppableItems
    {
        Heart,
        Armor,
        Chest,
        Coin,
        CoinPile,
        Amethyst,
        Emerald,
        Ruby,
        NULL
    }
}