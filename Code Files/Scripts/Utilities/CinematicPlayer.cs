using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class CinematicPlayer : MonoBehaviour {

    public VideoClip BossCinematic;
    public bool isPlaying = false;
    public bool hasPlayed = false;

    public Player m_LocalPlayer = null;

    public void PlayCutscene()
    {
        m_LocalPlayer.Health.IsInvincible = true;
        PlayerHUDManager.instance.gameObject.SetActive(false);
        // Will attach a VideoPlayer to the main camera.
        GameObject camera = GameObject.Find("Main Camera");

        // VideoPlayer automatically targets the camera backplane when it is added
        // to a camera object, no need to change videoPlayer.targetCamera.
        var videoPlayer = camera.GetComponent<VideoPlayer>();

        // Play on awake defaults to true. Set it to false to avoid the url set
        // below to auto-start playback since we're in Start().
        videoPlayer.playOnAwake = false;

        // By default, VideoPlayers added to a camera will use the far plane.
        // Let's target the near plane instead.
        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;

        // This will cause our scene to be visible through the video being played.
        videoPlayer.targetCameraAlpha = 1;

        // Set the video to play. URL supports local absolute or relative paths.
        // Here, using absolute.
        videoPlayer.clip = BossCinematic;

        // Skip the first 100 frames.
        videoPlayer.frame = -20;

        // Restart from beginning when done.
        videoPlayer.isLooping = false;

        // Each time we reach the end, we slow down the playback by a factor of 10.
        videoPlayer.loopPointReached += EndReached;

        // Start playback. This means the VideoPlayer may have to prepare (reserve
        // resources, pre-load a few frames, etc.). To better control the delays
        // associated with this preparation one can use videoPlayer.Prepare() along with
        // its prepareCompleted event.
        videoPlayer.Play();

        isPlaying = true;
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        vp.targetCameraAlpha = 0;
        vp.Stop();
        isPlaying = false;
        hasPlayed = true;
        m_LocalPlayer.Health.IsInvincible = false;
        PlayerHUDManager.instance.gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        Player p = other.GetComponent<Player>();

        if (p != null)
        {
            if (p.photonView.isMine && !isPlaying && !hasPlayed)
            {
                m_LocalPlayer = p;
                PlayCutscene();
            }
        }
    }

}
