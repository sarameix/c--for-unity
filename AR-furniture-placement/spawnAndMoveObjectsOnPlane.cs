// This script is used to handle spawning or transforming objects onto a ground plane in an augmented reality scene.
// There are two modes: Build and Move
// Build Mode allows the user to instantiate objects on a ground plane.
// Move Mode allows the user to translate, rotate, and scale existing objects on the ground plane.
// This script uses the AR Foundation and Lean Touch packages in Unity.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Lean.Touch;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine.Animations;

[RequireComponent(typeof(ARRaycastManager))]

public class SpawnAndMoveObjectsOnPlane : MonoBehaviour
{
    // *** PARAMETERS ***

    // Customizable Parameters

    [SerializeField]
    private GameObject placedPrefab;

    [SerializeField]
    private Camera arCamera;

    public string mode = "Build";

    // Hidden Parameters

    public GameObject testHit;
    public Text testText;
    private List<GameObject> placedObjects = new List<GameObject>();
    private List<PlacementObject> placementObjectList = new List<PlacementObject>();
    private Vector2 touchPosition = default;
    private ARRaycastManager arRaycastManager;
    private bool onTouchHold = false;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private PlacementObject lastSelectedObject;
    private float initialFingersDistance;
    private Vector3 initialScale;
    private float rotateDegrees = 0;
    private GameObject lastDeletedObj;
    private Transform lastDeletedTrans;
    private float scaleFactor = 1;
    private UnityEngine.Animations.ConstraintSource constSource;

    // *** FUNCTIONS ***

    // Get Raycast Manager on Opening App
    void Awake()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
    }

    // Update Function Called Once Per Frame
    void Update()
    {
        // Executing Build Mode
        if (mode == "Build")
        {
            buildMode();
        }
        // Executing Move Mode
        else if (mode == "Move")
        {
            moveMode();
        }
    }

    // Build Mode Function
    void buildMode()
    {
        // Getting Touches and Keeping Track of Last Selected Object
        if (Input.touchCount > 0)
        {
            // Get Touches
            Touch touch = Input.GetTouch(0);
            touchPosition = touch.position;

            // Stopping Function if Hitting UI Element
            if (touchPosition.IsPointOverUIObject())
            {
                return;
            }

            // Setting Hit Object as Last Selected
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = arCamera.ScreenPointToRay(touch.position);
                RaycastHit hitObject;
                if (Physics.Raycast(ray, out hitObject))
                {
                    lastSelectedObject = hitObject.transform.GetComponent<PlacementObject>();
                }
            }

            // Removing Selection When Touch Ends
            if (touch.phase == TouchPhase.Ended)
            {
                lastSelectedObject.Selected = false;
                DeselectAllObjects();
            }
        }

        // Instantiating Object Based on User's Touch Raycast on AR Plane
        if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            // Getting Hits
            Pose hitPose = hits[0].pose;
            Touch touch = Input.GetTouch(0);

            // Instantiating Object if Touch Starts
            if (touch.phase == TouchPhase.Began)
            {
                GameObject placedObj = Instantiate(placedPrefab, hitPose.position, hitPose.rotation);
                //DontDestroyOnLoad(placedObj);
                placedObjects.Add(placedObj);
                lastSelectedObject = placedObj.transform.GetChild(0).gameObject.GetComponent<PlacementObject>();
                placementObjectList.Add(lastSelectedObject);
                AimConstraint aimConst = lastSelectedObject.dimensionsPanel.GetComponent<AimConstraint>();
                constSource.sourceTransform = arCamera.transform;
                constSource.weight = 1.0f;
                aimConst.AddSource(constSource);
            }
        }
    }

    // Move Mode Function
    void moveMode()
    {
        // Get Touches
        Touch touch = Input.GetTouch(0);
        touchPosition = touch.position;

        // Stopping Function if Hitting UI Element
        if (touchPosition.IsPointOverUIObject())
        {
            return;
        }

        // If No Finger On Screen
        if (Input.touchCount == 0)
        {
            DeselectAllObjects();
        }

        // If Screen Touched By One Finger
        if (Input.touchCount == 1)
        {
            // Select Hit Object at Start of Touch
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = arCamera.ScreenPointToRay(touch.position);
                RaycastHit hitObject;
                if (Physics.Raycast(ray, out hitObject))
                {
                    PlacementObject placementObject = hitObject.transform.gameObject.GetComponent<PlacementObject>();
                    if (placementObject != null)
                    {
                        ChangeSelectedObject(placementObject);
                    }
                }
            }

            // Deselect All Objects if Touch Ends
            if (touch.phase == TouchPhase.Ended)
            {
                DeselectAllObjects();
            }
            // Move Objects if Finger Still on Screen
            else
            {
                GameObject selected = CheckSelection();
                if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
                {
                    var hitPose = hits[0].pose;
                    selected.transform.position = hitPose.position;
                }
            }
        }

        if (Input.touchCount == 2)
        {
            // Getting Degree Change in Finger Gesture Per Frame
            rotateDegrees = LeanGesture.GetTwistDegrees();

            // Select Hit Object at Start of Touch
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = arCamera.ScreenPointToRay(touch.position);
                RaycastHit hitObject;
                if (Physics.Raycast(ray, out hitObject))
                {
                    PlacementObject placementObject = hitObject.transform.gameObject.GetComponent<PlacementObject>();
                    if (placementObject != null)
                    {
                        ChangeSelectedObject(placementObject);
                    }
                }
            }

            // Get Selected Object
            GameObject selected = CheckSelection();

            // Getting Initial Scale and Finger Distance
            if (touch.phase == TouchPhase.Began)
            {
                initialFingersDistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
                initialScale = selected.transform.localScale;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                DeselectAllObjects();
            }
            // Scaling and Rotating Object Based on Finger Positioning
            else
            {
                float currentFingersDistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
                if (initialFingersDistance > 0)
                {
                    scaleFactor = currentFingersDistance / initialFingersDistance;
                }
                else
                {
                    scaleFactor = 0.1f;
                }
               
                Vector3 newScale = initialScale * scaleFactor;
                if(newScale.x < 0.1)
                {
                    newScale.x = 0.35f;
                    newScale.y = 0.35f;
                    newScale.z = 0.35f;
                }
                
                selected.transform.localScale = newScale;
                selected.transform.Rotate(0, (-1*rotateDegrees), 0);
                //testText.text = (initialScale*scaleFactor).ToString();
            }
        }
    }

    // Function Setting Prefab to Place
    public void SetPrefabType(GameObject prefabType)
    {
        placedPrefab = prefabType;
    }

    // Function Undoing Last Placed Prefab
    public void UndoLastPlaced()
    {
        GameObject lastObj = placedObjects[placedObjects.Count - 1];
        lastDeletedObj = lastObj;
        placedObjects.RemoveAt(placedObjects.Count - 1);
        placementObjectList.RemoveAt(placementObjectList.Count - 1);
        Destroy(lastObj);
    }

    // Function Redoing Last Placed Prefab
    public void RedoLastPlaced()
    {
        GameObject current = Instantiate(lastDeletedObj, lastDeletedObj.transform.position, lastDeletedObj.transform.rotation);
        placedObjects.Add(current);
        PlacementObject placementObj = current.transform.GetChild(0).gameObject.GetComponent<PlacementObject>();
        placementObjectList.Add(placementObj);
    }

    // Function Clearing Scene and Lists of All Placed Prefabs
    public void ClearScene()
    {
        foreach (GameObject placementObject in placedObjects)
        {
            Destroy(placementObject);
        }
        placedObjects.Clear();
        placementObjectList.Clear();
    }

    // Function Selecting One Object and Deselecting All Others in Scene
    void ChangeSelectedObject(PlacementObject selected)
    {
        foreach (PlacementObject current in placementObjectList)
        {
            MeshRenderer meshRenderer = current.gameObject.GetComponent<MeshRenderer>();
            Outline outline = selected.gameObject.GetComponent<Outline>();
            if (selected != current)
            {
                current.Selected = false;;
            }
            else
            {
                current.Selected = true;
            }
        }

    }

    // Function Deselecting All Placed Objects
    void DeselectAllObjects()
    {
        foreach (PlacementObject current in placementObjectList)
        {
            MeshRenderer meshRenderer = current.gameObject.GetComponent<MeshRenderer>();
            current.Selected = false;
        }
    }

    // Function Returning Parent GameObject of Selected Object
    GameObject CheckSelection()
    {
        GameObject selected = null;
        foreach (PlacementObject current in placementObjectList)
        {
            if(current.Selected)
            {
                selected = current.transform.parent.gameObject;
            }
        }
        return selected;
    }
}
