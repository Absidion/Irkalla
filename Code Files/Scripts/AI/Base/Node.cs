//Writer: Liam
//Last Updated: Liam 12/28/2017

namespace TheNegative.AI.Node
{
    public abstract class Node
    {
        public bool IsPhysicsBased = false;             //used to tell the behaviour tree that this node is using physics calculations.
        public bool FirstCallActivated = false;         //This value is used to determine if the node has been activated before the reset
        protected AI m_AIReference;                     //the AI that this node is apart of

        public Node(AI reference)
        {
            m_AIReference = reference;
        }

        public abstract BehaviourState UpdateNodeBehaviour();                       //this method must be overriden, it's the behaviour of the node that will be activated
        public virtual void OnFirstTreeCall() { FirstCallActivated = true; }        //this method will get called at the very first call of the current tree pass.
        public virtual void Init() { }                                              //this method will get called after construction of the AI but before the AI's first Update Loop when more objects have spawned into the world
        public virtual void Stop() { }                                              //this method will get called when the AI dies or is removed from the scene. Useful for stopping coroutines
        public virtual void Reset() { FirstCallActivated = false; }                 //this method can be overriden to reset any values you may need to upon completion of node logic
        public virtual void LateUpdate() { }                                        //this method can be overriden and can be used to calculate or activate any thing after all other node logic has been done on the behaviour tree this frame
        public virtual void FixedUpdate() { }                                       //this method can be overriden and can be used to calculate physics based opperations during the physics worlds step.

    }
}