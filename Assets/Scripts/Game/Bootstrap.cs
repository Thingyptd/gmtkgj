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
            Debug.LogError("Nessun piano da caricare");
        }
    }
}