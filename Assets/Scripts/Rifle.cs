using UnityEngine;

public class Rifle : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private float rayDistance = 100f;
    [SerializeField] private LayerMask hitLayers;

    [Header("Debug")]
    [SerializeField] private bool drawRay = false;

    [Header("Ammo")]
    [SerializeField] private int maxAmmo = 6;
    [SerializeField] private int currentAmmo = 6;
    [SerializeField] private bool chambered;
    bool reloading = false;

    [Header("SFX")]
    [SerializeField] private AudioClip fireSFX;
    [SerializeField] private AudioClip reloadSFX;
    [SerializeField] private AudioClip noAmmoSFX;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] Animator animator;

    private Riflesway riflesway;
    void Start()
    {
        riflesway = this.gameObject.GetComponent<Riflesway>();
    }
    void Update()
    {
        CheckAimAndFire();
    }

    void Reload()
    {
        reloading = false;
        chambered = true;
    }

    private GameObject FireRaycast()
    {
        chambered = false;
        currentAmmo--;
        audioSource.PlayOneShot(fireSFX);
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        if (drawRay)
        {
            Debug.DrawRay(origin, direction * rayDistance, Color.red, 1f);
        }

        if (Physics.Raycast(origin, direction, out RaycastHit hit, rayDistance, hitLayers))
        {
            GameObject hitObject = hit.collider.transform.root.gameObject;

            if (hitObject.CompareTag("Deer"))
            {
                Deer deer = hitObject.GetComponent<Deer>();

                if (deer != null)
                {
                    deer.Damage();
                }
            }

            return hitObject;
        }

        return null;
    }
    void CheckAimAndFire()
    {
        if (Input.GetMouseButton(1))
        {
            riflesway.IsAiming(true);
        }
        else
        {
            riflesway.IsAiming(false);
        }

        if (Input.GetKeyDown(KeyCode.R) && chambered == false && currentAmmo > 0 && reloading == false)
        {
            reloading = true;
            animator.SetBool("Reload",true);
            audioSource.PlayOneShot(reloadSFX);
            Invoke("Reload", 1.34f);
        }
        else
        {
            animator.SetBool("Reload", false);
        }
        if (Input.GetMouseButtonDown(0) && chambered == true)
        {
            FireRaycast();
        }
        else if (audioSource != null && noAmmoSFX != null)
        {
            audioSource.clip = noAmmoSFX;
            audioSource.Play();
        }
        else
        {
            Debug.Log("Asign the audiosource and audio clip");
        }
    }
}
