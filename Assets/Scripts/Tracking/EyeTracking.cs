using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.XR.MagicLeap;

public class EyeTracking : MonoBehaviour
{
    public Camera cam;
    public UnityEngine.UI.Slider slider;
    public bool shouldRecord;
    bool recording;
    bool startedRecording;
    bool savingDone;
    string saved ="";
    public string path = "Assets/Scripts/Tracking/tracking.txt";
    public int updatesPerSecond = 5;
    DataPoints dataPoints;
    public bool showLiveGaze;
    public GameObject pointIndicator;
    ParticleSystem particleSystem;
    //calculate color based on tracked points in this radius
    public float neighborRadius = .5f;
    public float neighborPointWeight = 0.05f;

    List<GameObject> spawnedPoints;

    Shader shader;
    MLInput.Controller controller;

    UI ui;

    // Start is called before the first frame update
    void Start()
    {
        UI ui = GameObject.Find("Canvas").GetComponent<UI>();
        shader = Shader.Find("Standard");
        spawnedPoints = new List<GameObject>();
        particleSystem = GetComponent<ParticleSystem>();
        dataPoints = new DataPoints();

        MLInput.Start();
        MLInput.OnControllerButtonDown += OnButtonDown;
        controller = MLInput.GetController(MLInput.Hand.Left);
    }

    void OnButtonDown(byte controller, MLInput.Controller.Button button)
    {
        if (button == MLInput.Controller.Button.Bumper)
        {
            if (!startedRecording)
            {
                if (!MLEyes.IsStarted)
                {
                    MLEyes.Start();
                }
                Debug.Log("Recording Started");
                shouldRecord = true;
                startedRecording = true;
                ui.SetRecordingText(startedRecording);
            }
            else
            {
                StopCoroutine("WriteData");
                MLEyes.Stop();
                SaveAsJson();
                Debug.Log("Recording Stopped, Data Saved");
                List<TrackedPoint> points = LoadFromJson();
                ShowPoints(points);
                startedRecording = false;
                ui.SetRecordingText(startedRecording);
            }
        }
    }

    void OnDisable()
    {
        MLEyes.Stop();
        MLInput.Stop();
        MLInput.OnControllerButtonDown -= OnButtonDown;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //for debugging, delete later
        if (Input.GetKeyDown(KeyCode.A))
        {
            ShowPoints(LoadFromJson());
        }

        
        if (shouldRecord && !recording)
        {
            if(MLEyes.IsStarted && !MLEyes.LeftEye.IsBlinking && !MLEyes.RightEye.IsBlinking)
            StartCoroutine("WriteData");
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
        //TODO: replace with particle system or computeshader
        foreach (TrackedPoint point in points) 
        {
            MeshCorrected(point);
            GameObject obj = Instantiate(pointIndicator, point.pos, Quaternion.identity);
            //obj.GetComponent<TrackedPoint>().time = point.time;

            Color color = ComputeColor(ComputeWeight(points, point));
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();

            Material newMat = renderer.material;
            float alpha = newMat.color.a;
            newMat.color = new Color(color.r, color.g, color.b, alpha);

            renderer.material = newMat;

            ui.points.Add(obj);
        }

        ui.maxTime = points[points.Count - 1].time;
        ui.pointsData = points;

        slider.gameObject.SetActive(true);
        //SpawnParticles(points);
    }

    void SpawnParticles(List<TrackedPoint> points)
    {
        Debug.Log(particleSystem.particleCount);
        List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle>();
        foreach (TrackedPoint point in points)
        {
            ParticleSystem.Particle particle = new ParticleSystem.Particle();
            particle.position = point.pos;
            particle.startColor = ComputeColor(ComputeWeight(points, point));
            particle.velocity = Vector3.zero;
            particles.Add(particle);
        }

        particleSystem.SetParticles(particles.ToArray(), particles.Count);
        particleSystem.Play();
        Debug.Log(particleSystem.particleCount);
    }

    Color ComputeColor(float weight)
    {
        Color start = Color.blue;
        Color end = Color.green;
        if (weight > .5f)
        {
            start = Color.green;
            end = Color.red;
        }
         return Color.Lerp(start, end, weight);
    }

    float ComputeWeight(List<TrackedPoint> points, TrackedPoint toBeColored)
    {
        float weight = 0;
        foreach(TrackedPoint compare in points)
        {
            float dist = (compare.pos - toBeColored.pos).magnitude;
            if (compare != toBeColored && dist <= neighborRadius){
                weight += (1 - (neighborRadius - dist) / neighborRadius) * neighborPointWeight;
            }
        }
        return weight;
    }

    void MeshCorrected(TrackedPoint point)
    {
        //get view direction
        Vector3 dir = point.pos - point.camPos;

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, dir, out hit, Mathf.Infinity))
        {
            point.pos = hit.point;
            //Debug.DrawRay(point.camPos, dir, Color.green);
            //Debug.Log("Matched!");
        }
        else
        {
            Debug.Log("Nothing hit!");
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
