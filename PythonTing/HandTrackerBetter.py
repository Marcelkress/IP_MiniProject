# STEP 1: Import the necessary modules.
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
import cv2
import socket
import json

# Function to draw landmarks on the image
def draw_landmarks_on_image(image, detection_result):
    annotated_image = image.copy()
    for hand_landmarks in detection_result.hand_landmarks:
        for landmark in hand_landmarks:
            x = int(landmark.x * image.shape[1])
            y = int(landmark.y * image.shape[0])
            cv2.circle(annotated_image, (x, y), 5, (0, 255, 0), -1)
    return annotated_image

# STEP 2: Create a HandLandmarker object.
base_options = python.BaseOptions(model_asset_path='hand_landmarker.task')
options = vision.HandLandmarkerOptions(base_options=base_options, num_hands=2)
detector = vision.HandLandmarker.create_from_options(options)

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) # UDP
serverAddressPort = ("127.0.0.1", 5052)

# STEP 3: Initialize the webcam.
cap = cv2.VideoCapture(0)

while True:
    # STEP 4: Capture a frame from the webcam.q
    ret, frame = cap.read()
    if not ret:
        break   

    frame = cv2.flip(frame, 1)

    # Convert the frame to RGB as MediaPipe expects RGB images.
    frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    

    # Create a MediaPipe Image object from the frame.
    image = mp.Image(image_format=mp.ImageFormat.SRGB, data=frame_rgb)


    # STEP 5: Detect hand landmarks from the frame.
    detection_result = detector.detect(image)

    all_hands_data = []

    for hand_idx, hand_landmarks in enumerate(detection_result.hand_landmarks):
        landmarks = []
        for lm in hand_landmarks:
            landmarks.append({
                'x': lm.x,
                'y': lm.y,
                'z': lm.z
            })

        all_hands_data.append({            
            'hand_index': hand_idx,
            'landmarks': landmarks
            })

    # Serialize into a json file
    data = json.dumps({'hands': all_hands_data})

    # Send json file over UDP
    sock.sendto(data.encode(), serverAddressPort)

    # STEP 6: Process the classification result and visualize it.
    annotated_image = draw_landmarks_on_image(frame, detection_result)
    cv2.imshow('Hand Landmarks', annotated_image)

    # Exit the loop when 'q' is pressed.
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Release the webcam and close the OpenCV window.
cap.release()
cv2.destroyAllWindows()