using UnityEngine;
using System;
using System.Collections.Generic;
using TrackIRUnity;

[Serializable]
public class Limit {
    public float lower, upper;
}

public class TrackIRCamera : MonoBehaviour {
    public bool useGUI;
    TrackIRUnity.TrackIRClient trackIRclient;
    bool running;
    string status, data;
    public Rect statusRect;
    public Rect dataRect;
    public float positionReductionFactor, rotationReductionFactor;
    public Limit positionXLimits, positionYLimits, positionZLimits, yawLimits, pitchLimits, rollLimits;
    public bool useLimits;
    public Camera trackIRCamera;


	// Use this for initialization
	void Start () {
        trackIRclient = new TrackIRUnity.TrackIRClient();  // Create an instance of the TrackerIR Client to get data from
        status = "";
        data = "";
	}

    void StartCamera() {
        if (trackIRclient != null && !running) {                        // Start tracking
            status = trackIRclient.TrackIR_Enhanced_Init();
            running = true;
        }
    }

    void StopCamera() {
        if (trackIRclient != null && running) {                         // Stop tracking
            status = trackIRclient.TrackIR_Shutdown();
            running = false;
        }
    }

    void OnEnable() {
        StartCamera();
    }

    void OnDisable() {
        StopCamera();
    }

    void OnApplicationQuit() {                              // Shutdown the camera when we quit the application.
        StopCamera();
    }

    void OnGUI() {
        if (useGUI) {                                       // Gui for testing
            if (GUI.Button(new Rect(10, 10, 100, 25), "Init")) {
                StartCamera();
            }
            if (GUI.Button(new Rect(10, 45, 100, 25), "Shutdown")) {
                StopCamera();
            }
            GUI.TextArea(statusRect, status);
            GUI.TextArea(dataRect, data);
        }
    }

	// Update is called once per frame
	void Update () {
        if (running) {
            data = trackIRclient.client_TestTrackIRData();          // Data for debugging output, can be removed if not debugging/testing
            TrackIRClient.LPTRACKIRDATA tid = trackIRclient.client_HandleTrackIRData(); // Data for head tracking
            Vector3 pos = trackIRCamera.transform.localPosition;                          // Updates main camera, change to whatever
            Vector3 rot = trackIRCamera.transform.localRotation.eulerAngles;
            if (!useLimits) {
                pos.x = -tid.fNPX * positionReductionFactor;                                        
                pos.y = tid.fNPY * positionReductionFactor;
                pos.z = -tid.fNPZ * positionReductionFactor;

                rot.y = -tid.fNPYaw * rotationReductionFactor;
                rot.x = tid.fNPPitch * rotationReductionFactor;
                rot.z = tid.fNPRoll * rotationReductionFactor; 
            } else {
                pos.x = Mathf.Clamp(-tid.fNPX *- positionReductionFactor, positionXLimits.lower, positionXLimits.upper);
                pos.y = Mathf.Clamp(tid.fNPY * positionReductionFactor, positionYLimits.lower, positionYLimits.upper);
                pos.z = Mathf.Clamp(-tid.fNPZ * positionReductionFactor, positionZLimits.lower, positionZLimits.upper);
                
                rot.y = Mathf.Clamp(-tid.fNPYaw * rotationReductionFactor, yawLimits.lower, yawLimits.upper);
                rot.x = Mathf.Clamp(tid.fNPPitch * rotationReductionFactor, pitchLimits.lower, pitchLimits.upper);
                rot.z = Mathf.Clamp(tid.fNPRoll * rotationReductionFactor, rollLimits.lower, rollLimits.upper);
            }
            
            Camera.main.transform.localRotation = Quaternion.Euler(rot);
            Camera.main.transform.localPosition = pos;
        }
    }
}