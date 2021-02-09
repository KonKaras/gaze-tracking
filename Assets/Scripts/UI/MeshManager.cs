using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime;

public class MeshManager : MonoBehaviour
{
    public bool avgOverlappingVertices = true;
    public bool useMedian = true;
    //Omit attention under this value
    public int threshold = 0;
    public Gradient colorGradientLowerSpectrum;
    public Gradient colorGradientUpperSpectrum;
    int maxAttention = 0;
    float avgAttention = 0;
    int totalAttention = 0;
    int numTriangles = 0;
    public float samePositionTolerance = 0.0001f;


    int a, b;

    Dictionary<MeshColoring, Dictionary<int, List<GameObject>>> triangleToTrackedPointsMappingPerMesh;
    Dictionary<MeshColoring, Dictionary<int, int>> attentionPerTriangles;

   public Dictionary<MeshColoring, Dictionary<int, Container>> attentionPerVertices;
    public Dictionary<int, Vector3> allVertexPos;

    public void SetAttentionToTriangleList(MeshColoring mesh, int triangle, int attention)
    {
        if (attentionPerTriangles.ContainsKey(mesh))
        {
            if (attentionPerTriangles[mesh].ContainsKey(triangle)) attentionPerTriangles[mesh][triangle] = attention;
            else attentionPerTriangles[mesh].Add(triangle, attention);
        }
        else
        {
            Dictionary<int, int> tmp = new Dictionary<int, int>();
            tmp.Add(triangle, attention);
            attentionPerTriangles.Add(mesh, tmp);
        }
    }

    public void RemoveAttentionFromTriangle(MeshColoring mesh, int triangle)
    {
        if (attentionPerTriangles.ContainsKey(mesh))
        {
            if (attentionPerTriangles[mesh].ContainsKey(triangle))
            {
                //Debug.Log(mesh + " " + triangle + " " + attentionPerTriangles[mesh][triangle]);
                attentionPerTriangles[mesh].Remove(triangle);
                if (attentionPerTriangles[mesh].Keys.Count == 0)
                {
                    attentionPerTriangles.Remove(mesh);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Setup(Dictionary<MeshColoring, Dictionary<int, List<GameObject>>> dict)
    {
        triangleToTrackedPointsMappingPerMesh = dict;
        attentionPerTriangles = new Dictionary<MeshColoring, Dictionary<int, int>>();
      //  allVertexPos = new List<Vector3>();
        attentionPerVertices = new Dictionary<MeshColoring, Dictionary<int, Container>>();


        //SwitchShader();
    }

    void SwitchShader(MeshColoring meshColoring)
    {
        Shader vertexShader = Shader.Find("Shader Graphs/MeshShader");
        meshColoring.gameObject.GetComponent<MeshRenderer>().material.shader = vertexShader;
    }

    float UpdateMedian()
    {
        List<int> attentionValues = new List<int>();
        foreach (MeshColoring mesh in attentionPerTriangles.Keys)
        {
            //attentionValues.AddRange(attentionPerTriangles[mesh].Values);
            foreach (int i in attentionPerTriangles[mesh].Keys)
            {
                attentionValues.Add(attentionPerTriangles[mesh][i]);
            }
        }

        attentionValues.Sort();

        if (attentionValues.Count != 0)
        {
            return attentionValues.Count % 2 == 0 ? 0.5f * (attentionValues[attentionValues.Count / 2] + attentionValues[attentionValues.Count / 2 + 1]) : attentionValues[(attentionValues.Count + 1) / 2];
        }

        return 0;

        //return GetMedian(workingArray, workingArray.Length); ---> way more efficient but stack overflow, needs fixing
    }

    //https://www.geeksforgeeks.org/median-of-an-unsorted-array-in-liner-time-on/
    int GetMedian(int[] workingArray, int n)
    {
        a = -1;
        b = -1;

        if (n % 2 == 1)
        {
            MedianHelper(workingArray, 0, n - 1, (int)n / 2);
            return b;
        }
        else
        {
            MedianHelper(workingArray, 0, n - 1, (int)n / 2);
            return (a + b) / 2;
        }
    }

    int[] swapValues(int[] workingArray, int idx1, int idx2)
    {
        int tmp = workingArray[idx1];
        workingArray[idx1] = workingArray[idx2];
        workingArray[idx2] = tmp;
        return workingArray;
    }

    int GetIndexOfPivot(int[] workingArray, int left, int right)
    {
        int pivotIndex = 1;
        int runningIndex = 1;
        int rightValue = workingArray[right];
        while (runningIndex < right)
        {
            if (workingArray[runningIndex] < rightValue)
            {
                workingArray = swapValues(workingArray, pivotIndex, runningIndex);
                pivotIndex++;
            }
            runningIndex++;
        }
        workingArray = swapValues(workingArray, pivotIndex, right);
        return pivotIndex;
    }

    int randomPartition(int[] workingArray, int left, int right)
    {
        int n = right - left + 1;
        int pivot = new System.Random().Next() % n;//workingArray[Random.Range(0, workingArray.Length-1)] % n;
        workingArray = swapValues(workingArray, left + pivot, right);
        return GetIndexOfPivot(workingArray, left, right);
    }

    int MedianHelper(int[] workingArray, int left, int right, int x)
    {
        if (left <= right)
        {
            //get pivot index
            int pivotIndex = randomPartition(workingArray, left, right);
            //median found for odd number of values
            if (pivotIndex == x)
            {
                b = workingArray[pivotIndex];
                if (a != -1) return int.MinValue;
            }
            else if (pivotIndex == x - 1)
            {
                //a&b is middle of array
                a = workingArray[pivotIndex];
                if (b != -1) return int.MinValue;
            }

            // x <= pivot -> search left
            if (x <= pivotIndex)
            {
                return MedianHelper(workingArray, left, pivotIndex - 1, x);
            }
            else
            {
                return MedianHelper(workingArray, pivotIndex + 1, right, x);
            }
        }
        return int.MinValue;
    }

    public void InitMeshes()
    {

        foreach (MeshColoring child in transform.GetComponentsInChildren<MeshColoring>())
        {
            child.GetComponent<MeshFilter>().mesh.colors = new Color[child.GetComponent<MeshFilter>().mesh.vertices.Length];
            SwitchShader(child);
        }

        int attention = 0;
        foreach (MeshColoring key in triangleToTrackedPointsMappingPerMesh.Keys)
        {
            key.Initialize(triangleToTrackedPointsMappingPerMesh[key]);
            //Debug.Log(triangleToTrackedPointsMappingPerMesh[key].Count);


            foreach (int triangle in triangleToTrackedPointsMappingPerMesh[key].Keys)
            {
                attention = triangleToTrackedPointsMappingPerMesh[key][triangle].Count;
                if (attention > maxAttention) maxAttention = attention;
                numTriangles++;
                //allVertexPos.AddRange(key.getAllPositionsFromTriangle(triangle));



            }
        }
    }

    int GetActiveTriangleCount()
    {
        int num = 0;
        foreach (MeshColoring mesh in attentionPerTriangles.Keys)
        {
            num += attentionPerTriangles[mesh].Keys.Count;
        }
        return num != 0 ? num : 1;
    }

    public void UpdateMeshes()
    {
        maxAttention = 0;
        totalAttention = 0;
        foreach (MeshColoring key in triangleToTrackedPointsMappingPerMesh.Keys)
        {
            key.UpdateAttention();
            totalAttention += key.meshAttention;
            //Debug.Log(triangleToTrackedPointsMappingPerMesh[key].Count);
        }
        //Debug.Log("triangles " + numTriangles);
        //Debug.Log("totalAttention " + totalAttention);
        avgAttention = useMedian ? UpdateMedian() : totalAttention / (float)GetActiveTriangleCount();
        //Debug.Log("avg: " + avgAttention);


        foreach (MeshColoring key in triangleToTrackedPointsMappingPerMesh.Keys)
        {
            

            key.UpdateColor(maxAttention, avgAttention);

        }
    }

    public int GetMaxAttention()
    {
        return maxAttention;
    }

    public void SetMaxAttention(int attention)
    {
        maxAttention = attention;
    }
}

