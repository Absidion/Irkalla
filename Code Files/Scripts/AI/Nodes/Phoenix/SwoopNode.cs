using UnityEngine;

//Writer: Liam
//Last Updated: Liam 12/30/2017

namespace TheNegative.AI.Node
{
    public class SwoopNode : Node
    {
        private float Cooldown = 5.0f;                   //how long it takes this attack to cooldown
        private float SwoopTime = 5.0f;                  //how long it takes this attack to activate fully
        private float GravityAccel = 10.0f;              //the gravity accelerant

        private float m_CooldownTimer = 0.0f;           //the time since the dive was last used

        private Vector3 m_EndLocation;                  //the end location of the swoop
        private Vector3 m_StartLocation;                //the start location of the swoop

        private float m_SwoopSpeed;                     //the swoop speed of the AI

        private float m_SwoopStartTime;                 //the time that the swoop started at

        public SwoopNode(AI reference) : base(reference) { }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if(m_CooldownTimer > Cooldown)
            {
                //calucate where the AI should be this frame. This will also return whether or not the AI is still running the swoop.
                //If it is the Running will be returned otherwise it'll be Succeed
                return ExecuteSwoop();
            }
            else
            {
                //increment timer
                m_CooldownTimer += Time.deltaTime;

                //if the cooldown is up that means we need to set the swoop data and make the AI look at the target
                if(m_CooldownTimer > Cooldown)
                {
                    CalculateSwoopData();
                    m_AIReference.transform.LookAt(m_AIReference.Target.transform);
                }
            }

            return BehaviourState.Failed;
        }

        private BehaviourState ExecuteSwoop()
        {
            //calcualte the swoop end time
            float endTime = m_SwoopStartTime + SwoopTime;

            if (Time.time >= endTime)
            {
                //incase there is any small veriation in where the AI finished it should be sent to the correct location
                m_AIReference.transform.position = m_EndLocation;
                m_CooldownTimer = 0.0f;

                return BehaviourState.Succeed;
            }

            //get the cur time amount of the swoop and calculate the percentage of the swoop that's done
            float curSwoopTime = Time.time - m_SwoopStartTime;
            float swoopPercent = curSwoopTime / SwoopTime;

            Vector3 newPos = Vector3.Lerp(m_StartLocation, m_EndLocation, swoopPercent);

            float swoopHeight = 0.5f * -GravityAccel * curSwoopTime * curSwoopTime + m_SwoopSpeed * curSwoopTime;
            Debug.Log(swoopHeight);

            newPos.y = swoopHeight + m_StartLocation.y;

            m_AIReference.transform.position = newPos;
                        
            return BehaviourState.Running;
        }

        //calculate the end position of the Swoop
        private void CalculateSwoopData()
        {
            m_StartLocation = m_AIReference.transform.position;

            //calculate the distance to the player * 2 because we want the mid point of the swoop to be the players current position
            float swoopDistance = (m_AIReference.Target.transform.position - m_AIReference.transform.position).magnitude * 2;

            //get the normalized direction to the player's position
            Vector3 normPlayerDir = (m_AIReference.Target.transform.position - m_AIReference.transform.position).normalized;

            //multiply the normalized player direction by the distance to travel and add that value to the AI's current position to get the end location of the swoop
            //after make sure that the y location of the end position is still equal to the ai's current y location for consitancey
            m_EndLocation = m_AIReference.transform.position + (normPlayerDir * swoopDistance);
            m_EndLocation.y = m_AIReference.transform.position.y;

            //calcualte jump speeds
            m_SwoopSpeed = swoopDistance / SwoopTime;

            m_SwoopStartTime = Time.time;
        }
    }
}