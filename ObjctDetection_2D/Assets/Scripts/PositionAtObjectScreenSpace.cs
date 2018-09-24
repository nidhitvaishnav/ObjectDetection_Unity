using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PositionAtObjectScreenSpace : MonoBehaviour {

    private float _camDistance;
    Renderer rnd;
    // Use this for initialization
    void Start () {
        rnd = GetComponent<Renderer>();
        _camDistance = Vector3.Distance(Camera.main.transform.position, transform.position);
    }

    // Update is called once per frame
    void Update () {
        if (OpenCVObjectDetection.NormalizedObjPositions.Count == 0)
            return;
        float x = OpenCVObjectDetection.NormalizedObjPositions[0].x;
        float y = OpenCVObjectDetection.NormalizedObjPositions[0].y;
        byte r = (byte)(OpenCVObjectDetection.objectColor[0].x);
        byte g = (byte)(OpenCVObjectDetection.objectColor[0].y);
        byte b = (byte)(OpenCVObjectDetection.objectColor[0].z);

        Debug.LogWarningFormat("XY: (" + x + "," + y + ")", GetType());
        Debug.LogWarningFormat("RGB: (" + r + "," + g + "," + b + ")", GetType());
        transform.position = Camera.main.ViewportToWorldPoint(new Vector3(x,y, _camDistance));
        rnd.material.color = new Color32(r,g,b,(byte)1);
    }
}
