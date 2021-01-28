using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
    public Gradient colorGradient;
    int maxAttention = 1;

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
            }
        }
    }
    public void UpdateMeshes()
    {
        int attention = 0;
        foreach (MeshColoring key in triangleToTrackedPointsMappingPerMesh.Keys)
        {
            key.UpdateColor();

            //Debug.Log(triangleToTrackedPointsMappingPerMesh[key].Count);
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
