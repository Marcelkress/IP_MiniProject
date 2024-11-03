import cv2
import mediapipe as mp
import time
import socket
import json

# Hand tracking code from: https://github.com/Sousannah/hand-tracking-using-mediapipe.git

cap = cv2.VideoCapture(0)

mpHands = mp.solutions.hands
hands = mpHands.Hands()
mpDraw = mp.solutions.drawing_utils

pTime = 0
cTime = 0

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) # UDP
serverAddressPort = ("127.0.0.1", 5052)

while True:
    success, img = cap.read()
    img = cv2.flip(img, 1)
    imgRGB = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    results = hands.process(imgRGB)

    all_hands_data = []

    if results.multi_hand_landmarks:
        for hand_idx, handLms in enumerate(results.multi_hand_landmarks):
            for id, lm in enumerate(handLms.landmark):
                h, w, c = img.shape
                cx, cy = int(lm.x * w), int(lm.y * h)
                #print(id, cx, cy)
                if id == 4 :
                    cv2.circle(img, (cx, cy), 15, (255, 0, 255), cv2.FILLED)
                if id == 8 :
                    cv2.circle(img, (cx, cy), 15, (255, 0, 255), cv2.FILLED)
                if id == 12 :
                    cv2.circle(img, (cx, cy), 15, (255, 0, 255), cv2.FILLED)
                if id == 16 :
                    cv2.circle(img, (cx, cy), 15, (255, 0, 255), cv2.FILLED)
                if id == 20 :
                    cv2.circle(img, (cx, cy), 15, (255, 0, 255), cv2.FILLED)

            mpDraw.draw_landmarks(img, handLms, mpHands.HAND_CONNECTIONS)

            landmarks = []
            for id, lm in enumerate(handLms.landmark):
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


    cTime = time.time()
    fps = 1 / (cTime - pTime)
    pTime = cTime

    cv2.putText(img, str(int(fps)), (10, 70), cv2.FONT_HERSHEY_PLAIN, 3,
                (255, 0, 255), 3)

    cv2.imshow("Image", img)
    key = cv2.waitKey(1) & 0xFF
    if key == ord('q'):
        break

cap.release()
sock.close()
cv2.destroyAllWindows()