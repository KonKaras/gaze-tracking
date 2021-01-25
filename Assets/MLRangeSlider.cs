﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

public class MLRangeSlider : MonoBehaviour
{


    public Text ui_text;
    public Camera Camera;
    public MinimumSlider minSliderObj;
    public MaximumSlider maxSliderObj;
   
    private MLInput.Controller _controller;
    private enum selectedHandle
    {
        minHandle,
        maxHandle
    }

    private selectedHandle Handle;
    

   
    void Start()
    {
        MLInput.Start();
        _controller = MLInput.GetController(MLInput.Hand.Left);

        Handle = selectedHandle.maxHandle;

        MLInput.OnControllerTouchpadGestureEnd += MLInput_OnControllerTouchpadGestureEnd;
        
    }

   


    //this function does not work i do not know why
    private void MLInput_OnControllerTouchpadGestureEnd(byte controllerId, MLInput.Controller.TouchpadGesture touchpadGesture)
    {

       if (touchpadGesture.Type ==MLInput.Controller.TouchpadGesture.GestureType.ForceTapUp)
        {
            Debug.Log("Touch Tap recognized");
            Debug.Log(touchpadGesture.Type.ToString()) ;
            //toggle Handle
            if (Handle == selectedHandle.minHandle)
            {
                Handle = selectedHandle.maxHandle;
                ui_text.text = "max Handle selected";
            }
            else
            {
                Handle = selectedHandle.minHandle;
                ui_text.text = "min Handle selected";
            }

            Debug.Log("Toggled Handle");
        }

    }
    void OnDestroy()
    {
        MLInput.Stop();
        MLInput.OnControllerTouchpadGestureEnd -= MLInput_OnControllerTouchpadGestureEnd;
    }

    void OnDisable()
    {
        
        MLInput.Stop();
        MLInput.OnControllerTouchpadGestureEnd -= MLInput_OnControllerTouchpadGestureEnd;
    }

    void Update()
    {
        updateTransform();
        updateGestureText();
        updateSlider();
    //    updateSelectedSlider();
    }
    

   //just for debugging
    void updateGestureText()
    {
        string gestureType = _controller.CurrentTouchpadGesture.Type.ToString();
        string gestureState = _controller.TouchpadGestureState.ToString();
        string gestureDirection = _controller.CurrentTouchpadGesture.Direction.ToString();


     //   Debug.Log("Type: " + gestureType);
      //  Debug.Log("State: " + gestureState);
      //  Debug.Log("Direction: " + gestureDirection);

    }


    // used for softlocking the ui
    void updateTransform()
    {
        float speed = Time.deltaTime * 5.0f;

        Vector3 pos = Camera.transform.position + Camera.transform.forward;
        gameObject.transform.position = Vector3.SlerpUnclamped(gameObject.transform.position, pos, speed);

        Quaternion rot = Quaternion.LookRotation(gameObject.transform.position - Camera.transform.position);
        gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, rot, speed);
    }

    void updateSlider()
    {
        var inputType = _controller.CurrentTouchpadGesture.Type;
        float inputAngle = 0;
        if (inputType == MLInput.Controller.TouchpadGesture.GestureType.RadialScroll)
        {
          //  Debug.Log("RadialScroll detected");
            inputAngle = _controller.CurrentTouchpadGesture.Angle;

            float value = inputAngle/(float)3.6;

            if (Handle.Equals(selectedHandle.minHandle))
            {
                minSliderObj.value = value;
            //    Debug.Log("Changed minSLider value to: " + value);
            }

            if (Handle == selectedHandle.maxHandle)
            {
                maxSliderObj.value = value;
            //    Debug.Log("Changed maxSLider value to: " + value);
            }
        }

       


      
    }

    void updateSelectedSlider()
    {
         var inputType = _controller.CurrentTouchpadGesture.Type;
     
        if (inputType == MLInput.Controller.TouchpadGesture.GestureType.ForceTapUp)
        {
            Debug.Log("Touch Tap recognized");
            //toggle Handle
            if (Handle == selectedHandle.minHandle)
            {
                Handle = selectedHandle.maxHandle;
                ui_text.text = "max Handle selected";
            }
            else
            {
                Handle = selectedHandle.minHandle;
                ui_text.text = "min Handle selected";
            }

            Debug.Log("Toggled Handle");
        }

    }

}