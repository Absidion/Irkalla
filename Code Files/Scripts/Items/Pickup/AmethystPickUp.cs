//Writer: Liam
//Last Updated: Liam 12/2/2017

namespace TheNegative.Items
{
    public class AmethystPickUp : AbstractItem
    {
        private int m_Value = 25;

        public AmethystPickUp()
        {
            m_Name = "Amethyst";
            m_Description = "A shiny amethyst, worth a high price";
            m_SpriteName += "Amethyst";
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