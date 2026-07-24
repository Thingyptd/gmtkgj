using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(BoxCollider2D))]
public class Boulder : MonoBehaviour
{
    public Grid grid;

    void Awake()
    {
        if (grid == null)
            grid = FindObjectOfType<Grid>();

        GetComponent<BoxCollider2D>().isTrigger = true;

        // Auto-allineamento al centro cella, comodo se lo piazzi a occhio in editor
        if (grid != null)
        {
            Vector3Int cell = grid.WorldToCell(transform.position);
            Vector3 center = grid.GetCellCenterWorld(cell);
            center.z = transform.position.z;
            transform.position = center;
        }
    }

    /// <summary>
    /// Sposta il masso su una cella normale (non-pit).
    /// </summary>
    public void MoveToCell(Vector3Int cell)
    {
        Vector3 worldPos = grid.GetCellCenterWorld(cell);
        worldPos.z = transform.position.z;
        transform.position = worldPos;
    }

    /// <summary>
    /// Il masso viene spinto in un precipizio: cade dentro e viene rimosso dal gioco.
    /// Il pit resta "vuoto" (non si tappa) — se in futuro vuoi che si tappi, qui è dove intervenire.
    /// </summary>
    public void FallIntoPit()
    {
        Destroy(gameObject);
    }
}