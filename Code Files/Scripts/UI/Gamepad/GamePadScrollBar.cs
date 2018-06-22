using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePadScrollBar : MonoBehaviour
{
    private Scrollbar m_scrollBar;

    private void Start()
    {
        m_scrollBar = gameObject.GetComponent<Scrollbar>();
    }

    void Update ()
    {
        //if (m_scrollBar.enabled == true)
        //{
        //    if (Input.GetAxis("LookAxisX") != 0)
        //    {
        //        m_scrollBar.value += Input.GetAxis("LookAxisX");
        //    }
        //}
	}
}
