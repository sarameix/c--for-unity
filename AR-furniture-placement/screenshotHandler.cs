// This script handles all the functions needed to run a screenshot display page on the app.
// This script allows the user to scroll through and delete any screenshots taken on the app.
// This script only works when built to Android devices.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;
using System.IO;

public class ScreenshotHandler : MonoBehaviour
{
    [SerializeField]
    GameObject canvas;

    [SerializeField]
    Sprite empty;

    string[] files = null;
    public int visibleScreenshot = 0;

    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        files = Directory.GetFiles(Application.persistentDataPath + "/", "*.png");
        if (files.Length > 0)
        {
            GetPictureAndShowIt();
        }
        else
        {
            canvas.GetComponent<Image>().sprite = empty;
        }
    }

    void GetPictureAndShowIt()
    {
        if (files.Length > 0)
        {
            string pathToFile = files[visibleScreenshot];
            Texture2D texture = GetScreenshotImage(pathToFile);
            Sprite spriteTex = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            canvas.GetComponent<Image>().sprite = spriteTex;
        }
        else
        {
            canvas.GetComponent<Image>().sprite = empty;
        }
    }

    Texture2D GetScreenshotImage(string filePath)
    {
        Texture2D texture = null;
        byte[] fileBytes;
        if (File.Exists(filePath))
        {
            fileBytes = File.ReadAllBytes(filePath);
            texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
            texture.LoadImage(fileBytes);
        }
        return texture;
    }

    public void NextPicture()
    {
        if (files.Length > 0)
        {
            files = Directory.GetFiles(Application.persistentDataPath + "/", "*.png");
            visibleScreenshot += 1;
            if (visibleScreenshot > (files.Length-1))
            {
                visibleScreenshot = 0;
            }
            GetPictureAndShowIt();
        }
    }

    public void PreviousPicture()
    {
        if (files.Length > 0)
        {
            files = Directory.GetFiles(Application.persistentDataPath + "/", "*.png");
            visibleScreenshot -= 1;
            if (visibleScreenshot < 0)
            {
                visibleScreenshot = files.Length - 1;
            }
            GetPictureAndShowIt();
        }
    }

    public void DeleteCurrent()
    {
        if (files.Length > 0)
        {
            string pathToFile = files[visibleScreenshot];
            System.IO.File.Delete(pathToFile);
            files = Directory.GetFiles(Application.persistentDataPath + "/", "*.png");
            if ((visibleScreenshot > (files.Length - 1)) && (visibleScreenshot > 0))
            {
                visibleScreenshot -= 1;
            }
            GetPictureAndShowIt();
        }
    }
}
