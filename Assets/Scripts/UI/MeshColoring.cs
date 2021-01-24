using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//assing to reconstruction mesh prefab!
public class MeshColoring : MonoBehaviour
{
    Vector3[] vertices;
    Color[] colors;
    int[] triangles;

    MeshManager manager;

    Mesh mesh;
    bool isInitialized = false;
    Dictionary<int, int> attentionPerTriangle;
    Dictionary<int, List<GameObject>> associatedPointsPerTriangle;


    // Start is called before the first frame update
    void Start()
    {
    }

    public void Initialize()
    {
        manager = transform.parent.GetComponent<MeshManager>();
        mesh = GetComponent<MeshFilter>().mesh;

        vertices = mesh.vertices;
        colors = mesh.colors;
        triangles = mesh.triangles;

        attentionPerTriangle = new Dictionary<int, int>();
        associatedPointsPerTriangle = new Dictionary<int, List<GameObject>>();

        isInitialized = true;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (isInitialized)
        {
            UpdateColor();
        }
    }

    public void OnTimerValueChanged()
    {
        
    }

    void UpdateColor()
    {
        foreach(int triangle in associatedPointsPerTriangle.Keys)
        {
            int attention = 0;
            List<GameObject> trackedPoints = associatedPointsPerTriangle[triangle];
            foreach(GameObject obj in trackedPoints)
            {
                if (obj.activeSelf)
                {
                    attention++;
                }
                else
                {
                    attention--;
                }
            }
            attentionPerTriangle[triangle] = attention;
            manager.colorGradient.Evaluate(attention / manager.threshold);
        }
    } 
}
