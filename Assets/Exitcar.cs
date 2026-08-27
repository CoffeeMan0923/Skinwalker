using System;
using System.Collections;
using UnityEngine;

public class Exitcar : MonoBehaviour
{
    public bool CanExit = false;
    [SerializeField] private GameObject[] disableObjects;
    [SerializeField] private GameObject player;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openDoor;
    [SerializeField] private AudioClip closeDoor;

    void Start() 
    { 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space) && CanExit == true)
        {
            exitCar();

        }

    }
    void exitIsTrue()
    {
        CanExit = true;
    }
    void exitCar()
    {
        CanExit = false;
        foreach(GameObject obj in disableObjects)
        {
            obj.SetActive(false);
        }
        player.SetActive(true);
        StartCoroutine(exitSFX());
    }
    IEnumerator exitSFX()
    {
        audioSource.loop = false;
        audioSource.clip = openDoor;
        audioSource.Play();
        yield return new WaitForSeconds(1);
        audioSource.clip = closeDoor;
        audioSource.Play();
        yield break;
    }
}
