using UnityEngine;

namespace TheNegative.AI.Node
{
    public class AnimatorNode : Node
    {
        private Animator m_Animator;                            //Reference to the AI's Animator reference
        private bool m_IsAnimationFinished = false;             //Determines if the Animation is finished playing
        private string m_AnimationBoolName = string.Empty;      //The name of the animation bool that is going to be toggled on

        public AnimatorNode(AI reference, Animator animator, string animationName, ref LinkNodeConnector linkDelegate) : base(reference)
        {
            linkDelegate = Link;
            m_Animator = animator;
            m_AnimationBoolName = animationName;
        }

        public override void OnFirstTreeCall()
        {
            base.OnFirstTreeCall();
            //Set the Animation bool to be true
            m_Animator.SetBool(m_AnimationBoolName, true);
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            //if the animation is finished then return suceed else return running
            return m_IsAnimationFinished ? BehaviourState.Succeed : BehaviourState.Running;
        }

        //The link method is meant to be linked in the AI class for that specific animation's call back event.
        //When the event plays call this method from the event to get the animation to finish playing and the node to continue
        public void Link()
        {
            m_IsAnimationFinished = true;
            m_Animator.SetBool(m_AnimationBoolName, false);
        }

        public override void Reset()
        {
            base.Reset();
            m_IsAnimationFinished = false;
            m_Animator.SetBool(m_AnimationBoolName, false);
        }

        public override void Stop()
        {
            base.Stop();
            m_IsAnimationFinished = false;
            m_Animator.SetBool(m_AnimationBoolName, false);
        }
    }

    public delegate void LinkNodeConnector();
}