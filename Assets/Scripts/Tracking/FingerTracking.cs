using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FingerTracking : MonoBehaviour
{
    public MLHandTracking.HandType handType;
    UI ui;
    GraphicRaycaster raycast;
    public EventSystem eventSystem;
    PointerEventData eventData;
    public Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        MLHandTracking.Start();
        UI ui = GameObject.Find("Canvas").GetComponent<UI>();
        raycast = ui.gameObject.GetComponent<GraphicRaycaster>();
    }

    private void OnDisable()
    {
        MLHandTracking.Stop();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 handIndexTipPos = GetIndexFingerTipPos();
        if (handIndexTipPos != Vector3.positiveInfinity)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            eventData = new PointerEventData(eventSystem);
            eventData.position = cam.WorldToScreenPoint(handIndexTipPos);
            raycast.Raycast(eventData, results);

            bool gotSlider = false;
            foreach(RaycastResult res in results)
            {
                Transform parent = res.gameObject.transform;
                while (parent != null)
                {
                    if (parent.GetComponent<Slider>() != null)
                    {
                        Debug.Log(parent.gameObject.name);
                        Slider slider = parent.GetComponent<Slider>();
                        Debug.Log("slider handle at " + slider.handleRect.position);
                        Vector2 sliderInSP = cam.WorldToScreenPoint(slider.handleRect.position);
                        Vector2 tipInSP = cam.WorldToScreenPoint(handIndexTipPos);
                        //float newValue = Mathf.Abs((cam.WorldToScreenPoint(slider.handleRect.position).x - cam.WorldToScreenPoint(handIndexTipPos).x));
                        Debug.Log("slider in sp " + sliderInSP);
                        Debug.Log("tip in sp " + tipInSP);
                        //ui.HandleTime(newValue);//Mathf.Abs(((Vector2)cam.WorldToScreenPoint(slider.handleRect.position) - eventData.position).normalized.magnitude));
                        
                        gotSlider = true;
                        break;
                    }
                    parent = parent.transform.parent;
                }

                if (gotSlider) break;
           
            }
        }
    }

    Vector3 GetIndexFingerTipPos()
    {
        MLHandTracking.Hand hand = handType.Equals(MLHandTracking.HandType.Left) ? MLHandTracking.Left : MLHandTracking.Right;
        if (hand != null)
        {
            MLHandTracking.KeyPoint tip = hand.Index.Tip;
            //MLHandTracking.KeyPoint middle = hand.Index.MCP;

            //return tip.Position - middle.Position;
            Debug.Log(tip.Position);
            return tip.Position;
        }
        else
        {
            return Vector3.positiveInfinity;
        }
    }
}
