using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI : MonoBehaviour
{
    public TextMeshProUGUI text;
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
}
