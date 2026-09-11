
using Unity.VisualScripting;
using UnityEngine;

public class Fireplace : MonoBehaviour
{
    [SerializeField] private Light fireLight;
    [SerializeField] Camera playerCamera;
    [SerializeField] float maxLookDistance = 2f;
    private Currentitem currentitem;

    [Header("Heat")]
    [SerializeField] private float maxHeat = 100f;
    [SerializeField] private float currentHeat = 100f;
    [SerializeField] private float woodHeatAdd = 25f;
    [SerializeField] private float declinePerSecond = 5f;

    [Header("Light")]
    [SerializeField] private float maxBrightness = 1.5f;

    private float fireBrightness;
    float playerDis;
    public bool hasWood;
    void Start()
    {
        currentitem = Object.FindAnyObjectByType<Currentitem>();
    }
    private void Update()
    {
        CanInteract();
    }
    private void FixedUpdate()
    {
        DeclineHeat();
        FireBrightness();
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
    void CanInteract()
    {
        if (hasWood == true && Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxLookDistance))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    AddWood();
                    currentitem.DelItem();
                }
            }
        }
    }
}

