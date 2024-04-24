using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerManager : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer videoPlayer;
    [SerializeField]
    private GameObject ui;

    public static VideoPlayerManager instance;

    private void Awake()
    {
        //Static instance for easier access
        instance = this;
        
        ShowUI(false);
    }

    public void ShowUI(bool show)
    {
        ui.SetActive(show);

        if (!show)
        {          
            videoPlayer.Stop();
        }
        else
        {        
            videoPlayer.Play();
        }
    }

    public void SetVideoClip(VideoClip clip)
    {
        videoPlayer.clip = clip;
    }

    public void Play()
    {
        videoPlayer.Play();
    }

    public void Pause()
    {
        videoPlayer.Pause();
    }

    public void Replay()
    {
        //Set the played time to 0 to bring the clip to start
        videoPlayer.time = 0;
        Play();
    }
}
