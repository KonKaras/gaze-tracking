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
    public Dictionary<int, List<GameObject>> associatedPointsPerTriangle;


    // Start is called before the first frame update
    void Start()
    {
    }

    public void Initialize(Dictionary<int, List<GameObject>> dict)
    {
        manager = transform.parent.GetComponent<MeshManager>();
        mesh = GetComponent<MeshFilter>().mesh;

        vertices = mesh.vertices;
        colors = mesh.colors;
        triangles = mesh.triangles;

        attentionPerTriangle = new Dictionary<int, int>();
        associatedPointsPerTriangle = dict;
        foreach (int triangle in associatedPointsPerTriangle.Keys)
        {
            attentionPerTriangle.Add(triangle, 0);
        }
        isInitialized = true;
    }

    public void SetDict(Dictionary<int, List<GameObject>> dict)
    {

    }

    public void OnTimerValueChanged()
    {
        
    }

    public void UpdateColor()
    {
        if (isInitialized)
        {
            foreach (int triangle in associatedPointsPerTriangle.Keys)
            {
                int attention = 0;
                List<GameObject> trackedPoints = associatedPointsPerTriangle[triangle];
                foreach (GameObject obj in trackedPoints)
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
                Color triangleColor = manager.colorGradient.Evaluate(attention / manager.threshold);
                int[] vertices = VerticesFromTriangle(triangle);
                foreach (int v in vertices)
                {
                    mesh.colors[v] = triangleColor;
                }
            }
        }
    } 

    int[] VerticesFromTriangle(int triangle)
    {
        return new int[]{
            triangles[triangle * 3 + 0],
            triangles[triangle * 3 + 1],
            triangles[triangle * 3 + 2]};
    }
}
