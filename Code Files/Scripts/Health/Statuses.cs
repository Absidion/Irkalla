using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Status
{
    None,
    Burn,
    Freeze,
    Poison,
    Stun,
    Slow,
    Bleed,
    Hitstun,
    Root,
    COUNT
}

public static class StatusManager
{
    #region Damage Over Time Values
    private const int BurnDOT = 5;
    private const int FreezeDOT = 1;
    private const int PoisonDOT = 3;
    private const int BleedDOT = 6;
    #endregion
    #region Status Constant Timers
    private const float BurnTimer = 5.0f;
    private const float FreezeTimer = 5.0f;
    private const float PoisonTimer = 5.0f;
    private const float StunTimer = 2.0f;
    private const float SlowTimer = 5.0f;
    private const float BleedTimer = 3.0f;
    private const float HitstunTimer = 0.2f;
    private const float RootTimer = 0.5f;
    #endregion
    #region Slow Values
    private const float FreezeSlowValue = 0.1f;
    private const float SlowValue = 0.2f;
    private const float RootValue = 1.0f;
    #endregion

    private static List<Vignette> m_StatusVignettes;
    private static bool m_VignetteListCreated = false;

    static StatusManager()
    {
        //initialize object pools for all statuses, start at index 1 to ignore 'None'
        //for (int i = 0; i < (int)Status.COUNT; i++)
        {
            string name = "Effects/" + ((Status)1).ToString() + "WorldEffect";
            GameObject obj = Resources.Load<GameObject>(name);
            ObjectPoolManager.Instance.CreateOfflinePoolWithName(name, obj, true);
        }
    }

    public static void SetStatusShader(Status statusEffect)
    {
        if (!m_VignetteListCreated)
            CreateStatusList();

        foreach(Vignette statusVig in m_StatusVignettes)
        {
            if(statusVig.EffectName == statusEffect.ToString())
            {
                statusVig.SetEffect(GetTimerFromStatus(statusEffect));
            }
        }
    }

    public static void CreateStatusList()
    {
        m_StatusVignettes = new List<Vignette>();

        Vignette[] vignettes = GameObject.FindObjectsOfType<Vignette>();

        foreach(Vignette vignette in vignettes)
        {
            m_StatusVignettes.Add(vignette);            
            vignette.enabled = false;
        }

        m_VignetteListCreated = true;
    }


    public static int GetDOTFromStatus(Status status)
    {
        //checks the status and sees if there is a Damage over time value that the status has
        switch(status)
        {
            case Status.Burn:
                return BurnDOT;
                
            case Status.Freeze:
                return FreezeDOT;

            case Status.Poison:
                return PoisonDOT;

            case Status.Bleed:
                return BleedDOT;

            default:
                break;
        }
        return 0;
    }

    public static float GetTimerFromStatus(Status status)
    {
        switch (status)
        {
            case Status.Burn:
                return BurnTimer;

            case Status.Freeze:
                return FreezeTimer;

            case Status.Poison:
                return PoisonTimer;

            case Status.Slow:
                return SlowTimer;

            case Status.Stun:
                return StunTimer;

            case Status.Bleed:
                return BleedTimer;

            case Status.Hitstun:
                return HitstunTimer;

            case Status.Root:
                return RootTimer;

            default:
                break;
        }

        return -1;
    }

    public static float GetSlowValuesFromStatus(Status status)
    {
        switch(status)
        {
            case Status.Freeze:
                return FreezeSlowValue;

            case Status.Slow:
                return SlowValue;

            case Status.Root:
                return RootValue;

            default:
                break;
        }
        return 0;
    }

    public static bool GetIsImmobalizedFromStatus(Status status)
    {
        switch (status)
        {
            case Status.Stun:
                return true;

            case Status.Hitstun:
                return true;

            default:
                break;
        }
        return false;
    }

    public static GameObject GetWorldEffectFromStatus(Status status)
    {
        string name = "Effects/" + status.ToString() + "WorldEffect";
        return ObjectPoolManager.Instance.GetObjectFromOfflinePool(name);
    }
}
