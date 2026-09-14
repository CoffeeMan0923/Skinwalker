using UnityEngine;
using UnityEngine.AI;

public class MonsterEntrance : MonoBehaviour
{
    [Header("Entrance Type")]
    [SerializeField] private bool isWindow = false;
    [SerializeField] private bool isSecondaryDoor = false;

    [Header("Attack Point")]
    [SerializeField] private Transform attackPoint;

    [Header("Window Teleport")]
    [SerializeField] private Transform windowTeleportPoint;

    [Header("Window")]
    [SerializeField] private GameObject brokenWindowObject;
    [SerializeField] private float brokenWindowWaitTime = 40f;

    [Header("Door")]
    [SerializeField] private float maxDoorHealth = 4f;
    [SerializeField] private float currentDoorHealth = 4f;

    [Header("Barricade")]
    [SerializeField] private WindowBarricade windowBarricade;

    private bool isBroken;
    private float brokenTimer;

    public bool IsWindow => isWindow;
    public bool IsSecondaryDoor => isSecondaryDoor;

    public Transform AttackPoint => attackPoint;

    public Transform WindowTeleportPoint =>
        windowTeleportPoint;

    public bool IsBroken => isBroken;

    public bool IsBarricaded
    {
        get
        {
            return windowBarricade != null &&
                   windowBarricade.IsBarricaded;
        }
    }

    public bool IsReadyForEntry
    {
        get
        {
            return isBroken &&
                   brokenTimer >= brokenWindowWaitTime &&
                   !IsBarricaded;
        }
    }

    private void Start()
    {
        currentDoorHealth = maxDoorHealth;

        if (windowBarricade == null)
        {
            windowBarricade =
                GetComponent<WindowBarricade>();
        }
    }

    private void Update()
    {
        if (isBroken && isWindow)
        {
            brokenTimer += Time.deltaTime;
        }
    }

    public void Damage(float damage)
    {
        if (isBroken)
            return;

        if (isWindow)
        {
            BreakWindow();
            return;
        }

        currentDoorHealth -= damage;

        if (currentDoorHealth <= 0f)
        {
            BreakDoor();
        }
    }

    private void BreakWindow()
    {
        if (isBroken)
            return;

        isBroken = true;
        brokenTimer = 0f;

        // Swap to broken version.
        if (brokenWindowObject != null)
        {
            brokenWindowObject.SetActive(true);
        }

        // Disable this object's renderer.
        MeshRenderer mesh =
            GetComponent<MeshRenderer>();

        if (mesh != null)
        {
            mesh.enabled = false;
        }

        Collider col = GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = true;
        }
    }

    private void BreakDoor()
    {
        if (isBroken)
            return;

        isBroken = true;

        // Completely disable the door entrance.
        gameObject.SetActive(false);
    }

    public void ResetEntrance()
    {
        isBroken = false;
        brokenTimer = 0f;
        currentDoorHealth = maxDoorHealth;

        MeshRenderer mesh =
            GetComponent<MeshRenderer>();

        if (mesh != null)
        {
            mesh.enabled = true;
        }

        Collider col =
            GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = true;
        }

        if (brokenWindowObject != null)
        {
            brokenWindowObject.SetActive(false);
        }

        if (windowBarricade != null)
        {
            windowBarricade.RemoveBarricade();
        }

        gameObject.SetActive(true);
    }
    public void OnBarricaded()
    {
        SimpleMonsterAI monster =
            FindFirstObjectByType<SimpleMonsterAI>();

        if (monster != null)
        {
            monster.CancelWindowTarget(this);
        }
    }

    public void OnBarricadeRemoved()
    {
        // The window is now available again.
        // The monster's normal entrance-selection system
        // will be able to target it again.
    }
    public bool IsDoorDamaged()
    {
        return !isWindow &&
               !isBroken &&
               currentDoorHealth < maxDoorHealth;
    }

    public void RepairDoor()
    {
        if (isWindow)
            return;

        if (isBroken)
            return;

        currentDoorHealth = maxDoorHealth;
    }
}
