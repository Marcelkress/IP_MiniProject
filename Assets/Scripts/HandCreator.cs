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
    public Color lineColor;

    private LineRenderer lineRenderer;

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
            DrawConnectionsLine();
        }
    }

    void Start()
    {
        // Add a LineRenderer component to the GameObject
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 0;
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

                    // Draw lines
                    //Debug.DrawLine(newPos, handTransforms[i + 1].transform.position, lineColor, 0.1f);
                }
            }
        }
    }
    private void DrawConnections()
    {
        List<HandData> handsData = UDP.ReceiveHandsData();

        foreach(HandData hand in handsData)
        {
            for(int i = 0; i == 5; i++)
            {
                Vector3 beginPos = new Vector3(hand.landmarks[i].x, hand.landmarks[i].y, 0) * scaleFactor;
                Vector3 endPos = new Vector3(hand.landmarks[i+1].x, hand.landmarks[i+1].y, 0) * scaleFactor;

                DrawLine(beginPos, endPos, .1f);
            }   
        }
    }
    private void DrawLine(Vector3 beginPosition, Vector3 endPosition, float duration)
    {
        Debug.DrawLine(beginPosition, endPosition, lineColor, duration);
    }

    private void DrawConnectionsLine()
    {
        List<HandData> handsData = UDP.ReceiveHandsData();

        foreach (HandData hand in handsData)
        {
            List<Transform> handTransforms = hand.hand_index == 1 ? rightHand : leftHand;

            // Set the number of positions in the LineRenderer
            lineRenderer.positionCount = handTransforms.Count;

            for (int i = 0; i < handTransforms.Count - 1; i++)
            {
                Vector3 beginPos = handTransforms[i].localPosition;
                Vector3 endPos = handTransforms[i + 1].localPosition;

                // Set the positions in the LineRenderer
                lineRenderer.SetPosition(i, beginPos);
                lineRenderer.SetPosition(i + 1, endPos);
            }
        }
    }
}