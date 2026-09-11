
using UnityEngine;

public class Rifle : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;

    [Header("Raycast")]
    [SerializeField] private float rayDistance = 100f;
    [SerializeField] private LayerMask hitLayers;

    [Header("Debug")]
    [SerializeField] private bool drawRay = false;

    [Header("Aim Dot")]
    [SerializeField] private bool showAimDot = true;
    [SerializeField] private float aimDotSize = 8f;
    [SerializeField, Range(0f, 1f)] private float aimDotOpacity = 0.5f;
    private Texture2D aimDotTexture;

    [Header("Ammo")]
    [SerializeField] private int maxAmmo = 6;
    [SerializeField] private int currentAmmo = 6;
    [SerializeField] private bool chambered;
    private bool reloading = false;

    [Header("SFX")]
    [SerializeField] private AudioClip fireSFX;
    [SerializeField] private AudioClip reloadSFX;
    [SerializeField] private AudioClip noAmmoSFX;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private Animator animator;

    private Riflesway riflesway;

    void Start()
    {
        riflesway = GetComponent<Riflesway>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        CreateAimDotTexture();
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

        if (audioSource != null && fireSFX != null)
        {
            audioSource.PlayOneShot(fireSFX);
        }

        Vector3 origin = playerCamera.transform.position;

        Vector3 direction = playerCamera.transform.forward;

        if (drawRay)
        {
            Debug.DrawRay(
                origin,
                direction * rayDistance,
                Color.red,
                1f
            );
        }

        if (Physics.Raycast(
            origin,
            direction,
            out RaycastHit hit,
            rayDistance,
            hitLayers))
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
        bool aiming = Input.GetMouseButton(1);

        if (aiming)
        {
            riflesway.IsAiming(true);
        }
        else
        {
            riflesway.IsAiming(false);
        }

        if (Input.GetKeyDown(KeyCode.R)
            && chambered == false
            && currentAmmo > 0
            && reloading == false)
        {
            reloading = true;

            animator.SetBool("Reload", true);

            if (audioSource != null && reloadSFX != null)
            {
                audioSource.PlayOneShot(reloadSFX);
            }

            Invoke(nameof(Reload), 1.34f);
        }
        else
        {
            animator.SetBool("Reload", false);
        }

        if (Input.GetMouseButtonDown(0) && chambered == true)
        {
            FireRaycast();
        }
    }

    void OnGUI()
    {
        if (!showAimDot || !Input.GetMouseButton(1))
            return;

        float x = (Screen.width - aimDotSize) / 2f;
        float y = (Screen.height - aimDotSize) / 2f;

        Color originalColor = GUI.color;

        GUI.color = new Color(
            1f,
            1f,
            1f,
            aimDotOpacity
        );

        GUI.DrawTexture(
            new Rect(x, y, aimDotSize, aimDotSize),
            aimDotTexture
        );

        GUI.color = originalColor;
    }
    void CreateAimDotTexture()
    {
        int size = 64;

        aimDotTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(
                    new Vector2(x, y),
                    center
                );

                if (distance <= radius)
                    aimDotTexture.SetPixel(x, y, Color.white);
                else
                    aimDotTexture.SetPixel(x, y, Color.clear);
            }
        }

        aimDotTexture.Apply();
    }
}

