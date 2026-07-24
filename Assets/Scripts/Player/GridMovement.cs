using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class GridMovement : MonoBehaviour
{
    [Header("Grid & Tilemaps")]
    public Grid grid;
    public Tilemap collisionTilemap; // muri: sempre bloccanti
    public Tilemap pitsTilemap;      // precipizi: letali (non bloccanti) per chi non vola

    [Header("Movement Settings")]
    public bool instantMove = true;
    public float moveSpeed = 10f;

    [Header("Input")]
    public bool allowHoldRepeat = false;
    public float repeatInterval = 0.15f;

    [Header("Fall Animation")]
    public float fallDuration = 0.4f;

    [Header("Pit Teeter")]
    [Tooltip("Durata dello scivolamento verso il bordo: è anche la finestra di tempo per annullare")]
    public float teeterDuration = 0.6f;
    [Tooltip("Quanto il personaggio si avvicina al pit prima di cadere (0 = resta fermo, 1 = arriva già al centro del pit)")]
    [Range(0f, 1f)] public float teeterLeanRatio = 0.45f;
    [Tooltip("Durata dell'animazione di ritorno se annulli in tempo")]
    public float cancelReturnDuration = 0.15f;

    private bool isTeetering = false;

    [Header("Visual")]
    [Tooltip("Trascina qui manualmente lo SpriteRenderer figlio")]
    public SpriteRenderer spriteRenderer;

    [Header("Runtime Data (impostati da CharacterManager)")]
    public CharacterData data;
    public int movesRemaining;

    // Vera morte per esaurimento mosse: il Manager farà subentrare il prossimo personaggio
    public event Action<GridMovement> OnMovesExhausted;

    // Caduta in un pit: incidente RECUPERABILE, stesso personaggio, il Manager deve riportarlo a terra
    public event Action<GridMovement> OnFellIntoPit;

    // Invocato ogni volta che QUALSIASI personaggio tocca una cella di terra (non-pit):
    // serve al Manager per tracciare l'ultima posizione sicura condivisa tra tutti i personaggi
    public event Action<Vector3> OnGroundCellEntered;

    public event Action<int, int> OnMovesChanged;

    public event Action<GridMovement> OnStairsEntered;

    private PlayerControls controls;

    // Stack delle direzioni attualmente premute, in ordine di pressione (l'ultima è la più recente)
    private List<Vector2Int> heldDirections = new List<Vector2Int>();

    private bool isMoving = false;
    private Vector3 targetPosition;
    private float repeatTimer;
    private bool isDead = false;

    void Awake()
    {
        controls = new PlayerControls();

        if (grid == null)
            grid = FindAnyObjectByType<Grid>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
            Debug.LogError($"[{name}] GridMovement non trova nessuno SpriteRenderer nei figli! Il colore del personaggio non verrà applicato.", this);
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    /// <summary>
    /// Inizializza il personaggio con i dati del CharacterData e lo posiziona sulla griglia.
    /// Chiamato da CharacterManager subito dopo l'Instantiate.
    /// </summary>
    public void Initialize(CharacterData characterData, Vector3 spawnWorldPos, Tilemap walls, Tilemap pits)
    {
        data = characterData;
        movesRemaining = data.moveRange;
        collisionTilemap = walls;
        pitsTilemap = pits;
        isDead = false;
        transform.localScale = Vector3.one;

        OnMovesChanged?.Invoke(movesRemaining, data.moveRange);

        if (spriteRenderer != null)
            spriteRenderer.color = data.characterColor;

        targetPosition = SnapToGridCenter(spawnWorldPos);
        transform.position = targetPosition;

        // Controllo immediato: se il personaggio nasce già su un pit e non vola, cade subito
        // (caso: il predecessore è morto sopra un pit e il successivo eredita quella posizione)
        Vector3Int spawnCell = grid.WorldToCell(transform.position);
        bool spawnIsPit = pitsTilemap != null && pitsTilemap.HasTile(spawnCell);

        if (spawnIsPit && !data.canFlyOverPits)
        {
            isDead = true; // blocca input durante l'animazione, non è una morte definitiva
            StartCoroutine(FallAndRecover());
        }
        else if (!spawnIsPit)
        {
            OnGroundCellEntered?.Invoke(transform.position);
        }
        // se spawnIsPit ma il personaggio vola, resta lì semplicemente, nessun aggiornamento del ground
    }

    void Update()
    {
        if (isDead) return;

        if (isTeetering)
            return; // durante il bilico, la coroutine gestisce tutto l'input rilevante

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
            // Rimuovi eventuali duplicati e metti questa direzione in cima allo stack
            heldDirections.Remove(direction);
            heldDirections.Add(direction);

            // Un input "nuovo" prova subito un movimento (se non stiamo già muovendoci)
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
            // Hold-repeat: usa la direzione più recente ancora tenuta premuta
            if (allowHoldRepeat && heldDirections.Count > 0)
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

        if (!CanEnterCell(nextCell, direction))
            return; // bloccato da muro o masso non spingibile: non consuma mosse

        bool isPit = pitsTilemap != null && pitsTilemap.HasTile(nextCell);
        bool willFall = isPit && !data.canFlyOverPits;

        if (willFall)
        {
            StartCoroutine(TeeterAndFall(direction, nextCell));
            return; // spostamento e consumo mossa gestiti dentro la coroutine, SOLO se non annullato
        }

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

        DeathTrap trap = FindTrapAt(nextCell);
        if (trap != null && (data == null || !data.isImmuneToTraps))
        {
            trap.Trigger();
            isDead = true;
            OnMovesExhausted?.Invoke(this);
            return;
        }

        if (movesRemaining <= 0)
        {
            isDead = true;
            OnMovesExhausted?.Invoke(this);
        }

        Stairs stairs = FindStairsAt(nextCell);
        if (stairs != null)
        {
            OnStairsEntered?.Invoke(this);
        }
    }

    private IEnumerator TeeterAndFall(Vector2Int direction, Vector3Int nextCell)
    {
        isTeetering = true;

        Vector3 originalPos = transform.position;
        Vector3 pitCenter = grid.GetCellCenterWorld(nextCell);
        pitCenter.z = originalPos.z;

        // Il punto "limite" verso cui il personaggio scivola: non necessariamente il centro del pit,
        // ma quanto definito da teeterLeanRatio (es. 0.45 = quasi a metà tra le due celle)
        Vector3 edgeTarget = Vector3.Lerp(originalPos, pitCenter, teeterLeanRatio);

        InputAction oppositeAction = GetActionForDirection(-direction);

        float t = 0f;
        bool cancelled = false;

        // Scivolamento continuo: la posizione avanza gradualmente verso edgeTarget,
        // frame per frame controlliamo se arriva l'input opposto per annullare
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
            // Torna indietro dalla posizione ATTUALE (parziale, dove si trovava quando hai premuto) fino all'origine
            yield return StartCoroutine(LerpPosition(transform.position, originalPos, cancelReturnDuration));
            transform.position = originalPos;
            targetPosition = originalPos;
            isTeetering = false;
            yield break; // nessuna mossa consumata, il personaggio riprende il controllo normale
        }

        // Scivolamento completato senza annullare: qui avviene la vera caduta
        transform.position = edgeTarget;
        targetPosition = pitCenter;

        movesRemaining--;
        OnMovesChanged?.Invoke(movesRemaining, data.moveRange);

        isTeetering = false;
        isDead = true; // blocca input durante l'animazione di caduta vera e propria (shrink)
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

        // Segnala al Manager: "sono caduto, riportami all'ultima cella di terra sicura"
        OnFellIntoPit?.Invoke(this);
    }

    /// <summary>
    /// Chiamato dal Manager dopo aver determinato l'ultima cella di terra sicura.
    /// Ripristina posizione e scala, riabilita l'input, SENZA restituire la mossa persa nella caduta.
    /// </summary>
    public void RecoverFromFall(Vector3 groundWorldPos)
    {
        Vector3 snapped = SnapToGridCenter(groundWorldPos);
        transform.position = snapped;
        targetPosition = snapped;
        transform.localScale = Vector3.one;
        isMoving = false;
        isDead = false;

        // La cella di recupero è per definizione terra: aggiorna comunque il tracking globale
        OnGroundCellEntered?.Invoke(snapped);

        // Se il personaggio era arrivato a 0 mosse proprio a causa della caduta,
        // ora che è "salvo" è comunque a fine corsa: scatta la vera morte.
        if (movesRemaining <= 0)
        {
            isDead = true;
            OnMovesExhausted?.Invoke(this);
        }
    }

    /// <summary>
    /// Verifica se il personaggio può entrare in una cella, considerando muri e massi.
    /// I pit NON bloccano l'ingresso: sono letali/recuperabili, non invalicabili.
    /// </summary>
    private bool CanEnterCell(Vector3Int cell, Vector2Int direction)
    {
        // Muri: sempre bloccanti, nessun potere li supera
        if (collisionTilemap != null && collisionTilemap.HasTile(cell))
            return false;

        Boulder boulder = FindBoulderAt(cell);
        if (boulder != null)
        {
            if (data == null || !data.canPushBoulders)
                return false;

            Vector3Int beyondCell = cell + new Vector3Int(direction.x, direction.y, 0);

            // Ostacoli VERI oltre il masso: muro o altro masso bloccano comunque la spinta
            bool beyondBlockedHard =
                (collisionTilemap != null && collisionTilemap.HasTile(beyondCell)) ||
                FindBoulderAt(beyondCell) != null;

            if (beyondBlockedHard)
                return false;

            bool beyondIsPit = pitsTilemap != null && pitsTilemap.HasTile(beyondCell);

            if (beyondIsPit)
                boulder.FallIntoPit();   // il masso cade e viene distrutto
            else
                boulder.MoveToCell(beyondCell); // spinta normale su terra
        }

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

    private Vector3 SnapToGridCenter(Vector3 worldPos)
    {
        Vector3Int cell = grid.WorldToCell(worldPos);
        Vector3 center = grid.GetCellCenterWorld(cell);
        center.z = worldPos.z;
        return center;
    }

    /// <summary>Posizione attuale in world space, utile al Manager per spawn/tracking.</summary>
    public Vector3 GetCurrentWorldPosition() => transform.position;

    /// <summary>
    /// Applica un cambio di piano: aggiorna i riferimenti alle Tilemap e riposiziona il personaggio
    /// sullo spawn point del nuovo piano. Non consuma mosse, non è una morte.
    /// </summary>
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
}