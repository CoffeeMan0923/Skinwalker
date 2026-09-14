using UnityEngine;

public class WindowBarricade : MonoBehaviour
{
    [Header("Barricade")]
    [SerializeField] private GameObject barricadeObject;

    private bool barricaded;

    public bool IsBarricaded => barricaded;

    public void Barricade()
    {
        if (barricaded)
            return;

        barricaded = true;

        if (barricadeObject != null)
        {
            barricadeObject.SetActive(true);
        }

        MonsterEntrance entrance =
            GetComponent<MonsterEntrance>();

        if (entrance != null)
        {
            entrance.OnBarricaded();
        }
    }

    public void RemoveBarricade()
    {
        barricaded = false;

        if (barricadeObject != null)
        {
            barricadeObject.SetActive(false);
        }

        MonsterEntrance entrance =
            GetComponent<MonsterEntrance>();

        if (entrance != null)
        {
            entrance.OnBarricadeRemoved();
        }
    }
}