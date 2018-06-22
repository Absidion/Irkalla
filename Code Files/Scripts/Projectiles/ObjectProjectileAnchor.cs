using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheNegative.Items;

//Author: James
//Last edited: James 12/1/2017
public class ObjectProjectileAnchor : ObjectProjectile
{       
    //protected Vector3 m_Displacement = Vector3.zero;            //The displacement between the Player and the target being Pulled
    //private bool m_isPulling = false;                           //A check for if we are currently pulling the target or not
    //private Collider m_PulledTarget = null;                     //Stores the target being pulled so we can updated the target's position as it gets pulled towards the player.
    //private float m_Force = 0.0f;

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (((1 << other.gameObject.layer) & m_LayerMask) != 0) //if the collided gameobject is on the layer provided
    //    {
    //        if (other.tag == "Enemy")
    //        {
    //           m_PulledTarget = other;               
    //        }
    //    }
    //}

    //protected override void Update()
    //{
    //    if (m_PulledTarget != null)
    //    {
    //        m_PulledTarget.GetComponent<Rigidbody>().isKinematic = false;
    //        m_Force = m_PulledTarget.GetComponent<Rigidbody>().mass;
    //        m_Displacement = m_Player.transform.position - m_PulledTarget.transform.position;
    //        if (!m_isPulling)
    //        {   
    //            m_PulledTarget.GetComponent<Rigidbody>().AddForce((m_Displacement * m_Force), ForceMode.Impulse);
    //            this.GetComponent<Rigidbody>().AddForce(m_Player.transform.position - this.transform.position, ForceMode.Impulse);
    //            m_isPulling = true;
    //        }

    //        if (m_Displacement.magnitude < 3)
    //        {
    //            m_PulledTarget.GetComponent<Rigidbody>().isKinematic = true;
    //            m_PulledTarget = null;
    //            gameObject.SetActive(false);
    //        }
    //    }
    //}
}
