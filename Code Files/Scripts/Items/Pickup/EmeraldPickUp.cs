//Writer: Liam
//Last Updated: Liam 12/2/2017

namespace TheNegative.Items
{
    public class EmeraldPickUp : AbstractItem
    {
        private int m_Value = 25;

        public EmeraldPickUp()
        {
            m_Name = "Emerald";
            m_Description = "A shiny Emerald, worth a high price";
            m_SpriteName += "Emerald";
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