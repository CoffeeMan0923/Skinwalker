using System;
using UnityEngine;

public class Currentitem : MonoBehaviour
{
    [Header("Item Slots")]
    [SerializeField] private GameObject[] itemSlots = new GameObject[3];
    [SerializeField] private Transform itemPlaceholder;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float maxGrabDistance = 3f;

    [Header("Fireplace")]
    [SerializeField] private Fireplace fireplace;

    [Header("UI")]
    [SerializeField] private RectTransform[] slotUI = new RectTransform[3];
    [SerializeField] private float selectedSlotMultiplier = 1.1f;
    [SerializeField] private float uiSmoothSpeed = 10f;

    [Header("Rifle")]
    [SerializeField] private Rifle rifleScript;
    [SerializeField] private Riflesway rifleSwayScript;

    private Vector3[] originalUIScales;
    private int selectedSlot = 0;

    void Start()
    {
        originalUIScales = new Vector3[slotUI.Length];

        for (int i = 0; i < slotUI.Length; i++)
        {
            if (slotUI[i] != null)
            {
                originalUIScales[i] = slotUI[i].localScale;
            }
        }

        UpdateActiveItem();
        UpdateFireplace();
    }

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

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            DelItem();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectSlot(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectSlot(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectSlot(2);
        }

        UpdateUIScale();
    }

    public void GrabItem()
    {
        Ray ray = mainCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0)
        );

        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, maxGrabDistance))
            return;

        GameObject item = hit.collider.gameObject;

        // =====================================================
        // INFINITE WOOD SUPPLY
        // =====================================================

        InfiniteWood woodPile =
            item.GetComponent<InfiniteWood>();

        if (woodPile == null)
        {
            woodPile =
                item.GetComponentInParent<InfiniteWood>();
        }

        if (woodPile != null)
        {
            int emptySlot = GetEmptySlot();

            if (emptySlot == -1)
            {
                return;
            }

            GameObject wood = woodPile.GetWood();

            if (wood == null)
            {
                return;
            }

            Rigidbody woodRb =
                wood.GetComponent<Rigidbody>();

            if (woodRb != null)
            {
                Destroy(woodRb);
            }

            Collider woodCollider =
                wood.GetComponent<Collider>();

            if (woodCollider != null)
            {
                woodCollider.enabled = false;
            }

            wood.transform.position =
                itemPlaceholder.position;

            wood.transform.SetParent(
                mainCamera.transform
            );

            itemSlots[emptySlot] = wood;

            SelectSlot(emptySlot);

            return;
        }

        // =====================================================
        // BARRICADE
        // =====================================================

        WindowBarricade barricade =
            item.GetComponent<WindowBarricade>();

        if (barricade == null)
        {
            barricade =
                item.GetComponentInParent<WindowBarricade>();
        }

        if (barricade != null)
        {
            if (itemSlots[selectedSlot] != null &&
                itemSlots[selectedSlot].CompareTag("Wood"))
            {
                barricade.Barricade();

                Destroy(itemSlots[selectedSlot]);

                itemSlots[selectedSlot] = null;

                UpdateFireplace();

                return;
            }

            return;
        }

        // =====================================================
        // NORMAL INTERACTION LAYER
        // =====================================================

        if (item.layer != 6)
        {
            return;
        }

        // =====================================================
        // DOOR
        // =====================================================

        if (item.CompareTag("Door"))
        {
            Door door =
                item.GetComponent<Door>();

            if (door != null)
            {
                MonsterEntrance entrance =
                    item.GetComponent<MonsterEntrance>();

                if (entrance == null)
                {
                    entrance =
                        item.GetComponentInParent<MonsterEntrance>();
                }

                // Repair damaged door with wood
                if (entrance != null &&
                    entrance.IsDoorDamaged() &&
                    itemSlots[selectedSlot] != null &&
                    itemSlots[selectedSlot].CompareTag("Wood"))
                {
                    entrance.RepairDoor();

                    Destroy(itemSlots[selectedSlot]);

                    itemSlots[selectedSlot] = null;

                    UpdateFireplace();

                    return;
                }

                // Normal door interaction
                door.UseDoor();
            }

            return;
        }

        // =====================================================
        // NORMAL ITEM PICKUP
        // =====================================================

        int emptySlotNormal = GetEmptySlot();

        if (emptySlotNormal == -1)
        {
            return;
        }

        // =====================================================
        // RIFLE
        // =====================================================

        if (item.CompareTag("Rifle"))
        {
            PickUpRifle(
                item,
                emptySlotNormal
            );

            return;
        }

        // =====================================================
        // COOKED MEAT
        // =====================================================

        if (item.CompareTag("CookedMeat"))
        {
            Meat meat =
                item.GetComponent<Meat>();

            if (meat != null)
            {
                meat.enabled = true;
            }
        }

        // =====================================================
        // NORMAL ITEM
        // =====================================================

        Rigidbody rb =
            item.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Destroy(rb);
        }

        Collider col =
            item.GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = false;
        }

        item.transform.position =
            itemPlaceholder.position;

        item.transform.SetParent(
            mainCamera.transform
        );

        itemSlots[emptySlotNormal] = item;

        SelectSlot(emptySlotNormal);
    }

    private void PickUpRifle(GameObject rifle, int slot)
    {
        Rigidbody rb =
            rifle.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Destroy(rb);
        }

        BoxCollider boxCollider =
            rifle.GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        if (rifleScript != null)
        {
            rifleScript.enabled = true;
        }

        if (rifleSwayScript != null)
        {
            rifleSwayScript.enabled = true;
        }

        itemSlots[slot] = rifle;

        SelectSlot(slot);
    }

    private int GetEmptySlot()
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] == null)
            {
                return i;
            }
        }

        return -1;
    }

    private void SelectSlot(int slot)
    {
        if (slot < 0 || slot >= itemSlots.Length)
            return;

        selectedSlot = slot;

        UpdateActiveItem();
        UpdateFireplace();
    }

    private void UpdateActiveItem()
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null)
            {
                itemSlots[i].SetActive(
                    i == selectedSlot
                );
            }
        }
    }

    private void UpdateUIScale()
    {
        for (int i = 0; i < slotUI.Length; i++)
        {
            if (slotUI[i] == null)
                continue;

            Vector3 targetScale =
                originalUIScales[i];

            if (i == selectedSlot)
            {
                targetScale *= selectedSlotMultiplier;
            }

            slotUI[i].localScale = Vector3.Lerp(
                slotUI[i].localScale,
                targetScale,
                Time.deltaTime * uiSmoothSpeed
            );
        }
    }

    private void UpdateFireplace()
    {
        if (fireplace == null)
            return;

        if (itemSlots[selectedSlot] != null &&
            itemSlots[selectedSlot].CompareTag("Wood"))
        {
            fireplace.hasWood = true;
            return;
        }
        else
        {
            fireplace.hasWood = false;
        }

        if (itemSlots[selectedSlot] != null &&
            itemSlots[selectedSlot].CompareTag("Meat"))
        {
            fireplace.hasMeat = true;
            return;
        }
        else
        {
            fireplace.hasMeat = false;
        }
    }

    public void DropItem()
    {
        if (itemSlots[selectedSlot] == null)
            return;

        GameObject item =
            itemSlots[selectedSlot];

        // =====================================================
        // RIFLE
        // =====================================================

        if (item.CompareTag("Rifle"))
        {
            DropRifle(item);

            itemSlots[selectedSlot] = null;

            UpdateFireplace();

            return;
        }

        // =====================================================
        // COOKED MEAT
        // =====================================================

        if (item.CompareTag("CookedMeat"))
        {
            Meat meat =
                item.GetComponent<Meat>();

            if (meat != null)
            {
                meat.enabled = false;
            }
        }

        // =====================================================
        // NORMAL ITEM
        // =====================================================

        item.transform.SetParent(null);

        Rigidbody rb =
            item.GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = item.AddComponent<Rigidbody>();
        }

        Collider col =
            item.GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = true;
        }

        itemSlots[selectedSlot] = null;

        UpdateFireplace();
    }

    private void DropRifle(GameObject rifle)
    {
        if (rifleScript != null)
        {
            rifleScript.enabled = false;
        }

        if (rifleSwayScript != null)
        {
            rifleSwayScript.enabled = false;
        }

        BoxCollider boxCollider =
            rifle.GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }

        Rigidbody rb =
            rifle.GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = rifle.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.useGravity = true;
    }

    public bool DelItem()
    {
        if (selectedSlot < 0 || selectedSlot >= itemSlots.Length)
            return false;

        GameObject item = itemSlots[selectedSlot];

        if (item == null)
        {
            UpdateFireplace();
            return false;
        }

        itemSlots[selectedSlot] = null;

        Destroy(item);

        UpdateActiveItem();
        UpdateFireplace();

        return true;
    }
    public GameObject GetCurrentItem()
    {
        if (selectedSlot < 0 || selectedSlot >= itemSlots.Length)
            return null;

        return itemSlots[selectedSlot];
    }
}