using System;
using UnityEngine;

public class Currentitem : MonoBehaviour
{
    [SerializeField] GameObject currentObj;
    [SerializeField] Transform itemPlaceholder;
    [SerializeField] Camera mainCamera;
    [SerializeField] Fireplace fireplace;
    [SerializeField] float maxGrabDistance;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            GrabItem();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropItem();
        }
    }
    public void GrabItem()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxGrabDistance))
        {
            GameObject item = hit.collider.gameObject;
            if (item.layer == 6)
            {
                if (item.tag == "Wood") fireplace.hasWood = true;
                else fireplace.hasWood = false;

                if(currentObj != null)
                {
                    currentObj.transform.SetParent(null);
                    currentObj.AddComponent<Rigidbody>();
                    currentObj.GetComponent<Collider>().enabled = true;
                }
                
                Destroy(item.GetComponent<Rigidbody>());
                item.transform.position = itemPlaceholder.position;
                item.GetComponent<Collider>().enabled = false;
                item.transform.SetParent(mainCamera.transform);
                currentObj = item;
            }
        }
    }
    void DropItem()
    {
        if (currentObj != null)
        {
            currentObj.transform.SetParent(null);
            currentObj.AddComponent<Rigidbody>();
            currentObj.GetComponent<Collider>().enabled = true;
            currentObj = null;
            fireplace.hasWood = false;
        }
    }
    public void DelItem()
    {
        if(currentObj != null)
        {
            Destroy(currentObj);
        }
        currentObj = null;
        fireplace.hasWood = false;
    }
}
