using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public List<GameObject> points;
    public List<TrackedPoint> pointsData;
    public float maxTime;

    private float minSliderValue = 0;
    private float maxSliderValue = 0;
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
        
        for (int i = 0; i < points.Count; i++)
        {
            if(pointsData[i].time <= selectedTime)
            {
                points[i].SetActive(true);
            }
            else
            {
                points[i].SetActive(false);
            }
        
        }
    }

    public void HandleTimeRange()
    {
        float maxTimeValue = maxSliderValue * maxTime;
        float minTimeValue = minSliderValue * maxTime;

        for (int i = 0; i < points.Count; i++)
        {
            if (pointsData[i].time <= maxTimeValue && pointsData[i].time >= minTimeValue)
            {
                points[i].SetActive(true);
            }
            else
            {
                points[i].SetActive(false);
            }

        }
    }

    public void setMinSliderValue (float value)
    {
        minSliderValue = value;
    }

    public void setMaxSliderValue(float value)
    {
        maxSliderValue = value;
    }
}
