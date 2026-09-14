using UnityEngine;

public class Meat : MonoBehaviour
{
    [SerializeField] Hunger hunger;
    [SerializeField] PlayerHealth health;
    [SerializeField] AudioSource audio;
    [SerializeField] AudioClip clip;
    void Start()
    {
        if(hunger == null)
        {
            hunger = Object.FindAnyObjectByType<Hunger>();
            health = Object.FindAnyObjectByType<PlayerHealth>();
            audio = Object.FindAnyObjectByType<SoundManager>().GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            hunger.IncreaseHunger(120);
            health.Heal(30);
            if(audio != null) audio.Play();
            Destroy(gameObject);
        }
    }

}
