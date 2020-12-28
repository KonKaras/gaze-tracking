using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class EyeTracking : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MLEyes.Start();
        StartCoroutine(WriteData());
    }

    void OnDisable()
    {
        MLEyes.Stop();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (MLEyes.IsStarted)
        {
            
        }
    }

    IEnumerator WriteData()
    {
        yield return new WaitForSeconds(.2f);
    }

}
