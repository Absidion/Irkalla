using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//Author: Daniel French
//Last edited: Daniel 11/25/2017
public class JournalElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
    public Image ItemSprite;
    public Sprite ToolTipBackgroundSprite;
    public string ItemName;
    public string ItemToolTipText;
    public string SpriteName;

    private bool m_isHovering = false;
    private bool m_isClicking = false;
    private bool m_IsSelecting = false;
    private GameObject m_ToolTip;
    private RectTransform m_ToolTipRect;
    private RectTransform m_ContentWindowRef;
    private Text m_ToolTipText;
    private Vector2 m_ToolTipPosOffset;
    private int m_MyIndex = -1;
    private int m_GridWidth;

    public void Init(Sprite sprite, string itemname, string itemtooltip, RectTransform contentWindow)
    {
        //Get the tooltips index
        m_MyIndex = contentWindow.childCount;

        m_ContentWindowRef = contentWindow;
        m_GridWidth = m_ContentWindowRef.GetComponent<GridLayoutGroup>().constraintCount;

        //set the sprite to the Item Image
        ItemSprite = gameObject.AddComponent<Image>();
        ItemSprite.enabled = true;
        ItemSprite.sprite = sprite;
        //Set more Members
        ItemName = itemname;
        ItemToolTipText = itemtooltip;
        transform.name = ItemName;

        m_ToolTip = GameObject.Instantiate(JournalUIManager.instance.ToolTipPrefab);
        //I am assuming that the fisrt child is the text (The prefab has to be made this way)
        m_ToolTipText = m_ToolTip.transform.GetChild(0).GetComponent<Text>();
        m_ToolTipText.text = ItemToolTipText;

        //The offset is just the m_ToolTip rect transform sizeDelta
        m_ToolTipPosOffset = m_ToolTip.GetComponent<RectTransform>().sizeDelta;
        m_ToolTipRect = m_ToolTip.GetComponent<RectTransform>();    

        HideText();

        m_ToolTipText.gameObject.transform.position = transform.position;
        m_ToolTip.transform.localPosition = Vector3.zero;

        //set the parent to the journal Sprite 
        m_ToolTip.transform.SetParent(transform);
    }

    private void Update()
    {
        if (m_IsSelecting)
        {
            if (Input.GetButtonUp("Submit"))
            {
                DisplayText();
            }
            else if (Input.GetButtonUp("Cancel"))
            {
                HideText();
            }
        }
    }


    private void SetToolTipPos()
    {
        // Get the distance from the side by index and set the local position accordingly
        int xIndex = m_MyIndex % m_GridWidth;
        int DistanceFromRight = m_GridWidth - xIndex;

        //Use the DistanceFromRight as scaling for a number you substract from the Tooltip's x pos
        float OffsetStep = m_ToolTipRect.sizeDelta.x / m_GridWidth;
        float Offset     = DistanceFromRight * OffsetStep;

        //Get a pos to the set  tooltip anchored pos to the rec pos
        Vector2 NewRecPos = m_ToolTipPosOffset;
        NewRecPos.x -= Offset;

        m_ToolTipRect.anchoredPosition = NewRecPos;
        m_ToolTipText.rectTransform.localPosition = Vector3.zero;

    }

    private void DisplayText()
    {
        //Display the text and set the position
        SetToolTipPos();
        //Set the local positions to 0 but offset the local pos of the tooltop 
        m_ToolTip.SetActive(true);        
    }

    private void HideText()
    {
        m_ToolTip.SetActive(false);
    }

    public bool CheckForMouseOver()
    {
        return m_isHovering;
    }

    public bool CheckForMouseClick()
    {
        return m_isClicking;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Pass the pointer screenspace pos to the display function
        DisplayText();
        m_isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Hide the text on pointer exit
        HideText();
        m_isHovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        m_isClicking = true;
    }

    public void SetIsHovering(bool flag)
    {
        m_isHovering = flag;
    }

    public void OnSelect(BaseEventData eventData)
    {
        m_IsSelecting = true;

        Color c = ItemSprite.color;
        c.a = 0.5f;
        ItemSprite.color = c;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        m_IsSelecting = false;
        HideText();

        Color c = ItemSprite.color;
        c.a = 1.0f;
        ItemSprite.color = c;
    }
}
