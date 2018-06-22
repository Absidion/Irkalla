using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InGameMenuNavigator : MonoBehaviour
{
    public Canvas PauseMenuCanvas;
    public Canvas JournalCanvas;
    public Canvas OptionsCanvas;
    public Canvas HelpCanvas;

    static public bool IsGamePaused = false;

    private List<Canvas> m_CanvasStack;                 //the list of canvases currently active
    private EventSystem m_EventSystem;
    private GameObject m_PauseMenuFirstSelected;        //the first button that gets selected when pausing the game

    void Awake()
    {
        m_EventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        m_CanvasStack = new List<Canvas>();

        if (PauseMenuCanvas == null)
        {
            PauseMenuCanvas = GameObject.Find("PauseMenu").GetComponent<Canvas>();
            if (PauseMenuCanvas == null)
            {
                Debug.LogError("Pause Menu Canvas has not been set in InGameMenuNavigation script. Please attach the proper object to the script.");
            }
        }

        if (JournalCanvas == null)
        {
            JournalCanvas = GameObject.Find("JournalCanvas").GetComponent<Canvas>();
            if (JournalCanvas == null)
            {
                Debug.LogError("Journal Canvas has not been set in InGameMenuNavigation script. Please attach the proper object to the script.");
            }
        }
        if (OptionsCanvas == null)
        {
            OptionsCanvas = GameObject.Find("Options").GetComponent<Canvas>();
            if (OptionsCanvas == null)
            {
                Debug.LogError("Options Canvas has not been set in InGameMenuNavigation script. Please attach the proper object to the script.");
            }
        }
        if (HelpCanvas == null)
        {
            HelpCanvas = GameObject.Find("HelpCanvas").GetComponent<Canvas>();
            if (HelpCanvas == null)
            {
                Debug.LogError("Help Canvas has not been set in InGameMenuNavigation script. Please attach the proper object to the script.");
            }
        }

        PauseMenuCanvas.gameObject.SetActive(true);
        GameObject.Find("SeedString").GetComponent<Text>().text = GameManager.Instance.PreTranslatedSeed;
        PauseMenuCanvas.enabled = false;

        JournalCanvas.gameObject.SetActive(true);
        JournalCanvas.enabled = false;

        OptionsCanvas.gameObject.SetActive(true);
        OptionsCanvas.enabled = false;

        HelpCanvas.gameObject.SetActive(true);
        HelpCanvas.enabled = false;

        m_PauseMenuFirstSelected = PauseMenuCanvas.transform.Find("ResumeButton").gameObject;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        //check for pause menu / journal menu input
        if (CharacterController.GetPauseDown())
        {
            SetPauseMenuActive();
        }
        if (CharacterController.GetJournalDown())
        {
            SetJournalActive();
        }

    }

    //sets the journal to be in the opposite state that it's currently in
    public void SetJournalActive()
    {
        if (PlayerHUDManager.instance.LocalPlayer != null)
        {
            //set the canvas to its opposite state
            JournalCanvas.enabled = !JournalCanvas.enabled;
            this.gameObject.GetComponent<PlayerHUDManager>().StopPlayerVelocity();

            ClearList();

            if (JournalCanvas.enabled)
            {
                PlayerHUDManager.instance.LocalPlayer.PlayerMovement.DisableMovementFX();

                //push the canvas onto the list
                m_CanvasStack.Add(JournalCanvas);
                //set the static paused value to true
                IsGamePaused = true;
                Cursor.lockState = CursorLockMode.None;
                GameObject.Find("PlayerUIPrefab").GetComponent<PlayerHUDManager>().ToggleCurrency(true);

                //set the first selected button
                if (JournalUIManager.instance.firstItemInJournal != null)
                {
                    m_EventSystem.SetSelectedGameObject(JournalUIManager.instance.firstItemInJournal);
                }
            }
            else
            {
                //unpause the game
                IsGamePaused = false;
                Cursor.lockState = CursorLockMode.Locked;
                GameObject.Find("PlayerUIPrefab").GetComponent<PlayerHUDManager>().ToggleCurrency(false);
            }
        }
    }

    public void SetOptionsActive()
    {
        //set the canvas to its opposite state
        OptionsCanvas.enabled = !OptionsCanvas.enabled;

        ClearList();

        if (OptionsCanvas.enabled)
        {
            OptionsCanvas.GetComponent<OptionsUI>().enabled = true;
            //push the canvas onto the list
            m_CanvasStack.Add(OptionsCanvas);
            //set the static paused value to true
            IsGamePaused = true;
            Cursor.lockState = CursorLockMode.None;

            //set the first selected button
            //if (JournalUIManager.instance.firstItemInJournal != null)
            //{
            //    m_EventSystem.SetSelectedGameObject(JournalUIManager.instance.firstItemInJournal);
            //}
        }
        else
        {
            //unpause the game
            IsGamePaused = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    //sets the pause menu to be in the opposite state it's currently in
    public void SetPauseMenuActive()
    {
        if (PlayerHUDManager.instance.LocalPlayer != null)
        {
            PauseMenuCanvas.enabled = !PauseMenuCanvas.enabled;

            ClearList();

            if (PauseMenuCanvas.enabled)
            {
                PlayerHUDManager.instance.LocalPlayer.PlayerMovement.DisableMovementFX();

                m_CanvasStack.Clear();
                //push the canvas onto the list
                m_CanvasStack.Add(PauseMenuCanvas);
                //pause the game
                IsGamePaused = true;
                Cursor.lockState = CursorLockMode.None;

                //set the first selected button
                m_EventSystem.SetSelectedGameObject(m_PauseMenuFirstSelected);
                this.gameObject.GetComponent<PlayerHUDManager>().StopPlayerVelocity();
            }
            else
            {
                //unpause the game
                IsGamePaused = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    //loads the new canvas and sets it to be the active canvas
    public void ActivateNewCanvas(Canvas canvasToAdd)
    {
        //if the game isn't paused then we need to pause it
        if (!IsGamePaused)
        {
            IsGamePaused = true;
        }

        //set the canvas at the back of the list to be disabled
        m_CanvasStack[m_CanvasStack.Count - 1].enabled = false;
        //activate the new canvas
        canvasToAdd.enabled = true;
        //push it to the back of the list
        m_CanvasStack.Add(canvasToAdd);
    }

    //remove canvas from stack
    public void PopCanvasOffList()
    {
        //disable the canvas at the back of the list
        m_CanvasStack[m_CanvasStack.Count - 1].enabled = false;
        //remove the canvas from the stack
        m_CanvasStack.RemoveAt(m_CanvasStack.Count - 1);

        if (m_CanvasStack.Count == 0)
        {
            IsGamePaused = false;
        }
    }

    //allows the use to quit to the main menu
    public void QuitToMainMenu()
    {
        IsGamePaused = false;

        GameObject launch = GameObject.Find("Managers");
        launch.GetPhotonView().RPC("SyncScenes", PhotonTargets.All, false);

        GameManager.Instance.QuitToMain();
    }

    private void ClearList()
    {
        //if the journal canvas is no longer active then we will quickly deactivate all items in the list and clear the list
        for (int i = 0; i < m_CanvasStack.Count; i++)
        {
            m_CanvasStack[i].enabled = false;
        }
        m_CanvasStack.Clear();
    }
}
