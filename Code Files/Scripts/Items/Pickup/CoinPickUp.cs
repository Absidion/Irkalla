//Writer: Liam
//Last Updated: Liam 12/2/2017

namespace TheNegative.Items
{
    public class CoinPickUp : AbstractItem
    {
        private int m_Value = 2;

        public CoinPickUp()
        {
            m_Name = "Coin";
            m_Description = "A shiny coin, worth a price";
            m_SpriteName += "Coin";
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