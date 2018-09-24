#include <sstream>
#include <string>
#include <iostream>
#include "opencv2/highgui.hpp"
#include "opencv2/imgproc.hpp"
#include "opencv2/objdetect.hpp"
#include "opencv2/imgproc.hpp"

using namespace cv;
using namespace std;

// Declare structure to be used to pass data from C++ to Mono.
struct CoOrdinates_Color
{
	CoOrdinates_Color(int x, int y, int b, int g, int r) : X(x), Y(y), B(b), G(g), R(r){}
	int X, Y, B, G, R;
};
int r, g, b;
//initial min and max HSV filter values. - can be changed using trackbar window
int H_MIN = 0;

int H_MAX = 256;
int S_MIN = 0;
int S_MAX = 256;
int V_MIN = 0;
int V_val = 0;
int V_MAX = 256;
RNG rng(12345);
//video capture object to acquire webcam feed
VideoCapture capture;
int _scale = 1;
//default capture width and height
const int FRAME_WIDTH = 640;
const int FRAME_HEIGHT = 480;

//max number of objects to be detected in frame
const int MAX_NUM_OBJECTS = 50;

//minimum and maximum object area
const int MIN_OBJECT_AREA = 20 * 20;
const int MAX_OBJECT_AREA = FRAME_HEIGHT * FRAME_WIDTH / 1.5;

//names that will appear at the top of each window
const string windowNameFeed = "Original Image";
const string windowNameHSV = "HSV Image";
const string windowNameThr = "Thresholded Image";
const string windowNameFiltered = "After noise elimination";
const string trackbarWindowName = "Trackbars";

void on_trackbar(int, void*)
{//This function gets called whenever a
 // trackbar position is changed
}

string  intToString(int number) {
	std::stringstream ss;
	ss << number;
	return ss.str();
}


void createTrackbarWindow() {
	//create window for trackbars with window
	namedWindow(trackbarWindowName, 0);
	//create memory to store trackbar name on window
	char TrackbarName[50];
	sprintf_s(TrackbarName, "H_MIN", 0);
	sprintf_s(TrackbarName, "H_MAX", 256);
	sprintf_s(TrackbarName, "S_MIN", 0);
	sprintf_s(TrackbarName, "S_MAX", 256);
	sprintf_s(TrackbarName, "V_MIN", 0);
	sprintf_s(TrackbarName, "V_MAX", 256);
	//creating trackbars and adding them into window
	//parameters:  Threshold variable that is changing, min value, max value, function to move trackbar 
	createTrackbar("H_MIN", trackbarWindowName, &H_MIN, H_MAX, on_trackbar);
	createTrackbar("H_MAX", trackbarWindowName, &H_MAX, 256, on_trackbar);
	createTrackbar("S_MIN", trackbarWindowName, &S_MIN, S_MAX, on_trackbar);
	createTrackbar("S_MAX", trackbarWindowName, &S_MAX, 256, on_trackbar);
	createTrackbar("V_MIN", trackbarWindowName, &V_MIN, V_MAX, on_trackbar);
	createTrackbar("V_MAX", trackbarWindowName, &V_MAX, 256, on_trackbar);
}

void drawObject(int x, int y, Mat &frame, vector< vector<Point> > &contours, Vec3b &colour) {

	//use some of the openCV drawing functions to draw crosshairs
	//on your tracked image!

	//added 'if' and 'else' statements to prevent
	//memory errors from writing off the screen (ie. (-25,-25) is not within the window!)
	//finding bounding box of countour size
	vector<vector<Point> > contours_poly(contours.size());
	vector<Rect> boundRect(contours.size());
	for (int i = 0; i < contours.size(); i++)
	{
		approxPolyDP(Mat(contours[i]), contours_poly[i], 3, true);
		boundRect[i] = boundingRect(Mat(contours_poly[i]));
	}
	/// Draw polygonal contour + bonding rects
	int b = colour[0];
	int g = colour[1];
	int r = colour[2];
	for (int i = 0; i< contours.size(); i++)
	{
		Scalar color = Scalar(r, g, b);
		drawContours(frame, contours_poly, i, color, 1, 8, vector<Vec4i>(), 0, Point());
		rectangle(frame, boundRect[i].tl(), boundRect[i].br(), color, 2, 8, 0);
	}


	circle(frame, Point(x, y), 5, Scalar(r, g, b), 2);
	if (y - 25>0)
		line(frame, Point(x, y), Point(x, y - 25), Scalar(r, g, b), 2);
	else line(frame, Point(x, y), Point(x, 0), Scalar(r, g, b), 2);
	if (y + 25<FRAME_HEIGHT)
		line(frame, Point(x, y), Point(x, y + 25), Scalar(r, g, b), 2);
	else line(frame, Point(x, y), Point(x, FRAME_HEIGHT), Scalar(r, g, b), 2);
	if (x - 25>0)
		line(frame, Point(x, y), Point(x - 25, y), Scalar(r, g, b), 2);
	else line(frame, Point(x, y), Point(0, y), Scalar(0, 255, 0), 2);
	if (x + 25<FRAME_WIDTH)
		line(frame, Point(x, y), Point(x + 25, y), Scalar(r, g, b), 2);
	else line(frame, Point(x, y), Point(FRAME_WIDTH, y), Scalar(r, g, b), 2);

	putText(frame, intToString(x) + "," + intToString(y), Point(x, y + 30), 1, 1, Scalar(r, g, b), 2);
}


void noiseElimination(Mat &thresh) {

	//Kernel to "dilate" and "erode" image.
	Mat erodeElement = getStructuringElement(MORPH_RECT, Size(3, 3));
	//dilate with larger element so make sure object is nicely visible
	Mat dilateElement = getStructuringElement(MORPH_RECT, Size(8, 8));

	erode(thresh, thresh, erodeElement);
	erode(thresh, thresh, erodeElement);

	dilate(thresh, thresh, dilateElement);
	dilate(thresh, thresh, dilateElement);
}


void trackFilteredObject(int &x, int &y, Mat threshold, Mat &cameraFeed) {

	Mat temp;
	threshold.copyTo(temp);
	//vectors that stores output of findContours
	vector< vector<Point> > contours;
	vector<Vec4i> hierarchy;

	
	//find contours of filtered image
	findContours(temp, contours, hierarchy, CV_RETR_CCOMP, CV_CHAIN_APPROX_SIMPLE);
	Vec3b colour;

	//use moments method to find our filtered object
	double refArea = 0;
	bool objectFound = false;
	if (hierarchy.size() > 0) {
		int numObjects = hierarchy.size();
		//if number of objects greater than MAX_NUM_OBJECTS we have a noisy filter
		if (numObjects<MAX_NUM_OBJECTS) {
			for (int index = 0; index >= 0; index = hierarchy[index][0]) {

				Moments moment = moments((cv::Mat)contours[index]);
				double area = moment.m00;

				//if the area is less than 20 px by 20px then it is probably just noise
				//if the area is the same as the 3/2 of the image size, probably just a bad filter
				//we only want the object with the largest area so we safe a reference area each
				//iteration and compare it to the area in the next iteration.
				if (area>MIN_OBJECT_AREA && area<MAX_OBJECT_AREA && area>refArea) {
					x = moment.m10 / area;
					y = moment.m01 / area;
					//getting color of the center pixel of the object
					colour = cameraFeed.at<Vec3b>(Point(x, y));
					

					objectFound = true;
					refArea = area;
				}
				else objectFound = false;
			}
			
			/// Show the image

			//let user know you found an object
			if (objectFound == true) {
				string blue = intToString(colour[0]);
				string green = intToString(colour[1]);
				string red = intToString(colour[2]);
				b = colour[0];
				g = colour[1];
				r = colour[2];

				putText(cameraFeed, "Tracking Object", Point(0, 50), 2, 1, Scalar(0, 255, 0), 2);
				putText(cameraFeed, "Color:bgr" + blue + "," + green + "," + red, Point(0, 100), 2, 1, Scalar(0, 255, 0), 2);
				//draw object location on screen
				drawObject(x, y, cameraFeed, contours, colour);
			}

		}
		else putText(cameraFeed, "TOO MUCH NOISE! ADJUST FILTER", Point(0, 50), 1, 2, Scalar(0, 0, 255), 2);
	}
}


extern "C" void __declspec(dllexport) __stdcall Detect(CoOrdinates_Color* output)
{
	Mat cameraFeed;
	capture >> cameraFeed;
	if (cameraFeed.empty())
		return;
	// Convert the frame to grayscale for cascade detection.

	//matrix that stores HSV image and binary threshold image
	Mat HSVImg;
	Mat thresholdImg;

	//slider bars window for HSV filters
	createTrackbarWindow();
	//x and y co-ordinates of the object
	int objX = 0, objY = 0;
	//getting color of the center pixel of the object
	Vec3b colour= cameraFeed.at<Vec3b>(Point(objX, objY));;

	//convert frame from BGR to HSV
	cvtColor(cameraFeed, HSVImg, COLOR_BGR2HSV);
	//filter HSV image between threshold values and store filtered image to thresholdImg
	inRange(HSVImg, Scalar(H_MIN, S_MIN, V_MIN), Scalar(H_MAX, S_MAX, V_MAX), thresholdImg);

	// noise elimination 
	noiseElimination(thresholdImg);
	//Object tracking function that returns x and y co-ordinates of the object
	trackFilteredObject(objX, objY, thresholdImg, cameraFeed);
	


	//storing data to pass them to the application
	output[0] = CoOrdinates_Color(objX, objY, b,g,r);

	//debug
	//imshow(windowNameHSV, HSVImg);
	//display output 
	imshow(windowNameThr, thresholdImg);
	imshow(windowNameFeed, cameraFeed);
	waitKey(30);
}


extern "C" int __declspec(dllexport) __stdcall  Init(int& outCameraWidth, int& outCameraHeight)
{
	// Open the stream.
	capture.open(0);
	if (!capture.isOpened())
		return -1;

	outCameraWidth = capture.get(CAP_PROP_FRAME_WIDTH);
	outCameraHeight = capture.get(CAP_PROP_FRAME_HEIGHT);

	return 0;
}

extern "C" void __declspec(dllexport) __stdcall  Close()
{
	destroyAllWindows();
	capture.release();
}
extern "C" void __declspec(dllexport) __stdcall SetScale(int scale)
{
	_scale = scale;
}


	