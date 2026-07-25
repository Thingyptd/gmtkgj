using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DeathTrap : MonoBehaviour
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

    /// <summary>
    /// Trap disappears when triggered
    /// </summary>
    public void Trigger()
    {
        Destroy(gameObject);
    }
}