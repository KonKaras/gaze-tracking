using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI : MonoBehaviour
{
    public bool showPoints = false;
    public TextMeshProUGUI text;
    public List<GameObject> points;
    public List<TrackedPoint> pointsData;
    public float maxTime;
    public float minTime;
    public MLRangeSlider slider;
    public TextMeshProUGUI currentTime;
    public MeshManager meshManager;

    private float minSliderValue = 0;
    private float maxSliderValue = 0;


    // Start is called before the first frame update
    void Start()
    {
        points = new List<GameObject>();
    }

    private void OnEnable()
    {
        //HandleTime(1);
    }

    public void SetRecordingText(bool isRecording)
    {
        if (isRecording) text.text = "Recording...";
        else text.text = "Tap Left Bumper to Record";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleTime(float value)
    {
        //commented out for compliation when using rangeTimeslider

       // slider.maxSliderObj = value;
        //slider.value = Mathf.Clamp(slider.value, 0f, 1f);
        float selectedTime = value * (maxTime-minTime);
        currentTime.text = "Time: " + System.Math.Round(selectedTime, 2) + "s";

        for (int i = 0; i < pointsData.Count; i++)
        {
            if (pointsData[i].time <= selectedTime)
            {
                points[i].SetActive(true);
                points[i].GetComponent<SpriteRenderer>().enabled = showPoints;
            }
            else
            {
                points[i].GetComponent<SpriteRenderer>().enabled = showPoints;
                points[i].SetActive(false);
            }
        }
        meshManager.UpdateMeshes();
    }

    public void HandleTimeRange()
    {
        float maxTimeValue = maxSliderValue * (maxTime-minTime);
        float minTimeValue = minSliderValue * (maxTime-minTime);
        float selectedMaxTime = slider.maxSliderObj.value* (maxTime-minTime);
        float selectedMinTime = slider.minSliderObj.value * (maxTime-minTime);
        currentTime.text = "Begin: " + System.Math.Round(selectedMinTime, 2) + "s"+ " End: " + System.Math.Round(selectedMaxTime, 2) + "s";
        for (int i = 0; i < points.Count; i++)
        {
            if (pointsData[i].time <= maxTimeValue && pointsData[i].time >= minTimeValue)
            {
                points[i].SetActive(true);
                points[i].GetComponent<SpriteRenderer>().enabled = showPoints;
            }
            else
            {
                points[i].GetComponent<SpriteRenderer>().enabled = showPoints;
                points[i].SetActive(false);
            }

        }
        meshManager.UpdateMeshes();
    }

    public void setMinSliderValue(float value)
    {
        minSliderValue = value;
    }

    public void setMaxSliderValue(float value)
    {
        maxSliderValue = value;
    }

}
