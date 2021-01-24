using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
    public Gradient colorGradient;
    public float threshold;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitMeshes()
    {
        foreach(MeshColoring child in transform.GetComponentsInChildren<MeshColoring>())
        {
            child.Initialize();
        }
    }
}
