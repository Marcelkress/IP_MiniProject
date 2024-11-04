using System.Collections.Generic;
using UnityEngine;

public class HandCreator : MonoBehaviour
{
    public UDPReceive UDP;
    public GameObject landMarkObject;
    public int scaleFactor = 15;
    public Transform parentTransform;
    public float waitTime;
    private float timePassed;
    bool called;
    public Vector3 thresholdVector;
    public List<Transform> leftHand;
    public List<Transform> rightHand;
    public float lerpValue;

    void Update()
    {
        timePassed += Time.deltaTime;

        if(timePassed >= waitTime && called == false)
        {
            CreateHands();
            called = true;
        }
        else if(called == true)
        {
            UpdateHands();
        }
    }

    void CreateHands()
    {
        if ( UDP.ReceiveHandsData() == null ||  UDP.ReceiveHandsData().Count == 0)
        {
            Debug.LogWarning("No hands data received.");
            return;
        }

        foreach (HandData hand in UDP.ReceiveHandsData())
        {
            foreach (HandLandmark landmark in hand.landmarks)
            {
                GameObject newLandMark = Instantiate(landMarkObject, parentTransform, true);

                if(hand.hand_index == 1)
                {
                    leftHand.Add(newLandMark.transform);
                }
                else
                {
                    rightHand.Add(newLandMark.transform);
                }

                newLandMark.transform.localPosition = new Vector3(landmark.x * -1, landmark.y * -1, 0) * scaleFactor;
            }
        }
    }

    void UpdateHands()
    {
        List<HandData> handsData = UDP.ReceiveHandsData();

        if (handsData == null || handsData.Count == 0)
        {
            return;
        }

        foreach (HandData hand in handsData)
        {
            List<Transform> handTransforms;

            if (hand.hand_index == 1)
            {   
                handTransforms = rightHand;
            }
            else
            {
                handTransforms = leftHand;
            }

            for (int i = 0; i < hand.landmarks.Count; i++)
            {
                HandLandmark landmark = hand.landmarks[i];

                Vector3 newPos = new Vector3(landmark.x * -1, landmark.y * -1, 0);

                if((newPos - handTransforms[i].transform.position).magnitude >= thresholdVector.magnitude)
                {
                    Transform tf = handTransforms[i];

                    // Update the local position relative to the parent transform
                    tf.localPosition = Vector3.Lerp(tf.position, newPos * scaleFactor, lerpValue);
                }
            }
        }
    }

}