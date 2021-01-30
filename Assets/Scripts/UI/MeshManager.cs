using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshManager : MonoBehaviour
{
    public bool avgOverlappingVertices = true;
    public bool useMedian = true;
    public Gradient colorGradientLowerSpectrum;
    public Gradient colorGradientUpperSpectrum;
    int maxAttention = 0;
    float avgAttention = 0;
    int median = 0;
    int totalAttention = 0;
    int numTriangles = 0;

    int a, b;

    Dictionary<MeshColoring, Dictionary<int, List<GameObject>>> triangleToTrackedPointsMappingPerMesh;
    Dictionary<int, int> attentionPerTriangles;

    public void SetAttentionToTriangleList(int triangle, int attention)
    {
        if (attentionPerTriangles.ContainsKey(triangle))
        {
            attentionPerTriangles[triangle] = attention;
        }
        else
        {
            attentionPerTriangles.Add(triangle, attention);
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
        attentionPerTriangles = new Dictionary<int, int>();
    }

    void UpdateMedian()
    {
        int[] workingArray = attentionPerTriangles.Values.ToArray<int>();
        median = GetMedian(workingArray, workingArray.Length);
    }

    //https://www.geeksforgeeks.org/median-of-an-unsorted-array-in-liner-time-on/
    int GetMedian(int[] workingArray, int n)
    {
        int result;
        a = -1;
        b = -1;

        if (n % 2 == 1)
        {
            MedianHelper(workingArray, 0, n - 1, n / 2);
            result = b;
        }
        else
        {
            MedianHelper(workingArray, 0, n - 1, n / 2);
            result = (a + b) / 2;
        }
        return result;
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
        while(runningIndex < right)
        {
            if(workingArray[runningIndex] < rightValue)
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
        int pivot = workingArray[Random.Range(0, workingArray.Length-1)] % n;
        workingArray = swapValues(workingArray, left + pivot, right);
        return GetIndexOfPivot(workingArray, left, right);
    }

    int MedianHelper(int[] workingArray, int left, int right, int x)
    {
        if(left <= right)
        {
            //get pivot index
            int pivotIndex = randomPartition(workingArray, left, right);
            //median found for odd number of values
            if(pivotIndex == x)
            {
                b = workingArray[pivotIndex];
                if (a != -1) return int.MinValue;
            }else if (pivotIndex == x - 1)
            {
                //a&b is middle of array
                a = workingArray[pivotIndex];
                if (a != -1) return int.MinValue;
            }

            // x <= pivot -> search left
            if(x <= pivotIndex)
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
        
        foreach(MeshColoring child in transform.GetComponentsInChildren<MeshColoring>())
        {
            child.GetComponent<MeshFilter>().mesh.colors = new Color[child.GetComponent<MeshFilter>().mesh.vertices.Length];
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
            }
        }
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
        avgAttention = useMedian ? median : totalAttention / (float)numTriangles;
        //Debug.Log("avg: " + avgAttention);
        foreach(MeshColoring key in triangleToTrackedPointsMappingPerMesh.Keys)
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
