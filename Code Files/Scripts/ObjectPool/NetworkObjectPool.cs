using System.Collections.Generic;
using UnityEngine;

//Author: Josue
//Last edited: Josue 10/2/2017
public class NetworkObjectPool
{
    private List<GameObject> m_Pool;                //list containing all of the pool objects
    private int m_MaxSize;                          //maximum number of elements the pool can have
    private int m_InitialSize;                      //initial number of element that are instantiated
    private string m_ObjectName;                    //name of object to spawn
    private bool m_DontDestroyOnLoad;               //whether or not the objects in the pool should be destroyed on loading
    private string m_Key;

    public NetworkObjectPool(string key, int initialSize, int maxSize, string name, bool dontDestroyOnLoad, bool createObjects = true)
    {
        m_Key = key;

        //Make sure each size is at least equal to one
        m_MaxSize = Mathf.Max(1, maxSize);
        m_InitialSize = Mathf.Max(1, initialSize);

        m_ObjectName = name;

        m_Pool = new List<GameObject>();

        m_DontDestroyOnLoad = dontDestroyOnLoad;

        if(createObjects)
            Grow();
    }

    //Returns an object from the pool if available
    public GameObject GetObjectFromPool()
    {
        //Try to find and grab an inactive object from the pool
        GameObject newObject = FindInactiveObjectInPool();

        if (newObject == null)
        {
            //if no inactive objects available, try and grow the pool out and try again
            Grow();
            return FindInactiveObjectInPool();
        }

        return newObject;
    }

    //Returns any inactive objects from the pool. Otherwise, returns null
    private GameObject FindInactiveObjectInPool()
    {
        for (int i = 0; i < m_Pool.Count; i++)
        {
            if (m_Pool[i].activeInHierarchy == false)
            {
                return m_Pool[i];
            }
        }

        return null;
    }

    //Grows out the size of the list and instantiates more objects if more are needed. Size doubles each time until max size is reached.
    private void Grow()
    {
        int growSize = 0;

        if (m_Pool.Count == 0) //if instantiating objects for the first time, use the initial size provided
        {
            growSize = m_InitialSize;
        }
        else if (m_Pool.Count >= m_MaxSize) //if pool has already passed the max size, it will not be able to grow anymore
        {
            growSize = m_MaxSize;
        }
        else
        {
            growSize = Mathf.Min(m_Pool.Count * 2, m_MaxSize); //otherwise, double the size of the pool
        }


        //instantiate over the network, set object to inactive and add to the pool
        for (int i = m_Pool.Count; i < growSize; i++)
        {
            GameObject gameObject = PhotonNetwork.Instantiate(m_ObjectName, new Vector3(0, 0, 0), Quaternion.identity, 0, null);

            string uniqueName = gameObject.name.Replace("(Clone)", ObjectPoolManager.UniqueObjectName.ToString());
            ObjectPoolManager.UniqueObjectName++;

            LiamBehaviour behaviour = gameObject.GetComponent<LiamBehaviour>();

            if (behaviour != null)
            {
                behaviour.SetActive(false);
                behaviour.SetName(uniqueName);
            }
            else
            {
                gameObject.SetActive(false);
            }

            m_Pool.Add(gameObject);

            if (m_DontDestroyOnLoad)
            {
                GameManager.Instance.DontDestroyNetworkObject(gameObject);
            }
        }
    }

    public int MaxSize { get { return m_MaxSize; } }
    public List<GameObject> Pool { get { return m_Pool; } }
    public bool DontDestroyOnLoad { get { return m_DontDestroyOnLoad; } }
    public string Key { get { return m_Key; } }
    public int InitialSize { get { return m_InitialSize; } set { m_InitialSize = value; } }
    public string ObjectName { get { return m_ObjectName; } set { m_ObjectName = value; } }
}
