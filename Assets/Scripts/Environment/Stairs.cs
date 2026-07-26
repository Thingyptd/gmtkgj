using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Stairs : MonoBehaviour
{
    public Grid grid;

    [Tooltip("Se true (default), le scale partono BLOCCATE indipendentemente da come il GameObject è impostato in scena (attivo o no). Vanno sbloccate esplicitamente via Unlock().")]
    public bool startLocked = true;

    public bool IsUnlocked { get; private set; }

    void Awake()
    {
        if (grid == null)
            grid = FindAnyObjectByType<Grid>();

        GetComponent<BoxCollider2D>().isTrigger = true;

        if (grid != null)
        {
            Vector3Int cell = grid.WorldToCell(transform.position);
            Vector3 center = grid.GetCellCenterWorld(cell);
            center.z = transform.position.z;
            transform.position = center;
        }

        IsUnlocked = !startLocked;
    }

    public void Unlock()
    {
        IsUnlocked = true;
        FMODEvents.Instance.PlayStartSound();
    }
}