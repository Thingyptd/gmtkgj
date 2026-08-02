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

        if (manager == null && stairsToRevealIfStandalone == null)
        {
            Debug.LogWarning($"[LevelButton:{name}] Nessun manager registrato e 'Stairs To Reveal If Standalone' è vuoto: questo bottone non farà nulla quando premuto.");
        }
    }

    public void SetManager(ButtonPuzzleManager puzzleManager)
    {
        manager = puzzleManager;
        Debug.Log($"[LevelButton:{name}] Registrato su ButtonPuzzleManager '{puzzleManager.name}'.");
    }

    public void Press()
    {
        if (isPressed)
        {
            Debug.Log($"[LevelButton:{name}] Press() ignorato: già premuto in precedenza.");
            return;
        }
        isPressed = true;

        FMODEvents.Instance.ButtonSound();

        Debug.Log($"[LevelButton:{name}] Press() eseguito. Manager assegnato: {(manager != null)}. Stairs standalone assegnato: {(stairsToRevealIfStandalone != null)}.");

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
            {
                stairsComponent.Unlock();
            }
            else
            {
                Debug.LogError($"[LevelButton:{name}] '{stairsToRevealIfStandalone.name}' non ha un componente Stairs!");
            }
        }
        else
        {
            Debug.LogError($"[LevelButton:{name}] Press() eseguito ma non c'è NÉ un manager NÉ 'stairsToRevealIfStandalone' assegnato. Vai nell'Inspector di questo bottone e controlla i riferimenti.");
        }
    }
}