using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
    public bool avgOverlappingVertices = true;
    public Gradient colorGradientLowerSpectrum;
    public Gradient colorGradientUpperSpectrum;
    int maxAttention = 0;
    float avgAttention = 0;
    int totalAttention = 0;
    int numTriangles = 0;

    Dictionary<MeshColoring, Dictionary<int, List<GameObject>>> triangleToTrackedPointsMappingPerMesh;



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
        avgAttention = totalAttention / (float)numTriangles;
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
