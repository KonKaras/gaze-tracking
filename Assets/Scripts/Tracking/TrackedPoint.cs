using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TrackedPoint
{
    public Vector3 pos;
    public Vector3 camPos;
    public float time;
}

[Serializable]
public class DataPoints
{
    public List<TrackedPoint> points;
    public DataPoints()
    {
        points = new List<TrackedPoint>();
    }
}
