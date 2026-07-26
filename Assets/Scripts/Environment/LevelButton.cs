using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LevelButton : MonoBehaviour
{
    public Grid grid;
    public SpriteRenderer spriteRenderer;

    public Sprite idleSprite;
    public Sprite pressedSprite;

    [Tooltip("Usato SOLO se questo bottone non è registrato su un ButtonPuzzleManager: in tal caso si autogestisce e sblocca queste scale da solo.")]
    public GameObject stairsToRevealIfStandalone;

    private ButtonPuzzleManager manager;
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
    }

    public void SetManager(ButtonPuzzleManager puzzleManager)
    {
        manager = puzzleManager;
    }

    public void Press()
    {
        if (isPressed) return;
        isPressed = true;

        if (spriteRenderer != null && pressedSprite != null)
            spriteRenderer.sprite = pressedSprite;

        if (manager != null)
        {
            manager.NotifyButtonPressed();
        }
        else if (stairsToRevealIfStandalone != null)
        {
            stairsToRevealIfStandalone.SetActive(true);

            Stairs stairsComponent = stairsToRevealIfStandalone.GetComponent<Stairs>();
            if (stairsComponent != null)
                stairsComponent.Unlock();
        }
    }
}