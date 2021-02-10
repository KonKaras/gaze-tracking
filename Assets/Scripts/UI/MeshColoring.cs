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
    //Dictionary<int, List<Container>> samePosVertices;
    //Dictionary<int, float> attentionPerVertex;
    public int meshAttention = 0;


    // Start is called before the first frame update
    void Start()
    {
    }

   /*
    public void InitSameVertices()
    {
        //for (int i = 0; i < vertices.Length; i++)
        foreach (int triangle in attentionPerTriangle.Keys)
        {
            foreach (int vertex in VerticesFromTriangle(triangle))
            {
                List<Container> sameVerticesInfo = new List<Container>();
                foreach (MeshColoring meshcoloring in manager.vertexInfo.Keys)
                {
                    foreach (int j in manager.vertexInfo[meshcoloring].Keys)
                    {
                        if (meshcoloring == this && vertex == j) continue;
                        if ((vertices[vertex] - manager.vertexInfo[meshcoloring][j].position).magnitude <= manager.samePositionTolerance)
                        {
                            sameVerticesInfo.Add(manager.vertexInfo[meshcoloring][j]);
                        }
                    }
                }
            }
        }
    }
   */

    public void Initialize(Dictionary<int, List<GameObject>> dict)
    {
        manager = transform.parent.GetComponent<MeshManager>();
        mesh = GetComponent<MeshFilter>().mesh;

        vertices = mesh.vertices;
        mesh.colors = new Color[vertices.Length];
        for(int i = 0; i<mesh.colors.Length; i++)// in mesh.colors)
        {
                mesh.vertices[i] = Vector3.one;
        }
        triangles = mesh.triangles;

        attentionPerTriangle = new Dictionary<int, int>();
        associatedPointsPerTriangle = dict;
        
        foreach (int triangle in associatedPointsPerTriangle.Keys)
        {
            attentionPerTriangle.Add(triangle, -1);
            if (manager.avgOverlappingVertices)
            {
                foreach (int i in VerticesFromTriangle(triangle))
                {
                    if (!manager.vertexInfo.ContainsKey(this))
                    {
                        Container info = new Container(this, 0, vertices[i], new List<Container>());
                        Dictionary<int, Container> sameVertexInfo = new Dictionary<int, Container>();
                        sameVertexInfo.Add(i, info);
                        manager.vertexInfo.Add(this, sameVertexInfo);
                    }
                    else
                    {
                        if (!manager.vertexInfo[this].ContainsKey(i))
                        {
                            Container info = new Container(this, 0, vertices[i], new List<Container>());
                            manager.vertexInfo[this].Add(i, info);
                        }
                    }
                }
            }
        }

        
        isInitialized = true;
    }

    void EnableSameVertices(int triangle, bool active)
    {
        foreach (int vertex in VerticesFromTriangle(triangle))
        {
            manager.vertexInfo[this][vertex].active = active;
        }
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
                    if (manager.avgOverlappingVertices)
                    {
                        EnableSameVertices(triangle, obj.activeSelf);
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
                        manager.vertexInfo[this][v].attention = value;
                    
                        /*
                        if (!manager.vertexInfo.ContainsKey(this))
                        {
                            //  attentionPerVertex.Add(v, value);
                            Dictionary<int, Container> infoOfVertex = new Dictionary<int, Container>();
                            infoOfVertex.Add( v, new Container(this, value, vertices[v]));
                            manager.vertexInfo.Add(this, infoOfVertex);
                        }
                        else
                        {
                            if (manager.vertexInfo[this].ContainsKey(v))
                            {
                                manager.vertexInfo[this][v].position = vertices[v];
                                manager.vertexInfo[this][v].attention = value;
                                manager.vertexInfo[this][v].meshColoring = this;
                            }
                            else
                            {
                                
                                manager.vertexInfo[this].Add(v, new Container(this,value,vertices[v]));
                            }
                        }
                        */
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
                        float value = manager.vertexInfo[this][v].attention;//attentionPerVertex[v];
                        int num = 0;
                        foreach (Container sameV in manager.vertexInfo[this][v].sameVerticesInfo)
                        {
                                if (sameV.active)
                                {
                                    Debug.Log("same Pos vertices contains key, value/key" + value + " " + v);
                                    value += sameV.attention;
                                    num++;
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
