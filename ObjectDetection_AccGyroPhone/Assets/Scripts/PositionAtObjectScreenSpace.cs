using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PositionAtObjectScreenSpace : MonoBehaviour {
    //initialization
    public static AudioClip moveSound, rotateSound;
    static AudioSource audioSrc;
    private float _camDistance;
    private bool gyroEnabled;
    private Gyroscope gyro;
    private Quaternion rot;
    Renderer rnd;
    private float x, y, z;
    private bool touchEnabled;
    private bool moveTouchFlag = true;
    Text myText;
    private int translateCount, rotateCount = 0;


    // Use this for initialization
    void Start () {

        //Initializing Renderer for color
        rnd = GetComponent<Renderer>();

        //Initializing text on canvas
        myText = GameObject.Find("Text").GetComponent<Text>();

        //Initializing motion
        _camDistance = Vector3.Distance(Camera.main.transform.position, transform.position);
        gyroEnabled = enableGyro();
        touchEnabled = enableTouch();

        //Initializing sound
        moveSound = Resources.Load<AudioClip>("move");
        rotateSound = Resources.Load<AudioClip>("rotate");
        audioSrc = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update () {
        //if no color data is passed, return
        if (OpenCVObjectDetection.objectColor.Count == 0)
            return;        

        //Initialize with motion, on screen touch, switch between translate and rotation
        if (Input.touchCount > 0)
        {
            //switch with help of moveTouchFlag
            //when moveTouchFlag is true, disable gyro, and make object at initial rotate position 
            //when moveTouchFlag is false, enable gyro
            if (moveTouchFlag)
            {
                moveTouchFlag = false;
                Input.gyro.enabled = true;
            }
            else
            {
                moveTouchFlag = true;
                Input.gyro.enabled = false;
                transform.rotation = Quaternion.identity;

            }
        }
        //motion/rotation process
        if (moveTouchFlag)
        {
            //rotation with respect to y axis
            x = Input.acceleration.x;
            z = Input.acceleration.z;
            y = Input.acceleration.y;
            
            //sound of motion
            PlaySound("move");

            //move object
            transform.Translate(x,0, -z);
            translateCount += 1;

            Debug.LogWarningFormat("XYZ: (" + x + "," + y + "," + z + ")", GetType());
        }
        else
        {
            //rotate object and play rotation sound
            gameObject.transform.rotation = Input.gyro.attitude;
            PlaySound("rotate");
            rotateCount += 1;
        }
        //set color
        byte r = (byte)(OpenCVObjectDetection.objectColor[0].x);
        byte g = (byte)(OpenCVObjectDetection.objectColor[0].y);
        byte b = (byte)(OpenCVObjectDetection.objectColor[0].z);
        rnd.material.color = new Color32(r, g, b, (byte)1);
        //set text
        myText.text = "Count:- (Traslation, rotation) =(" + translateCount + ", " + rotateCount+")";

        Debug.LogWarningFormat("RGB: (" + r + "," + g + "," + b + ")", GetType());
    }

    //check whether device supports gyroscope
    private bool enableGyro()
    {
        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
            return true;
        }
        return false;
    }

    //check whether device supports touch
    private bool enableTouch()
    {
        if (Input.touchSupported)
        {
            return true;
        }
        return false;
    }

    //function that plays sound
    public void PlaySound(string clip) 
    {
        switch (clip){
            case "move":
                audioSrc.PlayOneShot(moveSound);
                break;
            case "rotate":
                audioSrc.PlayOneShot(rotateSound);
                break;
        }
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
