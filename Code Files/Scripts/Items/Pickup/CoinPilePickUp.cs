//Writer: Liam
//Last Updated: Liam 12/2/2017

namespace TheNegative.Items
{
    public class CoinPilePickUp : AbstractItem
    {
        private int m_Value = 10;

        public CoinPilePickUp()
        {
            m_Name = "CoinPile";
            m_Description = "A pile of coins";
            m_SpriteName += "CoinPile";
            m_NetworkAffect = false;
            m_NetworkVisual = false;
            m_ItemType = ItemType.PickUp;
        }
        public override void ActivateItem(Player player)
        {
            player.ChangeCurrency(m_Value);

            if (player.photonView.isMine)
            {
                PlayerHUDManager.instance.ToggleCurrency(true, true);
                SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "PickupCurrency", player.transform.position);
            }
        }
    }
}