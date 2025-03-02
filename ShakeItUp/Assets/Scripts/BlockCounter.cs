using UnityEngine;

public class BlockCounter : MonoBehaviour
{
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Block"))
        {
            hit.gameObject.GetComponentInParent<PlatformManager>().ControlBlock(hit.gameObject);
        }

        if (hit.collider.CompareTag("Plane"))
        {
            int sceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
        }
    }
}