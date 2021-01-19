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
    GameObject lastTouchedUI = null;

    // Start is called before the first frame update
    void Start()
    {
        MLHandTracking.Start();
        ui = GameObject.Find("Canvas").GetComponent<UI>();
        raycast = ui.gameObject.GetComponent<GraphicRaycaster>();
    }

    private void OnDisable()
    {
        MLHandTracking.Stop();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (MLHandTracking.IsStarted)
        {
            Vector3 handIndexTipPos = GetIndexFingerTipPos();
            if (handIndexTipPos != Vector3.positiveInfinity)
            {
                
                List<RaycastResult> results = new List<RaycastResult>();
                eventData = new PointerEventData(eventSystem);
                eventData.position = cam.WorldToScreenPoint(handIndexTipPos);
                raycast.Raycast(eventData, results);
                Debug.DrawRay(handIndexTipPos, cam.transform.TransformDirection(Vector3.forward) * 10, Color.green);
                bool gotSlider = false;

                foreach (RaycastResult res in results)
                {
                    if (res.gameObject.name.Equals("Handle"))
                    {
                        Debug.Log(res.gameObject.name);
                        Transform parent = res.gameObject.transform;
                        while (parent != null)
                        {
                            if (parent.GetComponent<Slider>() != null)
                            {
                                lastTouchedUI = res.gameObject;
                                lastTouchedUI.GetComponent<Image>().color = Color.green;

                                Slider slider = parent.GetComponent<Slider>();
                                Vector2 sliderInSP = cam.WorldToScreenPoint(slider.handleRect.position).normalized;
                                Vector2 tipInSP = cam.WorldToScreenPoint(handIndexTipPos).normalized;
                                //float newValue = Mathf.Abs((cam.WorldToScreenPoint(slider.handleRect.position).x - cam.WorldToScreenPoint(handIndexTipPos).x));

                                float newValue = Mathf.Abs(sliderInSP.x - tipInSP.x);
                                Debug.Log("slider value" + newValue);
                                ui.HandleTime(newValue);//Mathf.Abs(((Vector2)cam.WorldToScreenPoint(slider.handleRect.position) - eventData.position).normalized.magnitude));

                                gotSlider = true;
                                break;
                            }
                            parent = parent.transform.parent;
                        }
                    }

                    if (gotSlider) break;
                }
                if(!gotSlider && lastTouchedUI != null) lastTouchedUI.gameObject.GetComponent<Image>().color = Color.grey;

                /*
                RaycastHit hit;
                if (Physics.Raycast(handIndexTipPos, cam.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
                {
                    if(hit.transform.gameObject.layer == 5)
                    {
                        Debug.Log(hit.transform.gameObject.name);
                    }
                }
                */
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
