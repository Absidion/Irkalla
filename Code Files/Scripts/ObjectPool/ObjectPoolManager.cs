using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

//Writer: Liam
//Last Updated: Liam 11/30/2017

public class ObjectPoolManager : Photon.MonoBehaviour
{
    static public ObjectPoolManager Instance;                   //singleton instance of the objectpoolmanager
    static public int UniqueObjectName = 0;                     //A unique name for objects that get added to network pools

    public int DefaultMinimumSize = 10;                         //default minimum size of object pools who don't get sizes passed in
    public int DefaultMaximumSize = 20;                         //default maximum size of object pools who don't get sizes passed in
    public bool CanSceneUnload = false;

    private Dictionary<string, NetworkObjectPool> m_OnlinePools;      //the dictionary of managed pools
    private Dictionary<string, OfflineObjectPool> m_OfflinePools;      //the dictionary of managed offline object pools  

    #region Unity Methods
    protected void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            m_OnlinePools = new Dictionary<string, NetworkObjectPool>();
            m_OfflinePools = new Dictionary<string, OfflineObjectPool>();
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            GameManager.FinalizeObjectPools += FinalizeObjectPools;
        }
        else
        {
            gameObject.SetActive(false);
            Destroy(this.gameObject);
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (CanSceneUnload)
        {
            //clear out no longer useful pools from the objectpool manager
            if (m_OnlinePools != null)
            {
                List<NetworkObjectPool> poolsToRemove = new List<NetworkObjectPool>();

                foreach (NetworkObjectPool netPool in m_OnlinePools.Values)
                {
                    foreach (GameObject obj in netPool.Pool)
                    {
                        if (obj != null)
                        {
                            obj.SetActive(false);
                            if (PhotonNetwork.isMasterClient && !netPool.DontDestroyOnLoad)
                            {
                                PhotonNetwork.Destroy(obj);
                            }
                        }
                    }

                    if (!netPool.DontDestroyOnLoad || netPool.Pool.Count == 0)
                        poolsToRemove.Add(netPool);
                }

                foreach (NetworkObjectPool pool in poolsToRemove)
                {
                    m_OnlinePools.Remove(pool.Key);
                }
            }

            //clear out no longer useful pools from the object pool manager
            if (m_OfflinePools != null)
            {
                List<OfflineObjectPool> poolsToRemove = new List<OfflineObjectPool>();

                foreach (OfflineObjectPool offlinePool in m_OfflinePools.Values)
                {
                    foreach (GameObject obj in offlinePool.Pool)
                    {
                        if (obj != null)
                        {
                            obj.SetActive(false);
                            if (!offlinePool.DontDestroyOnLoad)
                            {
                                Destroy(obj);
                            }
                        }
                    }

                    if (!offlinePool.DontDestroyOnLoad || offlinePool.Pool.Count == 0)
                        poolsToRemove.Add(offlinePool);
                }

                foreach (OfflineObjectPool pool in poolsToRemove)
                {
                    m_OfflinePools.Remove(pool.Key);
                }
            }

            CanSceneUnload = false;
        }
    }

    private void FinalizeObjectPools(object sender, System.EventArgs args)
    {
        //if you're not the master client do not execute on this logic
        if(PhotonNetwork.isMasterClient)
        {
            //iterate over every single network pool that the master client has and we will re-create these pools on the other client
            foreach(NetworkObjectPool pool in m_OnlinePools.Values)
            {
                //rpc to make sure the pools exisits before finding objects and filling it
                photonView.RPC("RPCMakeSurePoolExists", PhotonTargets.Others, pool.Key, pool.ObjectName, pool.InitialSize, pool.MaxSize, pool.DontDestroyOnLoad);
                foreach(GameObject obj in pool.Pool)
                {
                    //rpc to find the object by name in the scene and add it to the already created pool
                    photonView.RPC("RPCSyncPool", PhotonTargets.Others, pool.Key, obj.name);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (m_OnlinePools != null)
            foreach (NetworkObjectPool netPool in m_OnlinePools.Values)
            {
                foreach (GameObject obj in netPool.Pool)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);
                        Destroy(obj);
                    }
                }
            }

        if (m_OfflinePools != null)
            foreach (OfflineObjectPool offlinePool in m_OfflinePools.Values)
            {
                foreach (GameObject obj in offlinePool.Pool)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);
                        Destroy(obj);
                    }
                }
            }
    }
    #endregion

    #region Network Pool

    //returns an object pool from the manager's dictionary
    public NetworkObjectPool GetNetworkPoolFromManager(string name)
    {
        if (!m_OnlinePools.ContainsKey(name))
        {
            Debug.Assert(true, "Object pool with name, " + name + " doesn't exist in the current context.");
            return null;
        }

        return m_OnlinePools[name];
    }

    //returns an object from the object pool relative to the name passed in
    public GameObject GetObjectFromNetworkPool(string name)
    {
        if (!m_OnlinePools.ContainsKey(name))
        {
            Debug.Assert(true, "Object pool with name, " + name + " doesn't exist in the current context.");
            return null;
        }
        return m_OnlinePools[name].GetObjectFromPool();
    }


    public void CreateNetworkPoolWithName(string name, string referenceName, bool dontDestroyOnLoad)
    {
        //create the object pool
        if (!m_OnlinePools.ContainsKey(name))
        {
            NetworkObjectPool pool = new NetworkObjectPool(name, DefaultMinimumSize, DefaultMaximumSize, referenceName, dontDestroyOnLoad);
            m_OnlinePools.Add(name, pool);
        }
    }

    public void CreateNetworkPoolWithName(string name, string referenceName, int minSize, int maxSize, bool dontDestroyOnLoad)
    {
        if (!m_OnlinePools.ContainsKey(name))
        {
            NetworkObjectPool pool = new NetworkObjectPool(name, minSize, maxSize, referenceName, dontDestroyOnLoad);
            m_OnlinePools.Add(name, pool);
        }
    }
    #endregion

    #region Offline Pool
    public OfflineObjectPool GetOfflinePoolFromManager(string name)
    {
        if (!m_OfflinePools.ContainsKey(name))
        {
            Debug.Assert(true, "Object pool with name, " + name + " doesn't exist in the current context.");
            return null;
        }

        return m_OfflinePools[name];
    }

    public GameObject GetObjectFromOfflinePool(string name)
    {
        if (!m_OfflinePools.ContainsKey(name))
        {
            Debug.Assert(true, "Object pool with name, " + name + " doesn't exist in the current context.");
            return null;
        }

        return m_OfflinePools[name].GetObjectFromPool();
    }

    public void CreateOfflinePoolWithName(string name, GameObject reference, bool dontDestroyOnLoad)
    {
        if (!m_OfflinePools.ContainsKey(name))
        {
            OfflineObjectPool pool = new OfflineObjectPool(name, DefaultMinimumSize, DefaultMaximumSize, reference, dontDestroyOnLoad);
            m_OfflinePools.Add(name, pool);
        }
    }

    public void CreateOfflinePoolWithName(string name, int minSize, int maxSize, GameObject reference, bool dontDestroyOnLoad)
    {
        if (!m_OfflinePools.ContainsKey(name))
        {
            OfflineObjectPool pool = new OfflineObjectPool(name, minSize, maxSize, reference, dontDestroyOnLoad);
            m_OfflinePools.Add(name, pool);
        }
    }
    #endregion

    #region Sync Object Pool Accross Network
    [PunRPC]
    public void RPCMakeSurePoolExists(string key, string referenceName, int minSize, int maxSize, bool dontDestroyOnLoad)
    {
        if(!m_OnlinePools.ContainsKey(key))
        {
            NetworkObjectPool pool = new NetworkObjectPool(key, minSize, maxSize, referenceName, dontDestroyOnLoad, false);
            m_OnlinePools.Add(key, pool);
        }
    }

    [PunRPC]
    public void RPCSyncPool(string key, string objectName)
    {
        if(m_OnlinePools.ContainsKey(key))
        {
            GameObject[] everyObjectInTheSceneGodWhy = Resources.FindObjectsOfTypeAll<GameObject>();
            GameObject obj = null;
            foreach(GameObject currentOBJ in everyObjectInTheSceneGodWhy)
            {
                if(currentOBJ.name == objectName)
                {
                    obj = currentOBJ;
                    break;
                }
            }

            if (obj != null)
            {
                m_OnlinePools[key].Pool.Add(obj);
                if(m_OnlinePools[key].DontDestroyOnLoad)
                {
                    GameManager.Instance.DontDestroyNetworkObject(obj);
                }
            }
        }
    }
    #endregion
}
