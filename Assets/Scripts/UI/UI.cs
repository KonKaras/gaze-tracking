using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public List<GameObject> points;
    public List<TrackedPoint> pointsData;
    public float maxTime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        //HandleTime(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleTime(float value)
    {
        float selectedTime = value * maxTime;
        Debug.Log(value);
        
        for (int i = 0; i < points.Count; ++i)
        {
            if(pointsData[i].time <= selectedTime)
            {
                //Debug.Log(pointsData[i].time);
                points[i].SetActive(true);
            }
            else
            {
                Debug.Log(pointsData[i].time);
                points[i].SetActive(false);
            }
        
        }
    }
}
