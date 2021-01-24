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
    Slider slider;
    Transform beginIndicator;

    // Start is called before the first frame update
    void Start()
    {
        MLHandTracking.Start();
        ui = GameObject.Find("Canvas").GetComponent<UI>();
        slider = ui.transform.Find("Slider").GetComponent<Slider>();
        beginIndicator = slider.transform.Find("BeginIndicator");
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
            Vector3[] handIndexImplicitPose = GetIndexFingerTipPosandOrientation();
            Vector3 handIndexTipPos = handIndexImplicitPose[0];
            Vector3 handIndexTipDir = handIndexImplicitPose[1];
            if (handIndexTipPos != Vector3.positiveInfinity)
            {
                //Transform handIndexTip = new Transform(handIndexTipPos, Quaternion.identity, new Vector3(1,1,1));
                /*

                List<RaycastResult> results = new List<RaycastResult>();
                eventData = new PointerEventData(eventSystem);
                eventData.position = cam.WorldToScreenPoint(handIndexTipPos);
                raycast.Raycast(eventData, results);
                Debug.DrawRay(handIndexTipPos, cam.transform.TransformDirection(Vector3.forward) * 10, Color.green);
                bool gotSlider = false;

                foreach (RaycastResult res in results)
                {
                    Debug.Log(res.gameObject.name);

                    if (res.gameObject.name.Equals("Handle"))
                    {
                        Transform parent = res.gameObject.transform;
                        while (parent != null)
                        {
                            if (parent.GetComponent<Slider>() != null)
                            {
                                lastTouchedUI = res.gameObject;
                                lastTouchedUI.GetComponent<Image>().color = Color.green;

                                Slider slider = parent.GetComponent<Slider>();
                                
                                Vector2 sliderInVP = cam.WorldToViewportPoint(slider.handleRect.position);
                                Vector2 tipInVP = cam.WorldToViewportPoint(handIndexTipPos);

                                Transform beginIndicator = slider.transform.Find("BeginIndicator");
                                Vector2 beginInVP = cam.WorldToViewportPoint(beginIndicator.position);

                                float sliderWidth = slider.gameObject.GetComponent<RectTransform>().rect.width;
                                Vector2 endInSP = cam.WorldToScreenPoint(beginIndicator.position) + beginIndicator.right.normalized * sliderWidth;
                                Vector2 endInVP = cam.ScreenToViewportPoint(endInSP);
                                Debug.Log("beginInSP " + cam.WorldToScreenPoint(beginIndicator.position));
                                Debug.Log("beginInVP " + beginInVP);
                                Debug.Log("endInSP " + endInSP);
                                Debug.Log("endInVP " + endInVP);
                                Debug.Log("view pos " + tipInVP);
                                //float newValue = Mathf.Abs((cam.WorldToScreenPoint(slider.handleRect.position).x - cam.WorldToScreenPoint(handIndexTipPos).x));

                                float newValue = Mathf.Abs(tipInVP.x - beginInVP.x);
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
                */
                //if(!gotSlider && lastTouchedUI != null) lastTouchedUI.gameObject.GetComponent<Image>().color = Color.grey;

                Debug.DrawRay(handIndexTipPos, handIndexTipDir*10, Color.red);
                //Debug.Log("Scanning...");
                RaycastHit hit;
                bool gotSlider = false;
                if (Physics.Raycast(handIndexTipPos, handIndexTipDir, out hit, Mathf.Infinity))
                {
                    if (hit.transform.gameObject.layer == 5)
                    {
                        Debug.Log(hit.transform.gameObject.name);

                        lastTouchedUI = hit.transform.gameObject;
                        lastTouchedUI.GetComponent<Image>().color = Color.green;
                        Debug.Log("hit point " + hit.point);
                        Debug.Log("beginIndicator " + beginIndicator.position);
                        Vector3 hitDifference = hit.point - lastTouchedUI.transform.position;
                        float newValue = hitDifference.magnitude;
                        beginIndicator.Translate(hit.point);
                        
                        /*
                        Vector2 sliderInVP = cam.WorldToViewportPoint(slider.handleRect.position);
                        Vector2 tipInVP = cam.WorldToViewportPoint(handIndexTipPos);

                        Transform beginIndicator = slider.transform.Find("BeginIndicator");
                        Vector2 beginInVP = cam.WorldToViewportPoint(beginIndicator.position);

                        float sliderWidth = slider.gameObject.GetComponent<RectTransform>().rect.width;
                        Vector2 endInSP = cam.WorldToScreenPoint(beginIndicator.position) + beginIndicator.right.normalized * sliderWidth;
                        Vector2 endInVP = cam.ScreenToViewportPoint(endInSP);
                        Debug.Log("beginInSP " + cam.WorldToScreenPoint(beginIndicator.position));
                        Debug.Log("beginInVP " + beginInVP);
                        Debug.Log("endInSP " + endInSP);
                        Debug.Log("endInVP " + endInVP);
                        Debug.Log("view pos " + tipInVP);
                        //float newValue = Mathf.Abs((cam.WorldToScreenPoint(slider.handleRect.position).x - cam.WorldToScreenPoint(handIndexTipPos).x));

                        float newValue = Mathf.Abs(tipInVP.x - beginInVP.x);
                        */
                        Debug.Log("slider value" + newValue);
                        //ui.HandleTime(newValue);//Mathf.Abs(((Vector2)cam.WorldToScreenPoint(slider.handleRect.position) - eventData.position).normalized.magnitude));

                        gotSlider = true;

                    }
                }
                else
                {
                    if (!gotSlider && lastTouchedUI != null) lastTouchedUI.gameObject.GetComponent<Image>().color = Color.grey;
                }
            }
        }
    }

    float MapRangeToRange(float input, float input_min, float input_max, float output_min, float output_max)
    {
        return (input - input_min) * (output_max - output_min) / (input_max - input_min) + output_min;
    }

    Vector3[] GetIndexFingerTipPosandOrientation()
    {
        Vector3[] result = new Vector3[2];
        MLHandTracking.Hand hand = handType.Equals(MLHandTracking.HandType.Left) ? MLHandTracking.Left : MLHandTracking.Right;
        if (hand != null)
        {
            MLHandTracking.KeyPoint tip = hand.Index.Tip;
            MLHandTracking.KeyPoint knuckle = hand.Index.MCP; //middle bone of index finger
            
            Vector3 dir = -(tip.Position - knuckle.Position).normalized;
            //Debug.Log("world pos " + tip.Position);
            result[0] = tip.Position;
            result[1] = dir;
        }
        else
        {
            result[0] = Vector3.positiveInfinity;
            result[1] = Vector3.positiveInfinity;
        }
        return result;
    }
}
