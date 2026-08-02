using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Identità")]
    public string characterName = "Character";
    public Color characterColor = Color.white;
    public Sprite characterSprite;
    public Sprite idleFrame1;
    public Sprite idleFrame2;

    [Header("Selezione Squadra")]
    public Sprite selectionIcon;

    [Header("Sneak Animation")]
    public Sprite sneakFrame1;
    public Sprite sneakFrame2;
    public float sneakFrameDuration = 0.08f;

    [Header("Statistiche")]
    public int moveRange = 5;
    public bool canPushBoulders = false;
    public bool canFlyOverPits = false;
    public bool isImmuneToTraps = false;
}