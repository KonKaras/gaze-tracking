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
    Dictionary<int, List<int>> samePosVertices;
    //Dictionary<int, float> attentionPerVertex;
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
            attentionPerTriangle.Add(triangle, -1);
        }
        

        samePosVertices = new Dictionary<int, List<int>>();
       // attentionPerVertex = new Dictionary<int, float>();

        for (int i = 0; i < vertices.Length; i++)
        {
            List<int> sameVertices = new List<int>();
            foreach (MeshColoring meshcoloring in manager.attentionPerVertices.Keys)
            {


                foreach (int j in manager.attentionPerVertices[meshcoloring].Keys)
                {
                  //  if (i == j) continue;
                    if ((vertices[i] - manager.attentionPerVertices[meshcoloring][j].position).magnitude <= manager.samePositionTolerance)
                    {
                        sameVertices.Add(j);
                    }
                }
                if (samePosVertices.ContainsKey(i))
                {
                    samePosVertices[i] = sameVertices;
                }
                else
                {
                    samePosVertices.Add(i, sameVertices);
                }
                //Debug.Log(samePosVertices[i].Count);
            }
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
                if (attention > manager.threshold)
                {
                    manager.SetAttentionToTriangleList(this, triangle, attention);
                    meshAttention += attention;
                }
                else manager.RemoveAttentionFromTriangle(this, triangle);
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
                float start = manager.threshold > avgAttention ? 0 : manager.threshold;
                float end = avgAttention;
                float value = triangle.Value;
                Gradient colorGradient = manager.colorGradientLowerSpectrum;
                
                if (value > avgAttention)
                {
                    //Debug.Log("Upper: " +value);
                    start = avgAttention;
                    end = maxAttention;
                    colorGradient = manager.colorGradientUpperSpectrum;
                }

                float gradientEval = Mathf.InverseLerp(start, end, value);
                int[] triangleVertices = VerticesFromTriangle(triangle.Key);
               
                Color triangleColor = colorGradient.Evaluate(gradientEval);
                foreach (int v in triangleVertices)
                {
                    if (manager.avgOverlappingVertices)
                    {
                        if (!manager.attentionPerVertices.ContainsKey(this))
                        {
                            //  attentionPerVertex.Add(v, value);
                            Dictionary<int, Container> infoOfVertex = new Dictionary<int, Container>();
                            infoOfVertex.Add( v, new Container(value, vertices[v]) );
                            manager.attentionPerVertices.Add(this, infoOfVertex);
                        }
                        else
                        {
                            if (manager.attentionPerVertices[this].ContainsKey(v))
                            {
                                manager.attentionPerVertices[this][v].position = vertices[v];
                                manager.attentionPerVertices[this][v].attention = value;
                            }
                            else
                            {
                                
                                manager.attentionPerVertices[this].Add(v, new Container(value,vertices[v]));
                            }
                        //    attentionPerVertex[v] = value;
                        }
                    }
                    else
                    {
                        colors[v] = triangleColor;

                    }
                }
            }
            if (manager.avgOverlappingVertices) {
                foreach (int triangle in attentionPerTriangle.Keys)
                {
                    //Debug.Log(gradientEval);
                    //Color triangleColor = colorGradient.Evaluate(gradientEval);// / (float)manager.GetMaxAttention());
                    int[] triangleVertices = VerticesFromTriangle(triangle);

                    foreach (int v in triangleVertices)
                    {
                        float value = manager.attentionPerVertices[this][v].attention;//attentionPerVertex[v];
                        int num = 0;
                        if (samePosVertices.ContainsKey(v))
                        {
                            Debug.Log("same Pos vertices contains key, value/key"+value+" "+v);
                            foreach (int sameV in samePosVertices[v])
                            {
                                if (ContainsKey(sameV))
                                {
                                    value += attentionPerVertex[sameV];
                                    num++;
                                }
                            }
                        }
                        //float scaledAvgAttentionToGradientRange = Mathf.InverseLerp(manager.threshold, manager.GetMaxAttention(),avgAttention);
                        float avgValue = value / ((num == 0) ? 1 : num);

                        Gradient colorGradient = manager.colorGradientLowerSpectrum;

                        float start = manager.threshold > avgAttention ? 0 : manager.threshold;
                        float end = avgAttention;

                        if (avgValue > avgAttention)//scaledAvgAttentionToGradientRange)
                        {
                            //Debug.Log("Upper: " +value);
                            start = avgAttention;
                            end = maxAttention;
                            colorGradient = manager.colorGradientUpperSpectrum;
                        }

                        avgValue = Mathf.InverseLerp(start, end, avgValue);
                        colors[v] = colorGradient.Evaluate(avgValue);
                    }
                }
            }
            mesh.colors = colors;
        }
    }

     public int[] VerticesFromTriangle(int triangle)
    {
        return new int[]{
            triangles[triangle * 3 + 0],
            triangles[triangle * 3 + 1],
            triangles[triangle * 3 + 2]};
    }

    public Vector3 [] getAllPositionsFromTriangle(int triangle)
    {
        Vector3[] result = new Vector3[3];
        int index = 0;
        foreach (int i in VerticesFromTriangle(triangle))
        {
            result[index] = vertices[i];
            index++;
        }
        return result;
    }

}
