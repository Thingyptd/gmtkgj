using System.Collections.Generic;
using UnityEngine;

public class ButtonPuzzleManager : MonoBehaviour
{
    public List<LevelButton> buttons = new List<LevelButton>();
    public Stairs stairsToUnlock;

    private int pressedCount = 0;

    void Awake()
    {
        if (buttons.Count == 0)
        {
            Debug.LogWarning($"[ButtonPuzzleManager:{name}] La lista 'Buttons' è vuota! Nessun bottone registrato, le scale non si sbloccheranno mai.");
        }

        if (stairsToUnlock == null)
        {
            Debug.LogWarning($"[ButtonPuzzleManager:{name}] 'Stairs To Unlock' non è assegnato!");
        }

        foreach (var button in buttons)
        {
            if (button != null)
            {
                button.SetManager(this);
            }
            else
            {
                Debug.LogWarning($"[ButtonPuzzleManager:{name}] Un elemento della lista 'Buttons' è None (slot vuoto).");
            }
        }
    }

    public void NotifyButtonPressed()
    {
        pressedCount++;

        if (pressedCount >= buttons.Count && stairsToUnlock != null)
        {
            stairsToUnlock.Unlock();
        }
    }
}