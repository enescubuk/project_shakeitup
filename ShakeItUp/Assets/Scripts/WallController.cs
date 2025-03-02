using UnityEngine;
using DG.Tweening;

public class WallController : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log(111);
        if (other.gameObject.tag == "Block")
        {
        Debug.Log(2222);
            if (gameObject.name.Contains("Left"))
            {
                transform.DOMoveX(transform.position.x - 7.5f, 1).SetEase(Ease.InOutElastic);
            }
            else
            {
                transform.DOMoveX(transform.position.x + 7.5f, 1).SetEase(Ease.InOutElastic);
            }
        }
    }
}
