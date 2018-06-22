using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    public float IconSpinningSpeed = 10.0f;
    public float IconFlashingSpeed = 1.0f;

    public RectTransform SpinningIcon; // The icon to Spin
    public Image         FlashingIcon; // The icon to flash
    private Canvas       m_Canvas;

    private void Awake()
    {
        GameManager.FinalStep += DoneLoading;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //Set this to the deletgate where this function should be called
    public void DoneLoading(object sender, EventArgs loadFinished)
    {
        SoundManager.PlayAmbient();
        gameObject.SetActive(false);
    }
    
    void Update () {
        //In update we are gonna manage the spinning of the Symbol and the flashing of the alpha of the background image
        SpinIcon (SpinningIcon);
        FlashIcon(FlashingIcon);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        m_Canvas = GetComponent<Canvas>();
        m_Canvas.gameObject.SetActive(true);
    }

    private void SpinIcon(RectTransform icon)
    {
        //Rotate the icon with simple movement
        Vector3 NewRotation = icon.rotation.eulerAngles;
        NewRotation.z += (IconSpinningSpeed * Time.deltaTime);
        icon.transform.eulerAngles = NewRotation;
    }

    private void FlashIcon(Image icon)
    {
        //Change the Alpha channel value according to a sin wave
        //Get a sin number from 0-1 based on the time since the start of the program 
        //Offset the wave by 0.2
        float FrameAlpha = 0.5f * Mathf.Sin(Time.time / IconFlashingSpeed) + 0.7f;
        //Get a color equal to the current color
        Color AlphaColor = icon.color;
        //Set the alpha to the frame alpha
        AlphaColor.a = FrameAlpha;

        //Set the new color
        icon.color = AlphaColor;
    }
}
