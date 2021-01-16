using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class FingerTracking : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MLHandTracking.Start();
    }

    private void OnDisable()
    {
        MLHandTracking.Stop();
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
