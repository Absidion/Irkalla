using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Author: Daniel French
//Last edited: Josue 12/07/2017

public class JournalUIManager : MonoBehaviour
{
    public static JournalUIManager instance = null;
    public Vector2 ItemSpriteSize = new Vector2(90,90);

    public string BackgroundSpritePath = "Items/Textures/JournalSpriteBackground";

    public Image DamageBar;
    public Image DefenceBar;    
    public Image MovementSpeedBar;
    public Image CooldownReductionBar;
    public Image LuckBar;
    public Image LifeStealBar;
    public GridLayoutGroup GridLayout;
    public RectTransform ContentWindow;

    public GameObject ToolTipPrefab;
    public GameObject firstItemInJournal = null;

    private List<Selectable> m_JournalElements;     //each journal element that gets created gets put in here

    private void ResizeContentWindow()
    {
        //This code assumes the GridLayout is constrianed by number of Cols
        int NumberOfRows = ContentWindow.childCount / GridLayout.constraintCount;
        //Account for index 1 being 0
        NumberOfRows += 1;
        // The height is the Number of Rows multiplied by the scacing on the y axis
        //Width is not counted because Gridlayout is constrained to a fixed number of colums
        int height = NumberOfRows * (int)(GridLayout.cellSize.y + GridLayout.spacing.y);
        //Make a vec2 to assign the new sizeDelta, add a megic number to account for the initial padding from the grid
        Vector2 newSize = new Vector2(ContentWindow.sizeDelta.x , height + 150);
        //RectTransforms have to be resized via a Vector2 Size Delta

        ContentWindow.sizeDelta = newSize;

    }

    private void Awake()
    {
        instance = this;
        m_JournalElements = new List<Selectable>();
    }

    public void AddElementToViewport(string ItemNameString, string ToolTipString, string SpriteResourceString)
    {
        ResizeContentWindow();
        GameObject newObj = new GameObject(ItemNameString);
        RectTransform rect = newObj.AddComponent<RectTransform>();
        //Resize the rect so it fits better in the background
        rect.sizeDelta = ItemSpriteSize;

        //Load a sprite from the string
        Sprite ImageSprite = Resources.Load<Sprite>("Items/Sprites/" + SpriteResourceString);
        JournalElement JournalElement = rect.gameObject.AddComponent<JournalElement>();

        //Add selectable component and add to list
        rect.gameObject.AddComponent<Selectable>();
        m_JournalElements.Add(rect.gameObject.GetComponent<Selectable>());

        //set up selectable navigation
        for (int i = 0; i < m_JournalElements.Count; i++)
        {
            Navigation elementNav = m_JournalElements[i].navigation;

            if (elementNav.mode != Navigation.Mode.Explicit)
            {
                elementNav.mode = Navigation.Mode.Explicit;
            }

            //if first item in journal
            if (i == 0 && m_JournalElements.Count > 1)
            {
                elementNav.selectOnUp = m_JournalElements[m_JournalElements.Count - 1];
                elementNav.selectOnLeft = m_JournalElements[m_JournalElements.Count - 1];
                elementNav.selectOnDown = m_JournalElements[1];
                elementNav.selectOnRight = m_JournalElements[1];
            }
            else if (i != 0 && i != m_JournalElements.Count - 1) //if journal element in between the first and last
            {
                elementNav.selectOnUp = m_JournalElements[i - 1];
                elementNav.selectOnLeft = m_JournalElements[i - 1];
                elementNav.selectOnDown = m_JournalElements[i + 1];
                elementNav.selectOnRight = m_JournalElements[i + 1];
            }
            else if (i != 0 && i == m_JournalElements.Count - 1) //if last element in journal
            {
                elementNav.selectOnUp = m_JournalElements[i - 1];
                elementNav.selectOnLeft = m_JournalElements[i - 1];
                elementNav.selectOnDown = m_JournalElements[0];
                elementNav.selectOnRight = m_JournalElements[0];
            }

            m_JournalElements[i].navigation = elementNav;
        }

        //if the first item in the list has has not been set yet, set it
        if (firstItemInJournal == null)
        {
            firstItemInJournal = rect.gameObject;
        }

        //Add a background to the item that is being added
        GameObject BackgroundObj = new GameObject("ItemBackground");
        RectTransform BackgroundRect = BackgroundObj.AddComponent<RectTransform>();
        Sprite BackgroundSprite = Resources.Load<Sprite>(BackgroundSpritePath);
        Image BackgroundImage = BackgroundObj.AddComponent<Image>();
        BackgroundImage.sprite = BackgroundSprite;
        //Have the background be the parent object to ensure the sprite draws on top
        BackgroundObj.transform.SetParent(ContentWindow);
        
        //Initialize a new Journal element and add it to the content's children
        JournalElement.Init(ImageSprite, SpriteResourceString, ToolTipString, ContentWindow);
        newObj.transform.SetParent(BackgroundObj.transform);
    }

    public void ChangeStatValue(StatType statToChange, float amount, float max)
    {
        //use a StatType switch to get the bar
        //Resize it using the amount / max 
        switch (statToChange)
        {
            case StatType.DAMAGE:
                ResizeBar(DamageBar,amount, max);
                break;
            case StatType.LUCK:
                ResizeBar(LuckBar, amount, max);
                break;
            case StatType.CDR:
                ResizeBar(CooldownReductionBar, amount, max);
                break;
            case StatType.LIFESTEAL:
                ResizeBar(LifeStealBar, amount, max);
                break;
            case StatType.DEFENSE:
                ResizeBar(DefenceBar, amount, max);
                break;
            case StatType.MOVEMENT_SPEED:
                ResizeBar(MovementSpeedBar, amount, max);
                break;
        }
    }

    private void ResizeBar(Image bar, float stat, float statmax)
    {
        //Divide the states and set the bar's fill to the resulting decimal
        float fillAmount = stat / statmax;
        bar.fillAmount = fillAmount;
    }

    public void ChangeActiveItem(string ItemNameString, string ToolTipString, string SpriteResourceString)
    {
        //Find the the child of the name
        Transform child = ContentWindow.transform.Find(ItemNameString);
        child.SetAsFirstSibling();
    }
}
