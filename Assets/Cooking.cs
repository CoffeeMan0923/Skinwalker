using System.Collections;
using UnityEngine;

public class Cooking : MonoBehaviour
{
    [SerializeField] private GameObject cookedMeat;
    [SerializeField] private float timeToCook = 10;

    [SerializeField] private AudioClip meatcooking;
    [SerializeField] private Fireplace fireplace;
    private AudioSource soundSource;
    
    void Start()
    {
        fireplace = Object.FindAnyObjectByType<Fireplace>();
        StartCoroutine(cooking());
    }
    IEnumerator cooking()
    {
        //soundSource.clip = meatcooking;
        //soundSource.Play();
        yield return new WaitForSeconds(timeToCook);
        fireplace.canPlace = true;
        Instantiate(cookedMeat,gameObject.transform.position,gameObject.transform.rotation);
        Destroy(this.gameObject);
        yield return null;
    }
}
