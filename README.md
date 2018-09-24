# ObjectDetection_Unity
Given project contains the object detection using C++ and OpenCV with HSV thresholds and based on object color and movement, convert unity object color into given color and translate real word object movement into unity object.

#### ObjectDetection_Unity repository is devided into 2 parts:
- ObjectDetection_2D (Unity project)
- ObjectDetection_DLL (C++ project to generate .dll for Object Detection)


### To run the code, 
- start objectDetection_2D in unity,
- Download OpenCV (I am using opencv 3.4.2 version)
- Go to OpenCV -> build -> x64 -> vc15 -> bin
- Copy opencv_world342.dll and oepncv_world342d.dll (You might have different numbers based on version)
  into ObjectDetection_2D -> Assets -> Plugins
- Now click on run button in Unity.
- Set the slider based on HSV threshold image to detect the object.
- Once thresholds has been set and object is able to detect properly,
change the color of object/ move the object.

### To see the ObjectDetection_DLL code that works as backend in Unity project,
- Go to ObjectDetection_DLL -> ObjectDetection_DLL -> source.cpp
