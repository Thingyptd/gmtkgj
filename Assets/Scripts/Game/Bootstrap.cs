using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    void Start()
    {
        if (GameSession.Instance != null && GameSession.Instance.floorSceneNames.Count > 0)
        {
            SceneManager.LoadScene(GameSession.Instance.floorSceneNames[0]);
        }
        else
        {
            Debug.LogError("GameSession non configurato correttamente: nessun piano da caricare.");
        }
    }
}