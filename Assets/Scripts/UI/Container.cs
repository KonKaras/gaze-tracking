using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container
{

    public float attention;
    public Vector3 position;
    public MeshColoring meshColoring;
    public bool active = false;
    public List<Container> sameVerticesInfo;
    public Container(MeshColoring m_coloring, float attention, Vector3 pos, List<Container> same)
    {
        this.meshColoring = m_coloring;
        this.attention = attention;
        this.position = pos;
        this.sameVerticesInfo = same;
    }

}
