using System.Collections.Generic;
using TheNegative.AI;
using UnityEngine;

public class DOT : LiamBehaviour
{
    public enum TargetMask { Enemy, Player }

    public float DamageInterval = 0.1f;                         //How frequently damage will be dealt
    public int DamagePerInterval = 2;                           //Damage dealt per interval
    public bool UseTrigger = true;                              //Whether or not the DOT collider should be a trigger or collider
    public TargetMask TargetType = TargetMask.Enemy;            //The target enemy that will be targeted
    public Status[] StatusDamage = null;                        //Array of statuses that this DOT can deal

    private Collider m_Collider;                                //Collider used for determinning damage
    private float m_Timer = 0.0f;                               //Timer for how long it's been since damage has been dealt

    private int m_OwnersID = -1;                                //The owning gameobject's id (Player = playerNumber, AI = parsed AI name)
    private List<Transform> m_DamagedTargets;                   //Used to make sure that an object with multiple sub-colliders doesn't take damage multiple times
    
    protected override void Awake()
    {
        base.Awake();

        m_Collider = gameObject.GetComponent<Collider>();
        m_Collider.isTrigger = UseTrigger;

        m_DamagedTargets = new List<Transform>();        
    }

    protected void OnValidate()
    {
        if (m_Collider != null)
            m_Collider.isTrigger = UseTrigger;
    }

    protected void OnEnable()
    {
        if (m_DamagedTargets != null)
            m_DamagedTargets.Clear();
    }

    protected override void Update()
    {
        base.Update();

        m_Timer += Time.deltaTime;

        if (m_DamagedTargets.Count > 0)
        {
            foreach (Transform target in m_DamagedTargets)
            {
                if (target == null)
                    continue;
                //Determining the base target type and then deal the proper type of damage
                switch (TargetType)
                {
                    case TargetMask.Enemy:
                        AITakeDamageInterface ai = target.GetComponent<AITakeDamageInterface>();
                        ai.TakeDamage(OwnersID, DamagePerInterval, null, 1);
                        break;

                    case TargetMask.Player:
                        Player player = target.GetComponent<Player>();
                        player.TakeDamage(DamagePerInterval, Vector3.zero, null);
                        break;
                }
            }
            m_DamagedTargets.Clear();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //the target doesn't match the designated type so don't continue logic
        if (other.tag != TargetType.ToString() || m_Timer < DamageInterval)
            return;

        //if the list of target's to damage already contains the top level transform of the targeted object
        //then ignore it and don't add it to the list of targets
        if (!m_DamagedTargets.Contains(other.transform.root))
            //otherwise add it to the list of targets
            m_DamagedTargets.Add(other.transform.root);
    }

    public int OwnersID { get { return m_OwnersID; } set { m_OwnersID = value; } }
}
