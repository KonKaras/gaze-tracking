using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerHitRegister : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnColliderEnter(Collider other)
    {
        if(other.gameObject.layer == 5)
        {
            Debug.Log("Hit UI");
        }
    }
    public void Follow(Vector3 pos)
    {
        GetComponent<SphereCollider>().enabled = true;
        transform.position = pos;
    }

    public void Release()
    {
        GetComponent<SphereCollider>().enabled = false;
    }
}
