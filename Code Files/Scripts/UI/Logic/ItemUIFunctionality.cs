using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemUIFunctionality : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private bool isHovering = false;
    private bool isClicking = false;

    void Start()
    {

    }

    void Update()
    {

    }

    public bool CheckForMouseOver()
    {
        return isHovering;
    }

    public bool CheckForMouseClick()
    {
        return isClicking;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isClicking = true;
    }

    public void SetIsHovering(bool flag)
    {
        isHovering = flag;
    }
}
