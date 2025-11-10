using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Reloads the active scene when the player presses the R key.
/// Attach this to a persistent GameObject in the scene (for example GameManager or an empty object).
/// </summary>
public class RestartOnR : MonoBehaviour
{
    [Tooltip("Enable/disable restart with R key.")]
    public bool enableRestart = true;

    void Update()
    {
        if (!enableRestart) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            // reload the currently active scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
