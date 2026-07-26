using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LevelButton : MonoBehaviour
{
    public Grid grid;
    public SpriteRenderer spriteRenderer;

    public Sprite idleSprite;
    public Sprite pressedSprite;

    public GameObject stairsToReveal;

    private bool isPressed = false;

    void Awake()
    {
        if (grid == null)
            grid = FindAnyObjectByType<Grid>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        GetComponent<BoxCollider2D>().isTrigger = true;

        if (grid != null)
        {
            Vector3Int cell = grid.WorldToCell(transform.position);
            Vector3 center = grid.GetCellCenterWorld(cell);
            center.z = transform.position.z;
            transform.position = center;
        }

        if (spriteRenderer != null && idleSprite != null)
            spriteRenderer.sprite = idleSprite;

        if (stairsToReveal != null)
            stairsToReveal.SetActive(false);
    }

    public void Press()
    {
        if (isPressed) return;
        isPressed = true;

        if (spriteRenderer != null && pressedSprite != null)
            spriteRenderer.sprite = pressedSprite;

        if (stairsToReveal != null)
            stairsToReveal.SetActive(true);
    }
}