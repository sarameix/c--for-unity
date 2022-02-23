// This script sets up play, pause, and scroll functions for a video player in a Unity scene for Android.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class VideoPlayerFunctions : MonoBehaviour
{
    // Public Variables
    public VideoPlayer videoPlayer;
    public GameObject playButton;
    public GameObject pauseButton;
    public Slider timelineSlider;

    // Private Variables
    private string lastFunction = "Play";

    // Set Video Frame and Timeline Slider Value at Start
    void Start()
    {
        videoPlayer.frame += VideoStartTime.videoStartTime;
        timelineSlider.value = (float)VideoStartTime.videoStartTime / (float)videoPlayer.frameCount;
    }

    // Update Timeline Slider Every Frame
    void Update()
    {
        // *** IF VIDEO GETS STUCK ***
        
        // Check if Pause Button is Visible
        if (pauseButton.activeInHierarchy)
        {
            videoPlayer.Play();
        }
        
        // *** UPDATING TIMELINE SLIDER ***

        // If Video is Reached End
        if (((float)videoPlayer.frameCount - (float)videoPlayer.frame) <= 1.0f)
        {
            pauseButton.SetActive(false);
            playButton.SetActive(true);
            lastFunction = "Pause";
        }
        // If Video is at Beginning
        else if (videoPlayer.frame == 0.0f)
        {
            pauseButton.SetActive(true);
            playButton.SetActive(false);
            lastFunction = "Play";
        }

        // *** CHECKING AND UPDATING SLIDER IF USER IS SCRUBBING THROUGH TIMELINE ***

        // Adjusting Timeline Slider and Video Frame if Slider is Touched
        if (Input.touchCount > 0)
        {
            // Checking if Touching UI Element   
            int id = Input.GetTouch(0).fingerId;
            if (EventSystem.current.IsPointerOverGameObject(id))
            {
                // Checking if Touched UI Element is Timeline Slider
                GameObject currentObj = EventSystem.current.currentSelectedGameObject.gameObject;
                if (currentObj.CompareTag("Timeline"))
                {
                    // Skip to Frame Based on Slider Input
                    videoPlayer.frame = (long)(timelineSlider.value * videoPlayer.frameCount);
                }
                else
                {
                    // Continuing to Update Timeline Slider if Not Touched
                    timelineSlider.value = (float)videoPlayer.frame / (float)videoPlayer.frameCount;
                }    
            }
        }
        // Continuing to Update Timeline Slider if Not Touched
        else
        {
            timelineSlider.value = (float)videoPlayer.frame / (float)videoPlayer.frameCount;
        }
    }

    // Function that Plays or Pauses Video and Toggles Button
    public void playPauseVideo()
    {
        // Play Video and Toggle Buttons
        if (lastFunction == "Pause")
        {
            videoPlayer.Play();
            pauseButton.SetActive(!pauseButton.activeInHierarchy);
            playButton.SetActive(!playButton.activeInHierarchy);
            lastFunction = "Play";
        }

        // Pause Video and Toggle Buttons
        else if (lastFunction == "Play")
        {
            videoPlayer.Pause();
            pauseButton.SetActive(!pauseButton.activeInHierarchy);
            playButton.SetActive(!playButton.activeInHierarchy);
            lastFunction = "Pause";
        }
    }
}
