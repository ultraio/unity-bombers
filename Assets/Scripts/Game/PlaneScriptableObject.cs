using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "PlaneData", menuName = "ScriptableObjects/Plane Data")]
public class PlaneScriptableObject : ScriptableObject
{
    public int planeID;

    public GameObject planeModel_green;
    public GameObject planeModel_red;
    public string planeName;
    public string planeDescription;

    public Sprite planeThumbnail_green;
    public Sprite planeThumbnail_red;
}
