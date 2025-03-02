using UnityEngine;

public class BlockCounter : MonoBehaviour
{
void OnControllerColliderHit(ControllerColliderHit hit)
{
    // Yalnızca aşağı doğru olan çarpışmaları kabul et
    if (Vector3.Dot(hit.normal, Vector3.up) > 0.7f) // 0.7, karakterin blokun üstüne basmasını garanti eder
    {
        if (hit.collider.CompareTag("Block"))
        {
            hit.gameObject.GetComponentInParent<PlatformManager>().ControlBlock(hit.gameObject);
            Debug.Log(11111111111111111111);    
        }
    }

    // Eğer Plane'e çarptıysa, sahneyi yeniden başlat
    if (hit.collider.CompareTag("Plane"))
    {
        int sceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }
}


    
}