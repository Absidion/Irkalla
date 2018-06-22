using UnityEngine;

namespace TheNegative.Items
{
    public class EyeOfTheBeholder : AbstractActiveTimerBased
    {
        private DOT m_EyeNetworkBehaviour;

        private float m_Duration = 5.0f;
        private float m_DurationTimer = 0.0f;

        public EyeOfTheBeholder()
        {
            m_Name = "EyeOfTheBeholder";
            m_SpriteName += m_Name;
            m_Description = "Only death is at the end of this rainbow.";
            m_Tooltip = "Fire a destructive beam.";
            m_CooldownTime = 45.0f;
            m_Timer = m_CooldownTime;
            m_IsActivated = false;
        }

        public override void ActivateItem(Player player)
        {
            base.ActivateItem(player);
            if (player.photonView.isMine)
            {
                GameObject go = PhotonNetwork.Instantiate("Items/Meshes/" + m_Name.Replace("TheNegative.Items.", ""), player.ActiveItemLocation.position, player.ActiveItemLocation.rotation, 0);
                m_EyeNetworkBehaviour = go.GetComponentInChildren<DOT>();
                m_EyeNetworkBehaviour.OwnersID = player.PlayerNumber;
                m_EyeNetworkBehaviour.SetActive(false);
                go.transform.parent = player.ActiveItemLocation;
                go.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
            }
        }

        public override void UpdateItem()
        {
            //if the eye isn't activated, activate it at the correct location
            if (!m_IsActivated && m_Timer > m_CooldownTime)
            {
                m_IsActivated = true;
                m_EyeNetworkBehaviour.SetActive(true);
            }

            Debug.Log(m_DurationTimer);

            //while the eye is active make sure to increment its timer
            if (m_IsActivated)
            {
                m_DurationTimer += Time.deltaTime;
                if (m_DurationTimer >= m_Duration)
                {
                    m_IsActivated = false;
                    m_EyeNetworkBehaviour.SetActive(false);
                    m_DurationTimer = 0.0f;
                    m_Timer = 0.0f;
                }
            }
        }
    }
}