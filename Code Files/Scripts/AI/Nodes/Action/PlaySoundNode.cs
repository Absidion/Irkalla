namespace TheNegative.AI.Node
{
    class PlaySoundNode : Node
    {
        private string m_SoundName;
        private float  m_Duration = -1.0f;
        private bool   m_IsLooping = false;

        public PlaySoundNode(AI reference, string soundName, float duration = -1.0f) : base(reference)
        {
            if (duration != -1.0f)
            {
                m_IsLooping = true;
                m_Duration = duration;
            }
            
            m_SoundName = soundName;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (m_IsLooping == true)
            {
                SoundManager.GetInstance().photonView.RPC("PlaySFXFixedDurationNetworked", PhotonTargets.All, m_SoundName, m_Duration, m_AIReference.transform.position);
                return BehaviourState.Succeed;
            }
            else
            {
                SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, m_SoundName, m_AIReference.transform.position);
                return BehaviourState.Succeed;
            }
        }
    }
}