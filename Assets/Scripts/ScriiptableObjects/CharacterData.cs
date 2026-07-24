using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Identità")]
    public string characterName = "Character";
    public Color characterColor = Color.white;

    [Header("Statistiche")]
    [Tooltip("Numero di caselle che questo personaggio può percorrere prima di morire")]
    public int moveRange = 5;

    [Tooltip("Può spingere i massi (Boulder) incontrati sul percorso")]
    public bool canPushBoulders = false;

    [Tooltip("Può muoversi sopra le celle 'precipizio' senza cadere")]
    public bool canFlyOverPits = false;

    public bool isImmuneToTraps = false;
}