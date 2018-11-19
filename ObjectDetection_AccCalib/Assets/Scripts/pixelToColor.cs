using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class pixelToColor : MonoBehaviour {

    //Initializations
    public static Vector2 CameraResolution;
    private const int DetectionDownScale = 1;
    private CoOrdinates_Color[] _object;
    private float _camDistance;
    private int x, y;
    Renderer rnd;

    // Define the functions which can be called from the .dll.
    internal static class OpenCVInterop
    {
        [DllImport("ObjectDetection_DLL")]
        internal unsafe static extern void ObtainColor(int xPix, int yPix, CoOrdinates_Color* output);
    }



    void Start () {
        //initializing objects
        _object = new CoOrdinates_Color[1];
        rnd = GetComponent<Renderer>();
    }


    // Update is called once per frame
    void Update () {
        //if no color data is passed, return
        if (PositionAtObjectScreenSpace.pixelPos.Count == 0)
            return;

        //get pixelX and PixelY positions from PositionAtObjectScreenSpace
        x = (int)(PositionAtObjectScreenSpace.pixelPos[0].x);
        y = (int)(PositionAtObjectScreenSpace.pixelPos[0].y);

        Debug.LogWarningFormat("(xPix, yPix): (" + x + "," + y + ")", GetType());

        //getting object color from dll based on the pixel values provided by PositionAtObjectScreenSpace
        int detectedObjectCount = 1;
        unsafe
        {
            fixed (CoOrdinates_Color* outObj = _object)
            {
                OpenCVInterop.ObtainColor(x, y ,outObj);        //call to dll
            }
        }
        //reading color from dll
        for (int i = 0; i < detectedObjectCount; i++)
        {
            Debug.LogWarningFormat("calib RGB: (" + _object[i].R + "," + _object[i].G + "," + _object[i].B + ")", GetType());
            //set color passed through dll
            byte r = (byte)(_object[i].R);
            byte g = (byte)(_object[i].G);
            byte b = (byte)(_object[i].B);
            rnd.material.color = new Color32(r, g, b, (byte)1);
        }
    }
}
