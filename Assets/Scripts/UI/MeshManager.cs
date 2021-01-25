using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
    public Gradient colorGradient;
    public float threshold;

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
        /*
        foreach(MeshColoring child in transform.GetComponentsInChildren<MeshColoring>())
        {
            child.Initialize();
        }
        */
        foreach(MeshColoring key in triangleToTrackedPointsMappingPerMesh.Keys)
        {
            key.Initialize(triangleToTrackedPointsMappingPerMesh[key]);
            //Debug.Log(triangleToTrackedPointsMappingPerMesh[key].Count);
        }
    }
    public void UpdateMeshes()
    {
        Debug.Log("Update");
        foreach (MeshColoring key in triangleToTrackedPointsMappingPerMesh.Keys)
        {
            key.UpdateColor();
            //Debug.Log(triangleToTrackedPointsMappingPerMesh[key].Count);
        }
    }
}
