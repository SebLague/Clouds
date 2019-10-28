using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSTest : MonoBehaviour {
    public bool active;
    public GameObject[] disableWhenActive;
    public Transform[] waypoints;
    public float testDuration = 10;
    int index;
    float p;
    public string logName;

    public float avgFPS;

    float elapsedTime;
    int numFrames;
    float startDelay = 1;
    float totalDst;
    Transform cam;

    void Awake () {
        if (active) {
            foreach (var g in disableWhenActive) {
                g.SetActive (false);
            }
            cam = FindObjectOfType<Camera> ().transform;
            for (int i = 0; i < waypoints.Length - 1; i++) {
                totalDst += Vector3.Distance (waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }

    void Update () {
        if (elapsedTime > testDuration) {
            return;
        }
        if (active && Time.timeSinceLevelLoad > startDelay) {
            numFrames++;
            elapsedTime += Time.deltaTime;
            avgFPS = numFrames / elapsedTime;

            float speed = totalDst / testDuration;
            p += Time.deltaTime / Vector3.Distance (waypoints[index].position, waypoints[index + 1].position) * speed;
            cam.position = Vector3.Lerp (waypoints[index].position, waypoints[index + 1].position, p);
            var cR = cam.rotation;
            cam.LookAt (waypoints[index + 1]);
            cam.rotation = Quaternion.Slerp (cR, cam.rotation, Time.deltaTime);
            if (p >= 1 && index < waypoints.Length - 1) {
                index++;
                p = 0;

            }
        }
        if (elapsedTime > testDuration) {
            Debug.Log(logName + ": " + avgFPS);
        }
    }

    void OnDrawGizmos () {
        for (int i = 0; i < waypoints.Length; i++) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere (waypoints[i].position, 5);
            if (i < waypoints.Length - 1) {
                Gizmos.color = Color.black;
                Gizmos.DrawLine (waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
}