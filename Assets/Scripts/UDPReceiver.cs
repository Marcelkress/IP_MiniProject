using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.UIElements;

public class UDPReceive : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client; 
    public int port = 5052;
    public bool startRecieving = true;
    public bool printToConsole = false;
    public string data;
    public List<HandData> handsData; // List to store the hands data


    public void Awake()
    {
        // Creating new thread
        receiveThread = new Thread(
            new ThreadStart(ReceiveData));

        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    public List<HandData> ReceiveHandsData()
    {
        return handsData;
    }
    
    // receive thread
    private void ReceiveData()
    {
        client = new UdpClient(port);

        while (startRecieving)
        {
            try
            {
                // Receiving data and turning it into a string:
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

                byte[] dataByte = client.Receive(ref anyIP);

                data = Encoding.UTF8.GetString(dataByte);

                // Using a wrapper class because Jsonututility can't serialize directly to an array
                // Use the wrapper class to deserialize the JSON string
                HandDataWrapper wrapper = JsonUtility.FromJson<HandDataWrapper>(data);

                handsData = wrapper.hands;

                if (handsData != null)
                {
                    Debug.Log("Hands data deserialized successfully.");
                    foreach (var hand in handsData)
                    {
                        Debug.Log($"Hand {hand.hand_index}: Number of landmarks: {hand.landmarks.Count}");
                    }
                }
                else
                {
                    Debug.LogWarning("Hands data deserialization returned null.");
                }

                if (printToConsole) 
                { 
                    Debug.Log("Hands data received and deserialized.");
                }
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    /// <summary>
    /// Abort thread when object is disabled
    /// </summary>
    private void OnDisable()
    {
        startRecieving = false;
        receiveThread.Abort();
    }
}

[Serializable]
public class HandLandmark
{
    public float x;
    public float y;
    public float z;
}

[Serializable]
public class HandData
{
    public int hand_index;
    public List<HandLandmark> landmarks;
}

[Serializable]
public class HandDataWrapper
{
   public List<HandData> hands;
}