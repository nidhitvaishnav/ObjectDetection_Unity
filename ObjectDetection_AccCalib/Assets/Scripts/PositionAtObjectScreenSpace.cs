using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PositionAtObjectScreenSpace : MonoBehaviour {
    //initialization
    public static AudioClip moveSound;
    static AudioSource audioSrc;
    private float _camDistance;
    Renderer rnd;
    private float x, y, z;
    Text myText;
    private int translateCount = 0;
    public Camera cam;
    public static List<Vector2> pixelPos { get; private set; }
    private int camW = 640, camH = 480;


    // Use this for initialization
    void Start () {

        //Initializing Renderer for color
        rnd = GetComponent<Renderer>();
        pixelPos = new List<Vector2>();

        //Initializing text on canvas
        myText = GameObject.Find("Text").GetComponent<Text>();
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        
        //Initializing sound
        audioSrc = GetComponent<AudioSource>();
        moveSound = Resources.Load<AudioClip>("move");
      }

    // Update is called once per frame
    void Update () {
        //if no color data is passed, return
        if (OpenCVObjectDetection.objectColor.Count == 0)
            return;
       
        //motion/rotation process
        x = Input.acceleration.x;
        z = Input.acceleration.z;
        y = Input.acceleration.y;

        //sound of motion
        PlaySound("move");

        /*camera calibaration matrix (fx, fy, cx, cy are in pixels, but we can use them in mm according to openCV)
         * Reference: http://answers.opencv.org/question/189506/understanding-the-result-of-camera-calibration-and-its-units/
         * |x|      |510.42788783   0.              322.97625818 | |X|
         * |y| =    |0.             499.34270891    270.7047106  | |Y|
         * |z|      |0.             0.              1.           | |Z|
         * */

        double calibX = 5.1043 * x + 3.2298 * z;
        double calibY = 4.9934 * y + 2.7070 * z;
        double calibZ = z;

        //move object
        transform.Translate(x,0, -z);
        translateCount += 1;
        
        Debug.LogWarningFormat("displacement between real and calib XYZ: (" + (calibX-x) + "," + (calibY-y) + "," + (calibZ-z) + ")", GetType());
       
        //set text
        myText.text = "calib XYZ=(" + calibX +","+ calibY +","+calibZ +")";

        //converting accelerometer values into pixel and passing to pixelToColor.cs
        int xPix = FindPixVal(calibX, camW, z);
        int yPix = FindPixVal(calibY, camH, z);

        pixelPos.Clear();
        pixelPos.Add(new Vector2(xPix, yPix));
        
    }

    //function to find pixel
    public int FindPixVal(double val, int frameSize, double z)
    {
        //converting calibX and calibY in the frameSize for pixel (-ve, +ve)
        int range = frameSize / 2;
        double pixel=0;
        if ((val+range)<640 && (val+range)>0)
            pixel = val+range;
        return (int)pixel;
    }

    //function that plays sound
    public void PlaySound(string clip) 
    {
        audioSrc.PlayOneShot(moveSound);
    }

    //find abs value
    public float FindAbs(float n)
    {
        if (n < 0){
            n = n * (-1);
        }
        return n;
    }


}
