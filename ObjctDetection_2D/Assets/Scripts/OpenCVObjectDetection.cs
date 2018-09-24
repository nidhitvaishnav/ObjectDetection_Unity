using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

// Define the functions which can be called from the .dll.
internal static class OpenCVInterop
{
    [DllImport("ObjectDetection_DLL")]
    internal static extern int Init(ref int outCameraWidth, ref int outCameraHeight);

    [DllImport("OpenCVnUnity")]
    internal static extern int Close();

    [DllImport("ObjectDetection_DLL")]
    internal static extern int SetScale(int downscale);

    [DllImport("ObjectDetection_DLL")]
    internal unsafe static extern void Detect(CoOrdinates_Color* output);
}

// Define the structure to be sequential and with the correct byte size (5 ints = 4 bytes * 3 = 20 bytes)
[StructLayout(LayoutKind.Sequential, Size = 20)]
public struct CoOrdinates_Color
{
    public int X, Y, B, G, R;
}


public class OpenCVObjectDetection : MonoBehaviour
{
    public static List<Vector2> NormalizedObjPositions { get; private set; }
    public static List<Vector3> objectColor { get; private set; }
    public static Vector2 CameraResolution;

    /// <summary>
    /// Downscale factor to speed up detection.
    /// </summary>
    private const int DetectionDownScale = 1;

    private bool _ready;
    //private int _maxFaceDetectCount = 5;
    private CoOrdinates_Color[] _object;
    private float _camDistance;
    //public Renderer rnd;
    //public Color32 myColor;
    void Start()
    {
        int camWidth = 0, camHeight = 0;
        int result = OpenCVInterop.Init(ref camWidth, ref camHeight);
        if (result < 0)
        {
            if (result == -1)
            {
                Debug.LogWarningFormat("[{0}] Failed to open camera stream.", GetType());
            }
      //      rnd = GetComponent<Renderer>();
            _camDistance = Vector3.Distance(Camera.main.transform.position, transform.position);
            return;
        }

        CameraResolution = new Vector2(camWidth, camHeight);
        _object = new CoOrdinates_Color[1];
        NormalizedObjPositions = new List<Vector2>();
        objectColor = new List<Vector3>();
        OpenCVInterop.SetScale(DetectionDownScale);
        _ready = true;
    }
    void OnApplicationQuit()
    {
        if (_ready)
        {
            Debug.LogWarningFormat("before closing application", GetType());
            OpenCVInterop.Close();
            Debug.LogWarningFormat("before closing application", GetType());
        }
    }


    void Update()
    {
        if (!_ready)
            return;

        int detectedObjectCount = 1;
        unsafe
        {
            fixed (CoOrdinates_Color* outObj = _object)
            {
               // Debug.LogWarningFormat("Before calling Detect()", GetType());
                OpenCVInterop.Detect(outObj);
               // Debug.LogWarningFormat("After calling Detect()", GetType());
            }
        }
        NormalizedObjPositions.Clear();
        objectColor.Clear();
        for (int i = 0; i < detectedObjectCount; i++)
        {
            float objX = (_object[i].X * DetectionDownScale) / CameraResolution.x;
            float objY = 1f - ((_object[i].Y * DetectionDownScale) / CameraResolution.y);
            
            NormalizedObjPositions.Add(new Vector2(objX, objY));
            objectColor.Add(new Vector3(_object[i].R, _object[i].G, _object[i].B));


        }
    }
}
