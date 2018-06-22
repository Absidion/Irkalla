//Writer: Liam
//Last Updated: Liam 1/2/2018

using System;

namespace TheNegative.AI.Node
{
    //The parrallel node will run a node's Updatebehaviour and then check the result. Based on the result it will act upon either a success condition or a failure condition.
    public class ParallelNode : Node
    {
        private string m_Name = string.Empty;           //The name of the parallel node

        private Node m_ConditionToCheck = null;         //The condition that the Node must check
        private Node m_SucceedCondition = null;         //The success condition that will be acted upon if the condition to check is successful
        private Node m_FailCondition = null;            //The failure condition that will be acted upon if the condition to check is unsucessful

        public ParallelNode(AI reference, string name, Node condition, Node succeedCon, Node failCon) : base(reference)
        {
            m_Name = name;

            m_ConditionToCheck = condition;
            m_SucceedCondition = succeedCon;
            m_FailCondition = failCon;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {            
            BehaviourState result = m_ConditionToCheck.UpdateNodeBehaviour();

            //check to see whether the result is successful or a fail
            switch (result)
            {
                case BehaviourState.Failed:
                    result = m_FailCondition.UpdateNodeBehaviour();
                    break;

                case BehaviourState.Succeed:
                    result = m_SucceedCondition.UpdateNodeBehaviour();
                    break;
            }

            return result;
        }

        public override void Init()
        {
            m_ConditionToCheck.Init();
            m_FailCondition.Init();
            m_SucceedCondition.Init();
        }

        public override void Reset()
        {
            m_ConditionToCheck.Reset();
            m_FailCondition.Reset();
            m_SucceedCondition.Reset();
        }

        public override void Stop()
        {
            m_ConditionToCheck.Stop();
            m_FailCondition.Stop();
            m_SucceedCondition.Stop();
        }
    }
}