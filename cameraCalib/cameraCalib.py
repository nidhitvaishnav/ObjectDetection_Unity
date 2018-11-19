import numpy as np
import cv2
from datetime import datetime
from Webcam import Webcam
  
webcam = Webcam()
webcam.start()

# termination criteria
criteria = (cv2.TERM_CRITERIA_EPS + cv2.TERM_CRITERIA_MAX_ITER, 30, 0.001)

# prepare object points, like (0,0,0), (1,0,0), (2,0,0) ....,(6,5,0)
objp = np.zeros((6*7,3), np.float32)
objp[:,:2] = np.mgrid[0:7,0:6].T.reshape(-1,2)

# Arrays to store object points and image points from all the images.
objpoints = [] # 3d point in real world space
imgpoints = [] # 2d points in image plane.
count = 0
while count<50:     
    # get image from webcam
    image = webcam.get_current_frame()
 
    # display image
    cv2.imshow('grid', image)
    cv2.waitKey(1000)
 
    # save image to file, if pattern found
    gray = cv2.cvtColor(image,cv2.COLOR_BGR2GRAY)
    ret, corners = cv2.findChessboardCorners(gray, (7,6), None)
    
    # If found, add object points, image points (after refining them)
    if ret == True:
        objpoints.append(objp)

        corners2 = cv2.cornerSubPix(gray,corners,(11,11),(-1,-1),criteria)
        imgpoints.append(corners2)

        # Draw and display the corners
        img = cv2.drawChessboardCorners(image, (7,6), corners2,ret)
#         cv2.imshow('img',img)
#         cv2.waitKey(500)
    
        filename = datetime.now().strftime('%Y%m%d_%Hh%Mm%Ss%f') + '.jpg'
        cv2.imwrite("img/" + filename, image)
        ret, mtx, dist, rvecs, tvecs = cv2.calibrateCamera(objpoints, imgpoints, gray.shape[::-1],None,None)
        h,  w = img.shape[:2]
        newcameramtx, roi=cv2.getOptimalNewCameraMatrix(mtx,dist,(w,h),1,(w,h))
        if newcameramtx[0][0]>600 or newcameramtx[0][0]<400:
            continue
#         # undistort
#         dst = cv2.undistort(img, mtx, dist, None, newcameramtx)
#         
#         # crop the image
#         x,y,w,h = roi
#         dst = dst[y:y+h, x:x+w]

        # debug
        print("newcameramtx =\n {}".format(newcameramtx))
        # debug -ends
        
        
        #taking average of all camera matrix
        if count==0:
            avgCameraMatrix=newcameramtx
            oldCameraMatrix = avgCameraMatrix
        else:
            avgCameraMatrix = (oldCameraMatrix*count+newcameramtx)/(count+1)
        # debug
        print("\n-------------------------------------------------")
        print("avgCameraMatrix =\n{}".format(avgCameraMatrix))
        print("-------------------------------------------------\n")
        oldCameraMatrix = avgCameraMatrix
        count+=1
# debug -ends
#while -ends


