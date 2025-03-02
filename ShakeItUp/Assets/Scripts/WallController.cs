using UnityEngine;
using DG.Tweening;

public class WallController : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Block")
        {
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
