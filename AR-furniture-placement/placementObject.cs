// This script creates a class of objects that will be spawned on a ground plane in the app.
// The Placement Object class can lock, select, or display real-world dimensions of an object with a bounding box around it.

using Lean.Touch;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlacementObject : MonoBehaviour
{
    [SerializeField]
    private bool IsSelected;

    [SerializeField]
    private bool IsLocked;

    [SerializeField]
    private GameObject boundingBox;

    public GameObject dimensionsPanel;

    [SerializeField]
    private TextMeshPro dimensionsText;

    public UnityEngine.Vector3 initialDimensions;
    private UnityEngine.Vector3 initialScale;

    private void Start()
    {
        dimensionsText.text = initialDimensions.ToString();
        initialScale = dimensionsPanel.transform.localScale;
    }

    private void Update()
    {
        // Getting Parent Transform
        UnityEngine.Vector3 parentTransform = gameObject.transform.parent.localScale;
        
        // Getting New Dimensions and Updating Text
        UnityEngine.Vector3 newDimensions = initialDimensions;
        newDimensions.x *= parentTransform.x;
        newDimensions.y *= parentTransform.y;
        newDimensions.z *= parentTransform.z;
        dimensionsText.text = vectorToString(newDimensions);

        // Calculating New Scale and Scaling Panel
        UnityEngine.Vector3 newScale = new UnityEngine.Vector3();
        newScale = initialScale * (1 / parentTransform.x);
        dimensionsPanel.transform.localScale = newScale;
    }

    private string vectorToString(UnityEngine.Vector3 trans)
    {
        string scaleX = trans.x.ToString("F2") + " x ";
        string scaleY = trans.y.ToString("F2") + " x ";
        string scaleZ = trans.z.ToString("F2") + " inches";
        string sizeString = scaleX + scaleY + scaleZ;
        return sizeString;
    }

    public bool Selected
    {
        get
        {
            return this.IsSelected;
        }
        set
        {
            IsSelected = value;
            boundingBox.SetActive(value);
            dimensionsPanel.SetActive(value);
        }
    }

    public bool Locked
    {
        get
        {
            return this.IsLocked;
        }
        set
        {
            IsLocked = value;
        }
    }
}
