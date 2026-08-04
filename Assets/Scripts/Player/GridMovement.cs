using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class GridMovement : MonoBehaviour
{
    public Grid grid;
    public Tilemap collisionTilemap;
    public Tilemap pitsTilemap;

    public bool instantMove = true;
    public float moveSpeed = 10f;

    public bool allowHoldRepeat = false;
    public float repeatInterval = 0.15f;

    public float fallDuration = 0.4f;

    public float teeterDuration = 0.6f;
    [Range(0f, 1f)] public float teeterLeanRatio = 0.45f;
    public float cancelReturnDuration = 0.15f;

    private bool isTeetering = false;

    public SpriteRenderer spriteRenderer;
    private MovementAnimations movementAnimations;

    public CharacterData data;
    public int movesRemaining;

    public event Action<GridMovement> OnMovesExhausted;
    public event Action<GridMovement> OnFellIntoPit;
    public event Action<Vector3> OnGroundCellEntered;
    public event Action<int, int> OnMovesChanged;
    public event Action<GridMovement> OnStairsEntered;

    private PlayerControls controls;
    private List<Vector2Int> heldDirections = new List<Vector2Int>();

    private bool isMoving = false;
    private Vector3 targetPosition;
    private float repeatTimer;
    private bool isDead = false;

    public bool IsFacingLeft => spriteRenderer != null && spriteRenderer.flipX;

    void Awake()
    {
        controls = new PlayerControls();

        if (grid == null)
            grid = FindAnyObjectByType<Grid>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        movementAnimations = GetComponent<MovementAnimations>();
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    public void Initialize(CharacterData characterData, Vector3 spawnWorldPos, Tilemap walls, Tilemap pits, bool initialFacingLeft)
    {
        data = characterData;
        movesRemaining = data.moveRange;
        collisionTilemap = walls;
        pitsTilemap = pits;
        isDead = false;
        transform.localScale = Vector3.one;

        OnMovesChanged?.Invoke(movesRemaining, data.moveRange);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = data.characterColor;
            spriteRenderer.flipX = initialFacingLeft;
        }

        if (movementAnimations != null)
        {
            movementAnimations.SetIdleFrames(data.idleFrame1, data.idleFrame2);
            movementAnimations.SetSneakFrames(data.sneakFrame1, data.sneakFrame2, data.sneakFrameDuration);
        }

        targetPosition = SnapToGridCenter(spawnWorldPos);
        transform.position = targetPosition;

        Vector3Int spawnCell = grid.WorldToCell(transform.position);
        bool spawnIsPit = pitsTilemap != null && pitsTilemap.HasTile(spawnCell);

        if (spawnIsPit && !data.canFlyOverPits)
        {
            isDead = true;
            StartCoroutine(FallAndRecover());
        }
        else if (!spawnIsPit)
        {
            OnGroundCellEntered?.Invoke(transform.position);
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (PauseMenuUI.Instance != null)
                PauseMenuUI.Instance.TogglePause();
        }

        if (isTeetering)
            return;

        if (!isDead)
            HandleInputEdges();

        HandleMovement();
    }

    private void HandleInputEdges()
    {
        CheckDirection(controls.Player.MoveUp, Vector2Int.up);
        CheckDirection(controls.Player.MoveDown, Vector2Int.down);
        CheckDirection(controls.Player.MoveLeft, Vector2Int.left);
        CheckDirection(controls.Player.MoveRight, Vector2Int.right);
    }

    private void CheckDirection(InputAction action, Vector2Int direction)
    {
        if (action.WasPressedThisFrame())
        {
            heldDirections.Remove(direction);
            heldDirections.Add(direction);

            UpdateFacing(direction);

            if (!isMoving)
            {
                TryMove(direction);
                repeatTimer = repeatInterval;
            }
        }
        else if (action.WasReleasedThisFrame())
        {
            heldDirections.Remove(direction);
        }
    }

    private void HandleMovement()
    {
        if (!isMoving)
        {
            if (!isDead && allowHoldRepeat && heldDirections.Count > 0)
            {
                repeatTimer -= Time.deltaTime;
                if (repeatTimer <= 0f)
                {
                    Vector2Int dir = heldDirections[heldDirections.Count - 1];
                    TryMove(dir);
                    repeatTimer = repeatInterval;
                }
            }
        }
        else if (!instantMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }

    private void TryMove(Vector2Int direction)
    {
        if (isDead || isTeetering || movesRemaining <= 0) return;

        Vector3Int currentCell = grid.WorldToCell(transform.position);
        Vector3Int nextCell = currentCell + new Vector3Int(direction.x, direction.y, 0);

        if (!CanEnterCell(nextCell))
            return;

        Boulder boulder = FindBoulderAt(nextCell);
        if (boulder != null)
        {
            if (data == null || !data.canPushBoulders)
                return;

            Vector3Int beyondCell = nextCell + new Vector3Int(direction.x, direction.y, 0);

            bool beyondBlockedHard =
                (collisionTilemap != null && collisionTilemap.HasTile(beyondCell)) ||
                FindBoulderAt(beyondCell) != null;

            if (beyondBlockedHard)
                return;

            bool beyondIsPit = pitsTilemap != null && pitsTilemap.HasTile(beyondCell);

            if (beyondIsPit)
                boulder.FallIntoPit();
            else
                boulder.MoveToCell(beyondCell);

            movesRemaining--;
            OnMovesChanged?.Invoke(movesRemaining, data.moveRange);

            if (movementAnimations != null)
                movementAnimations.PlayMoveParticle(transform.position);

            if (movesRemaining <= 0)
            {
                isDead = true;
                OnMovesExhausted?.Invoke(this);
            }

            return;
        }

        bool isPit = pitsTilemap != null && pitsTilemap.HasTile(nextCell);
        bool willFall = isPit && !data.canFlyOverPits;

        if (willFall)
        {
            StartCoroutine(TeeterAndFall(direction, nextCell));
            return;
        }

        if (movementAnimations != null)
            movementAnimations.PlayMoveParticle(transform.position);

        Vector3 destination = grid.GetCellCenterWorld(nextCell);
        destination.z = transform.position.z;
        targetPosition = destination;

        if (instantMove)
            transform.position = targetPosition;
        else
            isMoving = true;

        movesRemaining--;
        OnMovesChanged?.Invoke(movesRemaining, data.moveRange);

        if (!isPit)
            OnGroundCellEntered?.Invoke(destination);

        LevelButton levelButton = FindButtonAt(nextCell);
        if (levelButton != null)
        {
            levelButton.Press();
        }

        Stairs stairs = FindStairsAt(nextCell);
        if (stairs != null && stairs.IsUnlocked)
        {
            if (movementAnimations != null)
                movementAnimations.StopSneaking();

            OnStairsEntered?.Invoke(this);
            FMODEvents.Instance.PlayStartSound();
            return;
        }

        DeathTrap trap = FindTrapAt(nextCell);
        if (trap != null && (data == null || !data.isImmuneToTraps))
        {
            trap.Trigger();
            isDead = true;
            OnMovesExhausted?.Invoke(this);
            return;
        }

        if (movementAnimations != null)
        {
            if (trap != null)
                movementAnimations.StartSneaking();
            else
                movementAnimations.StopSneaking();
        }

        if (movesRemaining <= 0)
        {
            isDead = true;
            OnMovesExhausted?.Invoke(this);
        }
    }

    private IEnumerator TeeterAndFall(Vector2Int direction, Vector3Int nextCell)
    {
        isTeetering = true;

        Vector3 originalPos = transform.position;
        Vector3 pitCenter = grid.GetCellCenterWorld(nextCell);
        pitCenter.z = originalPos.z;

        Vector3 edgeTarget = Vector3.Lerp(originalPos, pitCenter, teeterLeanRatio);

        InputAction oppositeAction = GetActionForDirection(-direction);

        float t = 0f;
        bool cancelled = false;

        while (t < teeterDuration)
        {
            if (oppositeAction != null && oppositeAction.WasPressedThisFrame())
            {
                cancelled = true;
                break;
            }

            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / teeterDuration);
            transform.position = Vector3.Lerp(originalPos, edgeTarget, p);
            yield return null;
        }
        

        if (cancelled)
        {
            yield return StartCoroutine(LerpPosition(transform.position, originalPos, cancelReturnDuration));
            transform.position = originalPos;
            targetPosition = originalPos;
            isTeetering = false;
            yield break;
        }
        FMODEvents.Instance.PlayFallSound();
        transform.position = edgeTarget;
        targetPosition = pitCenter;

        movesRemaining--;
        OnMovesChanged?.Invoke(movesRemaining, data.moveRange);

        isTeetering = false;
        isDead = true;
        StartCoroutine(FallAndRecover());
    }

    private IEnumerator LerpPosition(Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(from, to, t / duration);
            yield return null;
        }
        transform.position = to;
    }

    private InputAction GetActionForDirection(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return controls.Player.MoveUp;
        if (dir == Vector2Int.down) return controls.Player.MoveDown;
        if (dir == Vector2Int.left) return controls.Player.MoveLeft;
        if (dir == Vector2Int.right) return controls.Player.MoveRight;
        return null;
    }

    private IEnumerator FallAndRecover()
    {
        float t = 0f;
        Vector3 startScale = transform.localScale;

        while (t < fallDuration)
        {
            t += Time.deltaTime;
            float p = t / fallDuration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, p);
            yield return null;
        }

        transform.localScale = Vector3.zero;
        OnFellIntoPit?.Invoke(this);
    }

    public void RecoverFromFall(Vector3 groundWorldPos)
    {
        Vector3 snapped = SnapToGridCenter(groundWorldPos);
        transform.position = snapped;
        targetPosition = snapped;
        transform.localScale = Vector3.one;
        isMoving = false;
        isDead = false;

        if (movementAnimations != null)
            movementAnimations.StopSneaking();

        OnGroundCellEntered?.Invoke(snapped);

        if (movesRemaining <= 0)
        {
            isDead = true;
            OnMovesExhausted?.Invoke(this);
        }
    }

    private bool CanEnterCell(Vector3Int cell)
    {
        if (collisionTilemap != null && collisionTilemap.HasTile(cell))
            return false;

        return true;
    }

    private Boulder FindBoulderAt(Vector3Int cell)
    {
        Vector3 worldPos = grid.GetCellCenterWorld(cell);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        return hit != null ? hit.GetComponent<Boulder>() : null;
    }

    private DeathTrap FindTrapAt(Vector3Int cell)
    {
        Vector3 worldPos = grid.GetCellCenterWorld(cell);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        return hit != null ? hit.GetComponent<DeathTrap>() : null;
    }

    private Stairs FindStairsAt(Vector3Int cell)
    {
        Vector3 worldPos = grid.GetCellCenterWorld(cell);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        return hit != null ? hit.GetComponent<Stairs>() : null;
    }

    private LevelButton FindButtonAt(Vector3Int cell)
    {
        Vector3 worldPos = grid.GetCellCenterWorld(cell);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        return hit != null ? hit.GetComponent<LevelButton>() : null;
    }

    private Vector3 SnapToGridCenter(Vector3 worldPos)
    {
        Vector3Int cell = grid.WorldToCell(worldPos);
        Vector3 center = grid.GetCellCenterWorld(cell);
        center.z = worldPos.z;
        return center;
    }

    public Vector3 GetCurrentWorldPosition() => transform.position;

    public Vector3 GetTargetWorldPosition() => targetPosition;

    public void ApplyFloorTransition(Tilemap newWalls, Tilemap newPits, Vector3 spawnWorldPos)
    {
        collisionTilemap = newWalls;
        pitsTilemap = newPits;

        Vector3 snapped = SnapToGridCenter(spawnWorldPos);
        transform.position = snapped;
        targetPosition = snapped;
        isMoving = false;

        OnGroundCellEntered?.Invoke(snapped);
    }

    private void UpdateFacing(Vector2Int direction)
    {
        if (spriteRenderer == null) return;
        if (direction.x == 0) return;

        spriteRenderer.flipX = direction.x > 0;
    }

    public void SetInitialFacing(bool facingLeft)
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = facingLeft;
    }
}