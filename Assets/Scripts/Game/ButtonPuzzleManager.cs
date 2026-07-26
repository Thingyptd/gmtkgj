using System.Collections.Generic;
using UnityEngine;

public class ButtonPuzzleManager : MonoBehaviour
{
    public List<LevelButton> buttons = new List<LevelButton>();
    public GameObject stairsToReveal;

    private int pressedCount = 0;

    void Awake()
    {
        if (stairsToReveal != null)
            stairsToReveal.SetActive(false);

        foreach (var button in buttons)
        {
            if (button != null)
                button.SetManager(this);
        }
    }

    public void NotifyButtonPressed()
    {
        pressedCount++;

        if (pressedCount >= buttons.Count && stairsToReveal != null)
        {
            stairsToReveal.SetActive(true);

            Stairs stairsComponent = stairsToReveal.GetComponent<Stairs>();
            if (stairsComponent != null)
                stairsComponent.Unlock();
        }
    }
}