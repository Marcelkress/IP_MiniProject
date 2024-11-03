using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
    
public class HandIdentify : MonoBehaviour
{
    public UDPReceive UDP;
    public GameObject landMarkObject;
    public int scaleFactor = 15;
    public Transform parentTransform;
    public float waitTime;
    private float timePassed;
    bool called;

    public List<Transform> leftHand;
    public List<Transform> rightHand;

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
            List<Transform> handTransforms = hand.hand_index == 0 ? rightHand : leftHand;

            for (int i = 0; i < hand.landmarks.Count; i++)
            {
                HandLandmark landmark = hand.landmarks[i];
                if (handTransforms != null && handTransforms[i] != null)
                {
                    // Update the local position relative to the parent transform
                    handTransforms[i].localPosition = new Vector3(landmark.x * -1, landmark.y * -1, landmark.z) * scaleFactor;
                }
            }
        }
    }

}