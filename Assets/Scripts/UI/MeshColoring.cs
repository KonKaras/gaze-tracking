using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//assing to reconstruction mesh prefab!
public class MeshColoring : MonoBehaviour
{
    Vector3[] vertices;
    //Color[] colors;
    int[] triangles;

    MeshManager manager;

    Mesh mesh;
    bool isInitialized = false;
    public Dictionary<int, int> attentionPerTriangle;
    public Dictionary<int, List<GameObject>> associatedPointsPerTriangle;
    public int meshAttention = 0;


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

    public void UpdateAttention()
    {
        if (isInitialized)
        {
            meshAttention = 0;
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
                meshAttention += attention;

                //Update Maximum Attention
                if (manager.GetMaxAttention() < attention)
                {
                    manager.SetMaxAttention(attention);
                }
            }
        }
    }

    //assigne colors sorted
    public void UpdateColor(int maxAttention, float avgAttention)
    {
        if (isInitialized)
        {
            Color[] colors = new Color[mesh.colors.Length];
            foreach (KeyValuePair<int, int> triangle in attentionPerTriangle.OrderBy(key => key.Value).ToDictionary(t => t.Key, t => t.Value))
            //foreach (int triangle in attentionPerTriangle.Keys)
            {
                float start = 0;
                float end = avgAttention;
                float value = triangle.Value;
                Gradient colorGradient = manager.colorGradientLowerSpectrum;
                if (value > avgAttention)
                {
                    start = avgAttention;
                    end = maxAttention;
                    colorGradient = manager.colorGradientUpperSpectrum;
                }

                float gradientEval = Mathf.InverseLerp(start, end, value);

                //Debug.Log(gradientEval);
                Color triangleColor = colorGradient.Evaluate(gradientEval);// / (float)manager.GetMaxAttention());
                int[] vertices = VerticesFromTriangle(triangle.Key);

                foreach (int v in vertices)
                {
                    colors[v] = triangleColor;
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
