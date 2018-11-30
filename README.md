# ObjectDetection_Unity
Given project contains the object detection using C++ and OpenCV with HSV thresholds and based on object color and movement, convert unity object color into given color and translate real word object movement into unity object.

#### ObjectDetection_Unity repository is devided into 2 parts:
- ObjectDetection_2D (Unity project)
- ObjectDetection_AccGyroscope (Unity Project)
- ObjectDeteciton_AccCalib (Unity project)
- ObjectDetection_DLL (C++ project to generate .dll for Object Detection)
- CameraCalib


### Set up
- start objectDetection_2D (or ObjectDetection_AccGyroscope or ObjectDeteciton_AccCalib) in unity,
- Download OpenCV (I am using opencv 3.4.2 version)
- Go to OpenCV -> build -> x64 -> vc15 -> bin
- Copy opencv_world342.dll and opencvv_world342d.dll (You might have different numbers based on version)
  into ObjectDetection_2D (or ObjectDetection_AccGyroscope or ObjectDeteciton_AccCalib) -> Assets -> Plugins
- Now click on run button in Unity.
- For ObjectDetection_AccGyroscope or ObjectDeteciton_AccCalib download Unity Remote 5 in your android phone to see the movement of object
- Set the slider based on HSV threshold image to detect the object.
- Once thresholds has been set and object is able to detect properly,
change the color of object/ move the object.

### ObjectDetection_2D:
- Given project detects the object based on its color
- It changes the color of unity object based on the color of real object
- Here, object location is changing based on the movement of the object in the real world

### ObjectDetection_AccGyroscope
- Given project is an extension of ObjectDetection_2D using android application Unity remote 5.
- Here, apart from detecting and changing color of the object, object is being moved based on accelerometer or gyroscope of the andoid cellphone.
- Here, switch between accelerometer and gyroscope is happening with help of tap on the android screen in Unity Remote 5 application

### ObjectDeteciton_AccCalib
- Here camera calibaration is done using cameraCalib and we are using those cameraCalibaration matrix here
- Here, object is detected based on the laptop webcam, and calibaration matrix is applied.
- Here movement of object is being moved based on accelerometer of android cellphone.

### ObjectDetection_DLL
- This c++ script turn on the laptop camera, detect the color of object using scrollbar and pass the color and location of the object to C# script of unity application
- To see the code Go to ObjectDetection_DLL -> ObjectDetection_DLL -> source.cpp

### cameraCalib
- This Python script gets camera calibaration matrix of the laptop webcam
