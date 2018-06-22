using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//Author: James
//Created: 12/7/2017
//Last Updated: 12/18/2017

[Serializable]
public class Options
{
    public static Options Instance;                    //Singleton for Options so when we need to update a setting, we can get this and update it.
    public static float m_MasterVolume = 0.5f;         //Master Volume variable
    public static float m_SFXVolume = 0.5f;            //Sound Effects Volume variable
    public static float m_MusicVolume = 0.5f;          //Music Volume Variable

    public static float m_Brightness = 0.5f;           //Brightness variable

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    //When we want to update our brightness, this function gets called with the colour and brightness value.
    //We store the brightness float, because this is what the Slider uses to represent the brightness.
    //The colour value has RGB all as the same value as brightness.
    public static void UpdateBrightness(Color lightValue, float brightness)
    {
        m_Brightness = brightness;
        RenderSettings.ambientLight = lightValue;
    }

    //When we need to display Brightness as an option, we get the value
    public static float ReturnBrightness()
    {
        return m_Brightness;
    }

    //This function is called when we want to update our AudioSettings. 
    //If there is a SoundManager, then we can update it already
    //If there is no SoundManager, then we save the values, so when there is one, the values can be set right away
    public static void UpdateAudioSettings(float MSV, float SFXV, float MUV)
    {
        if (SoundManager.GetInstance() != null)
        {
            m_MasterVolume = MSV;
            m_SFXVolume = SFXV;
            m_MusicVolume = MUV;
            SoundManager.SetBGMVolume(MUV);
            SoundManager.SetSFXVolume(SFXV);
            SoundManager.SetMasterVolume(MSV);
        }
        else
        {
            m_MasterVolume = MSV;
            m_SFXVolume = SFXV;
            m_MusicVolume = MUV;
        }

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            Camera.main.GetComponent<AudioSource>().volume = MUV * MSV;
        }
    }

    //We update our screen resolution when we change it.
    public static void UpdateResolution(int width, int height, bool toggle)
    {
        Screen.SetResolution(width, height, toggle);
    }

    public static void UpdateGraphicsQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }

    public static void UpdateSensitivty(float speed, bool flag)
    {
        CharacterController.Sensitivety = speed;
        CharacterController.Inverted = flag;
    }
}
