using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Stairs : MonoBehaviour
{
    public Grid grid;
    public SpriteRenderer spriteRenderer;

    public bool startLocked = true;

    public Color lockedColor = new Color(.8f, .8f, .8f, 0.7f);
    public Color unlockedColor = Color.white;
    public float unlockFadeDuration = 0.3f;

    public bool IsUnlocked { get; private set; }

    void Awake()
    {
        if (grid == null)
            grid = FindAnyObjectByType<Grid>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError($"[Stairs:{name}] Nessuno SpriteRenderer trovato, né sull'oggetto stesso né nei figli! Le scale non saranno mai visibili.");
        }
        else if (spriteRenderer.sprite == null)
        {
            Debug.LogWarning($"[Stairs:{name}] Lo SpriteRenderer trovato non ha nessuno sprite assegnato.");
        }

        GetComponent<BoxCollider2D>().isTrigger = true;

        if (grid != null)
        {
            Vector3Int cell = grid.WorldToCell(transform.position);
            Vector3 center = grid.GetCellCenterWorld(cell);
            center.z = transform.position.z;
            transform.position = center;
        }

        IsUnlocked = !startLocked;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = IsUnlocked ? unlockedColor : lockedColor;
            Debug.Log($"[Stairs:{name}] Colore impostato: {spriteRenderer.color} (IsUnlocked={IsUnlocked})");
        }
    }

    public void Unlock()
    {
        if (IsUnlocked) return;

        IsUnlocked = true;

        if (spriteRenderer != null)
            spriteRenderer.DOColor(unlockedColor, unlockFadeDuration);
    }
}