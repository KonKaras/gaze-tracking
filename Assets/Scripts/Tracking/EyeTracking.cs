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
    public float neighborRadius;
    public float neighborPointWeight;

    public Color least;
    public Color middleLower;
    public Color middle;
    public Color middleUpper;
    public Color most;

    List<GameObject> spawnedPoints;
    MeshManager meshManager;
    Dictionary<MeshColoring, Dictionary<int, List<GameObject>>> triangleToTrackedPointsMappingPerMesh;

    Shader shader;
    MLInput.Controller controller;

    UI ui;

    // Start is called before the first frame update
    void Start()
    {
        ui = GameObject.Find("Canvas").GetComponent<UI>();
        shader = Shader.Find("Standard");
        spawnedPoints = new List<GameObject>();
        particleSystem = GetComponent<ParticleSystem>();
        dataPoints = new DataPoints();
        meshManager = GameObject.Find("Reconstruction").GetComponent<MeshManager>();
        triangleToTrackedPointsMappingPerMesh = new Dictionary<MeshColoring, Dictionary<int, List<GameObject>>>();


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
            Vector3 normal = Vector3.zero;
            GameObject obj = Instantiate(pointIndicator, point.pos, Quaternion.identity);

            MeshCorrected(point, ref normal, obj);
            meshManager.Setup(triangleToTrackedPointsMappingPerMesh);
            meshManager.InitMeshes();

            //obj.GetComponent<TrackedPoint>().time = point.time;
            obj.transform.rotation = Quaternion.FromToRotation(obj.transform.forward, normal);
            obj.transform.Translate(obj.transform.forward.normalized * 1 / 10);

            Color color = ComputeColor(ComputeWeight(points, point));
            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            
            float alpha = renderer.color.a;
            renderer.color = new Color(color.r, color.g, color.b, renderer.color.a);
            //Material newMat = renderer.material;
            //float alpha = newMat.color.a;
            //newMat.color = new Color(color.r, color.g, color.b, alpha);

            //renderer.material = newMat;
            Debug.Log(obj);
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
        /*
        Color start = least;
        Color end = middleLower;

        if (weight > .75f)
        {
            start = middleUpper;
            end = most;
        }else if(weight > .5f)
        {
            start = middle;
            end = middleUpper;
        }else if(weight > .25f)
        {
            start = middleLower;
            end = middle;
        }

        return Color.Lerp(start, end, weight);
        */
        if (weight > .8f) return most;
        else if (weight > .6f) return middleUpper;
        else if (weight > .4f) return middle;
        else if (weight > .2f) return middleLower;
        return least;
    }

    float ComputeWeight(List<TrackedPoint> points, TrackedPoint toBeColored)
    {
        float weight = 0;
        foreach(TrackedPoint compare in points)
        {
            float dist = (compare.pos - toBeColored.pos).magnitude;
            if (compare != toBeColored && dist <= neighborRadius){
                weight += ((neighborRadius - dist) / neighborRadius) * neighborPointWeight;
            }
        }
        return weight;
    }

    void MeshCorrected(TrackedPoint point, ref Vector3 normal, GameObject spawnedPoint)
    {
        //get view direction
        Vector3 dir = point.pos - point.camPos;

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, dir, out hit, Mathf.Infinity))
        {
            string objName = hit.transform.gameObject.name;
            int triangle = hit.triangleIndex;
            MeshColoring mcoloring = hit.transform.GetComponent<MeshColoring>();
            if (mcoloring != null)
            {
                if (triangleToTrackedPointsMappingPerMesh.ContainsKey(hit.transform.GetComponent<MeshColoring>()))
                {
                    if (triangleToTrackedPointsMappingPerMesh[mcoloring].ContainsKey(triangle))
                    {
                        //add point to hit triangle of mesh
                        triangleToTrackedPointsMappingPerMesh[mcoloring][triangle].Add(spawnedPoint);
                    }
                    else
                    {
                        //add triangle to mesh and add point
                        List<GameObject> trackedPoints = new List<GameObject>();
                        trackedPoints.Add(spawnedPoint);
                        triangleToTrackedPointsMappingPerMesh[mcoloring].Add(triangle, trackedPoints);
                    }
                }
                else
                {
                    //add new mesh to dict and first triangle entry
                    List<GameObject> trackedPoints = new List<GameObject>();
                    trackedPoints.Add(spawnedPoint);
                    Dictionary<int, List<GameObject>> newTriangleDict = new Dictionary<int, List<GameObject>>();
                    newTriangleDict.Add(triangle, trackedPoints);
                    triangleToTrackedPointsMappingPerMesh.Add(mcoloring, newTriangleDict);
                }
                spawnedPoint.transform.position = hit.point;
                normal = hit.normal;
            }
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
