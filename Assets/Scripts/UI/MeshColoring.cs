using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        mesh.colors = new Color[vertices.Length];
        triangles = mesh.triangles;

        attentionPerTriangle = new Dictionary<int, int>();
        associatedPointsPerTriangle = dict;
        foreach (int triangle in associatedPointsPerTriangle.Keys)
        {
            attentionPerTriangle.Add(triangle, 0);
        }
        isInitialized = true;
    }

    public void UpdateColor()
    {
        if (isInitialized)
        {
            int ver = 0;
            Color[] colors = new Color[mesh.colors.Length];
            int attention = 0;
            foreach (int triangle in associatedPointsPerTriangle.Keys)
            {
                attention = 0;
                List<GameObject> trackedPoints = associatedPointsPerTriangle[triangle];
                foreach (GameObject obj in trackedPoints)
                {
                    if (obj.activeSelf)
                    {
                        attention++;
                    }
                }
                attentionPerTriangle[triangle] = attention;

                //Update Maximum Attention
                if (manager.GetMaxAttention() < attention)
                {
                    manager.SetMaxAttention(attention);
                }
            }
            //assigne colors sorted
            foreach (KeyValuePair<int, int> triangle in attentionPerTriangle.OrderBy(key => key.Value).ToDictionary(t => t.Key, t => t.Value))
            {
                float gradientEval = Mathf.InverseLerp(0f, (float)manager.GetMaxAttention(), triangle.Value);
                Debug.Log(gradientEval);
                Color triangleColor = manager.colorGradient.Evaluate(gradientEval);// / (float)manager.GetMaxAttention());
                int[] vertices = VerticesFromTriangle(triangle.Key);

                foreach (int v in vertices)
                {
                    try {
                        colors[v] = triangleColor;
                    ver = v;
                    }catch (System.Exception e) {

                    }
                }
            }
            mesh.colors = colors;
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
