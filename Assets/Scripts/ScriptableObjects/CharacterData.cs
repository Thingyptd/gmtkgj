using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Identità")]
    public string characterName = "Character";
    public Color characterColor = Color.white;

    public Sprite idleFrame1;
    public Sprite idleFrame2;

    [Header("Statistiche")]
    public int moveRange = 5;

    public bool canPushBoulders = false;

    public bool canFlyOverPits = false;

    public bool isImmuneToTraps = false;
}