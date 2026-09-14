
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Fireplace : MonoBehaviour
{
    [SerializeField] private Light fireLight;
    [SerializeField] Camera playerCamera;
    [SerializeField] float maxLookDistance = 2f;
    [SerializeField] Transform playerLoc;
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] Slider temperatureBar;
    [SerializeField] GameObject meatObj;
    [SerializeField] Transform cookPos;
    private Currentitem currentitem;

    [Header("Heat")]
    [SerializeField] private float maxHeat = 100f;
    [SerializeField] private float currentHeat = 100f;
    [SerializeField] private float woodHeatAdd = 25f;
    [SerializeField] private float declinePerSecond = 5f;
    [SerializeField] private float maxPlayerTemperature = 100f;
    [SerializeField] private float playerTemperature = 100f;
    [SerializeField] private float naturalTemperatureGainPerSecond = 3f;

    [SerializeField] private float freezingTemperature = 10;
    [SerializeField] private float freezingDamage = 2.5f;
    [SerializeField] private float temperatureRange = 10f;
    [SerializeField] private float temperatureGainPerSecond = 10f;
    [SerializeField] private float temperatureLossPerSecond = 5f;

    [SerializeField] private RawImage coldEffedct;
    [Header("Light")]
    [SerializeField] private float maxBrightness = 1.5f;

    private float fireBrightness;
    private float distance;
    private AudioSource audioSource;
    public bool InZone;
    float playerDis;
    public bool hasWood;
    public bool hasMeat;
    public bool canPlace = true;
    public bool isNight;
    void Start()
    {
        currentitem = FindFirstObjectByType<Currentitem>();

        if (currentitem == null)
        {
            Debug.LogError("Fireplace could not find Currentitem in the scene!", this);
        }

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogWarning("Fireplace has no AudioSource assigned!", this);
        }

        StartCoroutine(ColdDamage());
    }
    private void Update()
    {
        distance = Vector3.Distance(playerLoc.transform.position, gameObject.transform.position);
        CanInteract();
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            InZone = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            InZone = false;
        }
    }
    
    private void FixedUpdate()
    {
        DeclineHeat();
        FireBrightness();
        Temperature();
    }

    private void DeclineHeat()
    {
        currentHeat = Mathf.MoveTowards(
            currentHeat,
            0f,
            declinePerSecond * Time.fixedDeltaTime
        );
    }

    private void FireBrightness()
    {
        float heatPercent = currentHeat / maxHeat;

        fireBrightness = heatPercent * maxBrightness;

        fireLight.intensity = fireBrightness;
    }

    public void AddWood()
    {
        currentHeat = Mathf.Clamp(
            currentHeat + woodHeatAdd,
            0f,
            maxHeat
        );
    }
    void CookMeat()
    {
        Instantiate(meatObj, cookPos.position, cookPos.rotation);
        canPlace = false;
    }
    void CanInteract()
    {
        if (!Input.GetKeyDown(KeyCode.E))
            return;
        if (currentitem == null)
        {
            currentitem = FindFirstObjectByType<Currentitem>();
        }
        if (currentitem == null)
        {
            Debug.LogError("Fireplace: Currentitem is NULL!", this);
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0)
        );

        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, maxLookDistance))
            return;

        if (hit.collider.gameObject != gameObject)
            return;

        GameObject currentItem = currentitem.GetCurrentItem();

        if (currentItem == null)
            return;

        if (currentItem.CompareTag("Wood"))
        {
            AddWood();

            currentitem.DelItem();
            return;
        }

        if (currentItem.CompareTag("Meat") && canPlace)
        {
            CookMeat();

            currentitem.DelItem();
            return;
        }
    }
    void Temperature()
    {
        if (audioSource != null)
        {
            audioSource.volume = playerTemperature / 100f;
        }
        if (temperatureBar != null)
        {
            temperatureBar.value = playerTemperature / 100f;
        }
        if (!isNight)
        {
            playerTemperature += naturalTemperatureGainPerSecond * Time.fixedDeltaTime;

            playerTemperature = Mathf.Clamp(
                playerTemperature,
                0f,
                maxPlayerTemperature
            );

            UpdateColdEffect();
            return;
        }

        float distancePercent = Mathf.Clamp01(distance / temperatureRange);

        //float warmthPercent = 1f - distancePercent;

        float fireHeatPercent = Mathf.Clamp01(currentHeat / maxHeat);

        float heatReceivedPercent = fireHeatPercent;

        float temperatureGain = temperatureGainPerSecond * heatReceivedPercent;

        if (fireHeatPercent <= 0f || !InZone)
        {

            playerTemperature -= temperatureLossPerSecond * Time.fixedDeltaTime;
        }
        else
        {
            temperatureGain = temperatureGainPerSecond * heatReceivedPercent;

            playerTemperature += temperatureGain * Time.fixedDeltaTime;
        }

        playerTemperature = Mathf.Clamp(
            playerTemperature,
            0f,
            maxPlayerTemperature
        );
        UpdateColdEffect();
    }
    void UpdateColdEffect()
    {
        if (coldEffedct != null)
        {
            float coldOpacity =
                1f - Mathf.Clamp01(playerTemperature / 80f);

            Color color = coldEffedct.color;
            color.a = coldOpacity;
            coldEffedct.color = color;
        }
    }
    IEnumerator ColdDamage()
    {
        if(playerTemperature <= freezingTemperature)
        {
            playerHealth.Damage(freezingDamage);
            yield return new WaitForSeconds(1);
            StartCoroutine(ColdDamage());
            yield return null;
        }
        else
        {
            yield return new WaitForSeconds(1);
            StartCoroutine(ColdDamage());
            yield return null;
        }
    }
}

