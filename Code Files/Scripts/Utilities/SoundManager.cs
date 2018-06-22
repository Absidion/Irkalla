using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Written by: James
//Created On: 11/28/2017
//Last Updated: James 12/18/2017

public class SoundManager : Photon.MonoBehaviour
{

    private const float m_MaxVolumeBGM = 1.0f;                          //Maximum volume for our background music
    private const float m_MaxVolumeSFX = 1.0f;                          //Maximum volume for our sound effects
    private static float m_CurrentVolumeNormalizedBGM = 1f;             //Background Music Current Volume normalized to multiple by the maximum volume to determine actual volume.
    private static float m_CurrentVolumeNormalizedSFX = 1f;             //Sound Effects Current Volume normalized to multiple by the maximum volume to determine actual volume.

    public int initialPoolSize = 15;                                    //How many AudioSources we are fine with having
    public int maxPoolSize = 20;                                        //How many AudioSources we are fine with having at most
    public string m_SfxPool = "sfxSources";
    private NetworkObjectPool m_SfxSources;                             //Reference to the ObjectPool for Sound Effects
    private AudioSource m_BGMSource;                                    //Background Music Source
    private static float m_MasterVolume = 1.0f;                         //Master Volume
    private bool m_IsPlayingBGM = false;

    //Static instance
    private static SoundManager m_Instance;
    private static GameObject SMObject;

    protected void Awake()
    {
        GameManager.Instance.DontDestroyNormalObject(gameObject);
    }
    public static SoundManager GetInstance()
    {
        if (m_Instance == null)
        {
            SMObject = GameObject.Find("Managers That Persist In Game");
            if (SMObject != null)
            {
                m_Instance = SMObject.GetComponent<SoundManager>();
                if (m_Instance == null)
                {
                    m_Instance = SMObject.AddComponent<SoundManager>();
                }
                m_Instance.SetUpPool();
            }
        }

        if (m_Instance != null)
        {
            return m_Instance;
        }
        return null;
    }

    private void SetUpPool()
    {
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.CreateNetworkPoolWithName(m_SfxPool, "Effects/SfxSource", initialPoolSize, maxPoolSize, true);
            m_SfxSources = ObjectPoolManager.Instance.GetNetworkPoolFromManager(m_SfxPool);

            //add our bgm sound source
            m_BGMSource = gameObject.AddComponent<AudioSource>();
            m_BGMSource.loop = true;
            m_BGMSource.playOnAwake = false;
            m_BGMSource.volume = GetBGMVolume();
            GameManager.Instance.DontDestroyNetworkObject(gameObject);
        }
    }

    //Volume Getters 
    //1. Background Music
    //2. Sound Effects 

    //We either return zero if sound is muted, or we return the maximum volume multiplied by the current normalized volume.
    public static float GetBGMVolume()
    {
        return (m_MaxVolumeBGM * m_CurrentVolumeNormalizedBGM) * m_MasterVolume;
    }

    public static float GetSFXVolume()
    {
        return (m_MaxVolumeSFX * m_CurrentVolumeNormalizedSFX) * m_MasterVolume;
    }

    //Background Music Utilities
    //When Background musc is disabled, this will fade it out
    private void FadeBGMOut(float delay, float fadeDuration)
    {
        if (m_IsPlayingBGM)
        {
            SoundManager soundMan = GetInstance();
            float toVolume = 0f;
            float fromVolume = GetBGMVolume();

            if (soundMan.m_BGMSource.clip == null)
            {
                Debug.LogError("Error: Could Not fade BGM out as BGM AudioSource has no currently playing clip.");
            }

            StartCoroutine(FadeBGM(fromVolume, toVolume, delay, fadeDuration));
        }

        if (m_BGMSource.volume == 0)
            m_IsPlayingBGM = false;
    }
    //When Background musc is enabled, this will fade it in
    private void FadeBGMIn(AudioClip bgmClip, float delay, float fadeDuration)
    {
        if (!m_IsPlayingBGM)
        {
            SoundManager soundMan = GetInstance();
            soundMan.m_BGMSource.clip = bgmClip;
            soundMan.m_BGMSource.Play();
            soundMan.m_BGMSource.volume = 0;

            float toVolume = GetBGMVolume();
            float fromVolume = 0f;

            StartCoroutine(FadeBGM(fromVolume, toVolume, delay, fadeDuration));
        }
    }
    //Couroutine function to fade background music
    IEnumerator FadeBGM(float fadefromVolume, float fadeToVolume, float delay, float duration)
    {
        SoundManager soundMan = GetInstance();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = (elapsed / duration);
            float volume = Mathf.Lerp(fadefromVolume, fadeToVolume, t);
            soundMan.m_BGMSource.volume = volume;

            elapsed += Time.deltaTime;
            yield return 0;
        }

    }

    public static void PlayAmbient()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1 && GameManager.Instance.PreTranslatedSeed != "red bing")
            PlayBGM(Resources.Load("Sounds/Brad Song 1 - Ambient") as AudioClip, true, 5);
    }

    public static void PlayCombat()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1 && GameManager.Instance.PreTranslatedSeed != "red bing")
            PlayBGM(Resources.Load("Sounds/Brad Song 2 - Combat") as AudioClip, true, 5);
    }

    public static void PlayRed()
    {
        PlayBGM(Resources.Load("Sounds/redbing") as AudioClip, true, 5);
    }

    //BGM Functions
    public static void PlayBGM(AudioClip bgmClip, bool fade, float fadeDuration)
    {
        SoundManager soundMan = GetInstance();

        if (fade)
        {
            if (soundMan.m_BGMSource.isPlaying)
            {
                //fade out, then switch and fade in
                soundMan.FadeBGMOut(fadeDuration / 2, fadeDuration / 2);
                soundMan.FadeBGMIn(bgmClip, fadeDuration / 2, fadeDuration / 2);
            }
            else
            {
                //just fade in
                soundMan.FadeBGMIn(bgmClip, fadeDuration, fadeDuration);
            }
        }
        else
        {
            //play immediately
            soundMan.m_BGMSource.loop = true;
            soundMan.m_BGMSource.volume = GetBGMVolume();
            soundMan.m_BGMSource.clip = bgmClip;
            soundMan.m_BGMSource.Play();
        }

    }

    //Stops playing Background music
    public static void StopBGM(bool fade, float fadeDuration)
    {
        SoundManager soundMan = GetInstance();
        if (soundMan.m_BGMSource.isPlaying)
        {
            //fade out, then switch and fade in
            if (fade)
            {
                soundMan.FadeBGMOut(fadeDuration, fadeDuration);
            }
            else
            {
                soundMan.m_BGMSource.Stop();
            }
        }
    }

    //SFX Utilities
    GameObject GetSFXSource()
    {
        SoundManager soundMan = GetInstance();
        GameObject Go = ObjectPoolManager.Instance.GetObjectFromNetworkPool(m_SfxPool);
        if (Go == null)
            return null;

        Go.SetActive(true);
        AudioSource sfx = Go.GetComponent<AudioSource>();

        sfx.loop = false;
        sfx.playOnAwake = false;
        sfx.volume = GetSFXVolume();

        return Go;
    }
    //As the sound effect is playing, the couroutine will let the effect play out before deactivating the audiosource
    IEnumerator RemoveSFXSource(GameObject sfxSource)
    {
        AudioSource sfx = sfxSource.GetComponent<AudioSource>();
        yield return new WaitForSeconds(sfx.clip.length);
        sfxSource.SetActive(false);
    }

    //As the sound effect is playing, the couroutine will let the effect play out on a loop until the length that has been entered has been reached. 
    //Then we deactivate the audiosource
    IEnumerator RemoveSFXSourceFixedLength(GameObject sfxSource, float length)
    {
        yield return new WaitForSeconds(length);
        sfxSource.SetActive(false);
    }

    //SFX Functions
    //Plays a single sound effect
    public static void PlaySFX(string sfxClip, Vector3 position)
    {
        SoundManager soundMan = GetInstance();
        GameObject source = soundMan.GetSFXSource();
        if (source == null)
            return;

        source.transform.position = position;
        AudioSource sfx = source.GetComponent<AudioSource>();
        sfx.volume = GetSFXVolume();
        sfx.clip = Resources.Load("Sounds/" + sfxClip) as AudioClip;
        sfx.Play();

        soundMan.StartCoroutine(soundMan.RemoveSFXSource(source));
    }

    //Plays a Random Sound Effect from an array of sound effects
    public static void PlayRandomSFX(Vector3 position, params string[] clipNames)
    {
        int randomIndex = Random.Range(0, clipNames.Length);
        SoundManager soundMan = GetInstance();
        GameObject source = soundMan.GetSFXSource();
        if (source == null)
            return;

        source.transform.position = position;
        AudioSource sfx = source.GetComponent<AudioSource>();
        sfx.volume = GetSFXVolume();
        sfx.clip = Resources.Load("Sounds/" + clipNames[randomIndex]) as AudioClip;
        sfx.Play();

        soundMan.StartCoroutine(soundMan.RemoveSFXSource(source));
    }

    //Plays a single sound effect with a randomized pitch to add variance. 
    public static void PlaySFXRandomizedPitch(string sfxClip, Vector3 position)
    {
        SoundManager soundMan = GetInstance();
        GameObject source = soundMan.GetSFXSource();
        if (source == null)
            return;

        source.transform.position = position;
        AudioSource sfx = source.GetComponent<AudioSource>();
        sfx.volume = GetSFXVolume();
        sfx.clip = Resources.Load("Sounds/" + sfxClip) as AudioClip;
        sfx.pitch = UnityEngine.Random.Range(0.85f, 1.2f);
        sfx.Play();

        soundMan.StartCoroutine(soundMan.RemoveSFXSource(source));
    }

    //Plays a single sound effect on a loop.
    //Returns an AudioSource back to the person calling it. 
    public static AudioSource PlaySFXLooped(string sfxClip)
    {
        SoundManager soundMan = GetInstance();
        GameObject source = soundMan.GetSFXSource();
        if (source == null)
            return null;

        AudioSource sfx = source.GetComponent<AudioSource>();
        sfx.volume = GetSFXVolume();
        sfx.clip = Resources.Load("Sounds/" + sfxClip) as AudioClip;
        sfx.loop = true;
        sfx.Play();

        return sfx;
    }

    //The Audiosource that was previously returned, would get passed into here to be stopped and disabled. 
    public static void StopSFXLooped(AudioSource sfx)
    {
        if (sfx == null) return;

        sfx.loop = false;
        sfx.Stop();
        sfx.volume = 0;
        sfx.gameObject.SetActive(false);
    }

    [PunRPC]
    public void PlaySFXNetworked(string sfxClip, Vector3 position)
    {
        SoundManager soundMan = GetInstance();
        GameObject source = soundMan.GetSFXSource();
        if (source == null)
            return;

        source.transform.position = position;
        AudioSource sfx = source.GetComponent<AudioSource>();
        sfx.volume = GetSFXVolume();
        sfx.clip = Resources.Load("Sounds/" + sfxClip) as AudioClip;
        sfx.Play();

        soundMan.StartCoroutine(soundMan.RemoveSFXSource(source));
    }
    [PunRPC] //Plays a Random Sound Effect from an array of sound effects
    public void PlayRandomSFXNetworked(string effect1, string effect2, string effect3, Vector3 position)
    {
        string[] clipNames = { effect1, effect2, effect3 };

        int randomIndex = UnityEngine.Random.Range(0, clipNames.Length);
        SoundManager soundMan = GetInstance();
        GameObject source = soundMan.GetSFXSource();
        if (source == null)
            return;

        source.transform.position = position;
        AudioSource sfx = source.GetComponent<AudioSource>();
        sfx.volume = GetSFXVolume();
        sfx.clip = Resources.Load("Sounds/" + clipNames[randomIndex]) as AudioClip;
        sfx.Play();

        soundMan.StartCoroutine(soundMan.RemoveSFXSource(source));
    }

    [PunRPC]     //Plays a single sound effect with a randomized pitch to add variance.  
    public void PlaySFXRandomizedPitchNetworked(string sfxClip, Vector3 position)
    {
        SoundManager soundMan = GetInstance();
        GameObject source = soundMan.GetSFXSource();
        if (source == null)
            return;

        source.transform.position = position;
        AudioSource sfx = source.GetComponent<AudioSource>();
        sfx.volume = GetSFXVolume();
        sfx.clip = Resources.Load("Sounds/" + sfxClip) as AudioClip;
        sfx.pitch = UnityEngine.Random.Range(0.85f, 1.2f);
        sfx.Play();

        soundMan.StartCoroutine(soundMan.RemoveSFXSource(source));
    }
    [PunRPC]     //Play a single sound effect with a duration. The effect will loop, and then be removed once the duration is reached.
    public void PlaySFXFixedDurationNetworked(string sfxClip, float duration, Vector3 position, float volumeMultiplier = 1.0f)
    {
        SoundManager soundMan = GetInstance();
        GameObject source = soundMan.GetSFXSource();
        if (source == null)
            return;

        source.transform.position = position;
        AudioSource sfx = source.GetComponent<AudioSource>();
        sfx.volume = GetSFXVolume() * volumeMultiplier;
        sfx.clip = Resources.Load("Sounds/" + sfxClip) as AudioClip;
        sfx.loop = true;
        sfx.Play();

        soundMan.StartCoroutine(soundMan.RemoveSFXSourceFixedLength(source, duration));
    }

    //Volume Control Functions
    //If we have all of our audio capable of being muted based on a toggle, this can be used to immediately disable them all.
    public static void DisableSoundImmediate()
    {
        SoundManager soundMan = GetInstance();
        soundMan.StopAllCoroutines();
        if (soundMan.m_SfxSources != null)
        {
            foreach (GameObject source in soundMan.m_SfxSources.Pool)
            {
                source.GetComponentInChildren<AudioSource>().volume = 0;
            }
        }
        soundMan.m_BGMSource.volume = 0f;
    }

    //If we have all of our audio muted based on a toggle, this can be used to immediately enable them again.
    public static void EnableSoundImmediate()
    {
        SoundManager soundMan = GetInstance();
        soundMan.StopAllCoroutines();
        if (soundMan.m_SfxSources.Pool != null)
        {
            foreach (GameObject source in soundMan.m_SfxSources.Pool)
            {
                source.GetComponentInChildren<AudioSource>().volume = GetSFXVolume();
            }
        }
        soundMan.m_BGMSource.volume = GetBGMVolume();
    }

    //Master Volume Control
    //Needs to be adjusted to be MasterVolume
    public static void SetMasterVolume(float newVolume)
    {
        GetInstance().MasterVolume = newVolume;
        AdjustSoundImmediate();
    }

    //Specifically Sound Effects Volume
    public static void SetSFXVolume(float newVolume)
    {
        m_CurrentVolumeNormalizedSFX = newVolume;
        AdjustSoundImmediate();
    }

    //Specifically Background Music
    public static void SetBGMVolume(float newVolume)
    {
        m_CurrentVolumeNormalizedBGM = newVolume;
        AdjustSoundImmediate();
    }

    //When volume has been adjusted, this will update audiosrouces. 
    public static void AdjustSoundImmediate()
    {
        SoundManager soundMan = GetInstance();
        if (soundMan != null)
        {
            if (soundMan.m_SfxSources != null)
            {
                foreach (GameObject source in soundMan.m_SfxSources.Pool)
                {
                    source.GetComponentInChildren<AudioSource>().volume = GetSFXVolume();
                }
            }
            //Debug.Log("BGM Volume: " + GetBGMVolume());
            soundMan.m_BGMSource.volume = GetBGMVolume();
            //Debug.Log("BGM volume is now: " + GetBGMVolume());
        }
    }

    public float MasterVolume
    {
        get
        {
            return m_MasterVolume;
        }

        set
        {
            m_MasterVolume = value;
        }
    }
}
