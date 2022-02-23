// This script handles taking screenshots by displaying a fake "flash" on the screen, capturing the appropriate elements, and storing the photo in the proper file path.
// This script only works when built to Android devices.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TakeScreenshot : MonoBehaviour
{
    [SerializeField]
    private GameObject blink;

    private Animator blinkAnim;

    private void Start()
    {
        blinkAnim = blink.GetComponent<Animator>();
        blink.SetActive(false);
    }

    public void Screenshot()
    {
        StartCoroutine("CaptureScreen");
    }
    
    IEnumerator CaptureScreen()
    {
        // Creating File Path
        string timeStamp = System.DateTime.Now.ToString("MM-dd-yyyy-HH-mm-ss");
        string fileName = "Screenshot_" + timeStamp + ".png";
        string pathToSave = fileName;

        // Hide Objects to be Omitted from Capture
        List<Image> needToUnhide = new List<Image>();
        foreach (Image omitObject in Resources.FindObjectsOfTypeAll(typeof(Image)) as Image[])
        {
            if(omitObject.color.a > 0.0)
            {
                needToUnhide.Add(omitObject);
            }
            var tempColor = omitObject.color;
            tempColor.a = 0.0f;
            omitObject.color = tempColor;
        }

        // Hide AR Planes
        List<LineRenderer> allLineRenderers = new List<LineRenderer>();
        foreach (LineRenderer lineRender in FindObjectsOfType(typeof(LineRenderer)) as LineRenderer[])
        {
            allLineRenderers.Add(lineRender);
            lineRender.gameObject.SetActive(false);
        }


        // Taking Screenshot
        ScreenCapture.CaptureScreenshot(pathToSave);

        // Camera Flash After Screenshot Captured
        yield return new WaitForEndOfFrame();
        blink.SetActive(true);
        blinkAnim.SetBool("isBlinking", true);
        float waitTime = blinkAnim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(waitTime);
        blink.SetActive(false);
        blinkAnim.SetBool("isBlinking", false);

        // Unhide Omitted Objects
        foreach (Image omitObject in needToUnhide)
        {
            var tempColor = omitObject.color;
            tempColor.a = 1f;
            omitObject.color = tempColor;
            Debug.Log(omitObject.gameObject.name);
        }

        // Unhide AR Planes
        foreach (LineRenderer lineRender in allLineRenderers)
        {
            lineRender.gameObject.SetActive(true);
        }
    }
}
