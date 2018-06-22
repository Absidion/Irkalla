using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

//Author: James 
//Last Updated: 1/6/2018

public class OptionsUI : MonoBehaviour {

    public Slider BrightnessSlider;                 //Reference to the Brightness Slider
    public Slider MasterVolumeSlider;               //Reference to the MasterVolume Slider
    public Slider MusicVolumeSlider;                //Reference to the MusicVolume Slider
    public Slider SFXVolumeSlider;                  //Reference to the Sound Effects Volume Slider
    public Slider MouseSensitivitySlider;           //Reference to the Slider that controls how sensitive the mouse is

    public Dropdown ResolutionValues;               //Reference to the Dropdown menu that contains a list of Resolutions
    public Dropdown QualityLevels;                  //Reference to the Dropdown menu that contains a list of Quality Levels
    public Toggle isFullscreen;                     //Reference to the Fullscreen toggle to determine if the window is fullscreen or not
    public Toggle isAxisInverted;                   //Reference to the Invert Axis toggle, to know if the camera/mouse will move in the opposite direction or not from how we moved.

    private Color m_BrightnessColor;                //Variable for the Brightness Colour that we change when we need to update the brightness instead of making this over and over again.

    public void Awake()
    {
        SetUpOptions();
        OpenedOptions();
        if(SceneManager.GetActiveScene().buildIndex == 0)
            UpdateSettings();
    }

    private void SetUpOptions()
    {
        List<String> ScreenSizes = new List<string>();
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            ScreenSizes.Add(Screen.resolutions[i].width.ToString() + " x " + Screen.resolutions[i].height.ToString());
        }
        
        ResolutionValues.AddOptions(ScreenSizes);

        for (int i = 0; i < ResolutionValues.options.Count; i++)
        {
            int indexCheck = i + 1;
            if (indexCheck < ResolutionValues.options.Count)
            {
                if (ResolutionValues.options[i].text == ResolutionValues.options[indexCheck].text)
                {
                    ResolutionValues.options.RemoveAt(indexCheck);
                }
            }
        }

        List<String> QualityLevelsList = new List<string>();
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            QualityLevelsList.Add(QualitySettings.names[i]);
        }
        QualityLevels.AddOptions(QualityLevelsList);

        MouseSensitivitySlider.value = CharacterController.Sensitivety;
        isAxisInverted.isOn = CharacterController.Inverted;

    }

    public void OpenedOptions()
    {
        BrightnessSlider.value = Options.ReturnBrightness();

        MasterVolumeSlider.value = Options.m_MasterVolume;
        MusicVolumeSlider.value = Options.m_MusicVolume;
        SFXVolumeSlider.value = Options.m_SFXVolume;

        isFullscreen.isOn = Screen.fullScreen;

        string currentRes = Screen.width.ToString() + " x " + Screen.height.ToString();

        for (int i = 0; i < ResolutionValues.options.Count; i++)
        {
            if (ResolutionValues.options[i].text == currentRes)
            {
                ResolutionValues.value = i;
            }
        }

        QualityLevels.value = QualitySettings.GetQualityLevel();

        MouseSensitivitySlider.value = CharacterController.Sensitivety;
        isAxisInverted.isOn = CharacterController.Inverted;
    }

    private void UpdateSettings()
    {
        m_BrightnessColor.r = BrightnessSlider.value;
        m_BrightnessColor.g = BrightnessSlider.value;
        m_BrightnessColor.b = BrightnessSlider.value;
        float bright = BrightnessSlider.value;

        Options.UpdateBrightness(m_BrightnessColor, bright);

        Options.UpdateAudioSettings(MasterVolumeSlider.value, SFXVolumeSlider.value, MusicVolumeSlider.value);

        Options.UpdateGraphicsQuality(QualityLevels.value);

        string Result = ResolutionValues.options[ResolutionValues.value].text;

        string[] Values = Result.Split('x', '@', 'H');
        int width = Int32.Parse(Values[0]);
        int height = Int32.Parse(Values[1]);
        Options.UpdateResolution(width, height, isFullscreen.isOn);

        Options.UpdateSensitivty(MouseSensitivitySlider.value, isAxisInverted.isOn);
    }

    private void Update()
    {
        if (gameObject.GetComponent<Canvas>().enabled == true)
        {
            UpdateSettings();
        }
    }
}
