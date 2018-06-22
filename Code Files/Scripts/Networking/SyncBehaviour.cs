using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

//Writer: Liam
//Last Update: 12/17/2017

public class SyncBehaviour : Photon.PunBehaviour
{
    //list of values that will be synced over the network
    private List<FieldInfo> m_MembersMarkedToSync;          //A list of variables in the scene that require syncing accross the network

    protected virtual void Awake()
    {
        m_MembersMarkedToSync = new List<FieldInfo>();

        //using Reflection we get the list of ALL field info in this object. This will collect all inherited memebers as well
        FieldInfo[] fieldInfo = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo info in fieldInfo)
        {
            //loop through every attribute that the current iterated field has, and if any match the attribute of SyncThis then that means it needs to be stored for syncing            
            foreach (object attribute in info.GetCustomAttributes(true))
            {
                if (attribute.GetType() == typeof(SyncThisAttribute))
                {
                    //add the value to the list and then log the addition of it to the members marked to sync list
                    m_MembersMarkedToSync.Add(info);
                    break;
                }
            }
        }
    }

    #region Serialization

    //Syncs every marked variable accross the network
    protected virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //loops through every fieldinfo in memebers marked to sync and the sends the value of it over the network
            foreach (FieldInfo syncthis in m_MembersMarkedToSync)
            {
                stream.SendNext(syncthis.GetValue(this));
            }
        }
        else if (stream.isReading)
        {
            //iterate through the list and recieve the values and set them appropriatly
            foreach (FieldInfo syncthis in m_MembersMarkedToSync)
            {
                RecieveValue(stream, syncthis);
            }
        }
    }

    //checks to see the type stored in the fieldinfo is which is being iterated through in the OnPhotonSerializeView method and then compares it's type to the list
    //of types that we're allowed to sync up with. If the type isn't a syncable type then a debug error message is sent to the user
    private void RecieveValue(PhotonStream stream, FieldInfo info)
    {
        if (info.FieldType == typeof(char))
        {
            info.SetValue(this, (char)stream.ReceiveNext());
        }
        else if (info.FieldType == typeof(short))
        {
            info.SetValue(this, (short)stream.ReceiveNext());
        }
        else if (info.FieldType == typeof(int))
        {
            info.SetValue(this, (int)stream.ReceiveNext());
        }
        else if (info.FieldType == typeof(float))
        {
            info.SetValue(this, (float)stream.ReceiveNext());
        }
        else if (info.FieldType == typeof(bool))
        {
            info.SetValue(this, (bool)stream.ReceiveNext());
        }
        else if (info.FieldType == typeof(string))
        {
            info.SetValue(this, (string)stream.ReceiveNext());
        }
        else if (info.FieldType == typeof(Vector3))
        {
            info.SetValue(this, (Vector3)stream.ReceiveNext());
        }
        else
        {
            info.SetValue(this, stream.ReceiveNext());
           //Debug.LogError("Unhandled type detected and not synced, either don't sync this or speak with his majesty about adding this value into the sync list");
        }
    }
    #endregion
}
