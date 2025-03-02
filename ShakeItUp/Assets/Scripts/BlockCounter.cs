using UnityEngine;

public class BlockCounter : MonoBehaviour
{
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Block"))
        {
            Debug.Log(hit.gameObject.transform.parent);
            hit.gameObject.GetComponentInParent<PlatformManager>().ControlBlock(hit.gameObject);
        }
    }
}
