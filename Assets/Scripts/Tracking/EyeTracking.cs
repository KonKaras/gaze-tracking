﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.XR.MagicLeap;

public class EyeTracking : MonoBehaviour
{
    public Camera cam;
    public bool shouldRecord;
    bool recording;
    bool savingDone;
    string saved ="";
    public string path = "Assets/Scripts/Tracking/tracking.txt";
    public int updatesPerSecond = 5;
    DataPoints dataPoints;
    public GameObject pointIndicator;

    // Start is called before the first frame update
    void Start()
    {
        dataPoints = new DataPoints();
        if (shouldRecord)
        {
           //MLEyes.Start();
        }
    }

    void OnDisable()
    {
        MLEyes.Stop();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ShowPoints(LoadFromJson());
        }

        if (shouldRecord && !recording)
        {
            if (!MLEyes.IsStarted)
            {
                MLEyes.Start();
            }
            StartCoroutine("WriteData");
        }
        else
        {
            if (!shouldRecord && !savingDone)
            {
                StopCoroutine("WriteData");
                SaveAsJson();
                List<TrackedPoint> points = LoadFromJson();
                ShowPoints(points);
            }
        }
    }

    IEnumerator WriteData()
    {
        savingDone = false;
        recording = true;
        TrackedPoint point = new TrackedPoint();
        point.pos = MLEyes.FixationPoint;//new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), Random.Range(-2f, 2f));
        point.time = Time.time;
        point.camPos = cam.transform.position;
        dataPoints.points.Add(point);
        yield return new WaitForSeconds(1/updatesPerSecond);
        saved = JsonUtility.ToJson(dataPoints);
        recording = false;
    }

    void SaveAsJson()
    {
        string values = "";
        foreach (TrackedPoint pt in dataPoints.points)
        {
            values += JsonUtility.ToJson(pt) + "\n";
        }
        values = values.Substring(0, values.Length - 1);

        savingDone = true;

        File.WriteAllText(path, values);
        AssetDatabase.ImportAsset(path);
    }

    List<TrackedPoint> LoadFromJson()
    {
        StreamReader reader = new StreamReader(path);
        string savedPoints = reader.ReadToEnd();

        List<TrackedPoint> recoveredPoints = new List<TrackedPoint>();
        string[] trackingEntries = savedPoints.Split('\n');
        foreach(string entry in trackingEntries)
        {
            TrackedPoint pt = JsonUtility.FromJson<TrackedPoint>(entry);
            recoveredPoints.Add(pt);
        }

        return recoveredPoints;
    }

    void ShowPoints(List<TrackedPoint> points)
    {
        foreach (TrackedPoint point in points) 
        {
            MeshCorrected(point);
            GameObject.Instantiate(pointIndicator, point.pos, Quaternion.identity);
        }
    }

    void MeshCorrected(TrackedPoint point)
    {
        //get view direction
        Vector3 dir = point.pos - point.camPos;

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, dir, out hit, Mathf.Infinity))
        {
            point.pos = hit.point;
        }
        else
        {
            Debug.Log("Nothing hit");
        }
        /*
        //try to catch points outside room
        else if(Physics.Raycast(cam.transform.position, -dir, out hit, Mathf.Infinity))
        {
            point.pos = hit.point;
        }
        */
    }
}
