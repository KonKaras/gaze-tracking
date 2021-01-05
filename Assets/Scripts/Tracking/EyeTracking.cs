using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.XR.MagicLeap;

public class EyeTracking : MonoBehaviour
{
    public bool shouldRecord;
    bool recording;
    bool savingDone;
    string saved ="";
    public string path = "Assets/Scripts/Tracking/tracking.txt";
    public int updatesPerSecond = 5;
    DataPoints dataPoints;

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
        if (!MLEyes.IsStarted)
        {
            MLEyes.Start();
        }
        if (shouldRecord && !recording && MLEyes.IsStarted)
        {
            StartCoroutine("WriteData");
        }
        else
        {
            if (!shouldRecord && !savingDone)
            {
                StopCoroutine("WriteData");
                SaveAsJson();
                List<TrackedPoint> points = LoadFromJson();
                foreach(TrackedPoint p in points)
                {
                    Debug.Log(p.time + ": " + p.pos);
                }
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
        dataPoints.points.Add(point);
        yield return new WaitForSeconds(1/updatesPerSecond);
        saved = JsonUtility.ToJson(dataPoints);
        recording = false;
    }

    void SaveAsJson()
    {
        //save joint settings
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
}
