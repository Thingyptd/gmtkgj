using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(BoxCollider2D))]
public class Boulder : MonoBehaviour
{
    public Grid grid;

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
    }
    public void MoveToCell(Vector3Int cell)
    {
        Vector3 worldPos = grid.GetCellCenterWorld(cell);
        worldPos.z = transform.position.z;
        transform.position = worldPos;
        FMODEvents.Instance.PlayBoulderSound();
    }

    public void FallIntoPit()
    {
        Destroy(gameObject);
    }
}