using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritPoint : MonoBehaviour
{
    [SerializeField]
    private int m_DamageMultiplier = 2;

    public int DamageMultiplier
    {
        get
        {
            return m_DamageMultiplier;
        }
    }
}
