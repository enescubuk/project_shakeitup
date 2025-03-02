using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; 

public class PlatformManager : MonoBehaviour
{
    public GameObject BlockPrefab;
    public List<GameObject> Blocks = new List<GameObject>();

    public float NextBlockDistance;
    public int currentBlockIndex = 0;

    public GameObject Player;
    private MovementController playerMovement;

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        if (Player != null)
        {
            playerMovement = Player.GetComponent<MovementController>();
        }

        NextBlockDistance += BlockPrefab.transform.localScale.z;

        for (int i = 0; i < 300; i++)
        {
            SpawnFirstBlocks();
        }
    }

    private void SpawnFirstBlocks()
    {
        Vector3 spawnPos = Blocks.Count > 0 ? Blocks[Blocks.Count - 1].transform.position : transform.position;

        float randomAngle = Random.Range(-22.5f, 22.5f);
        float angleRad = randomAngle * Mathf.Deg2Rad;

        float offsetX = Mathf.Sin(angleRad) * NextBlockDistance;
        float offsetZ = Mathf.Cos(angleRad) * NextBlockDistance;

        Vector3 newBlockPos = new Vector3(spawnPos.x + offsetX, spawnPos.y, spawnPos.z + offsetZ);
        Quaternion newBlockRotation = Quaternion.LookRotation(new Vector3(offsetX, 0, offsetZ));

        GameObject newBlock = Instantiate(BlockPrefab, newBlockPos, newBlockRotation, transform);
        Blocks.Add(newBlock);
    }

    private void SetNextBlock()
    {

        GameObject referenceBlock = Blocks[Blocks.Count - 1];
        Vector3 referencePos = referenceBlock.transform.position;

        float randomAngle = Random.Range(-22.5f, 22.5f);
        float angleRad = randomAngle * Mathf.Deg2Rad;

        float offsetX = Mathf.Sin(angleRad) * NextBlockDistance;
        float offsetZ = Mathf.Cos(angleRad) * NextBlockDistance;

        Vector3 newBlockPos = new Vector3(referencePos.x + offsetX, referencePos.y, referencePos.z + offsetZ);
        Quaternion newBlockRotation = Quaternion.LookRotation(new Vector3(offsetX, 0, offsetZ));

        GameObject recycledBlock = Blocks[0];
        recycledBlock.transform.position = newBlockPos;
        recycledBlock.transform.rotation = newBlockRotation;

        Blocks.RemoveAt(0);
        Blocks.Add(recycledBlock);
    }

    internal void ControlBlock(GameObject _block)
{
    for (int i = 0; i < Blocks.Count; i++)
    {
        if (Blocks[i] == _block)
        {
            if (currentBlockIndex != i)
            {
                currentBlockIndex = i;
                if (currentBlockIndex != 0)
                {
                    SetNextBlock();
                }

                // **Yanıp sönme efekti ekle**
                FlashBlock(_block);
            }
            break;
        }
    }
}

private void FlashBlock(GameObject block)
{
    MeshRenderer renderer = block.GetComponent<MeshRenderer>();

    if (renderer != null)
    {
        Color originalColor = renderer.material.color;

        // **Rengi yarım saniye boyunca transparan yap**
        renderer.material.DOColor(new Color(originalColor.r, originalColor.g, originalColor.b, 0), 0.25f)
            .SetLoops(2, LoopType.Yoyo) // 2 kez ileri geri oynasın (yanıp sönme)
            .OnComplete(() =>
            {
                // **Orijinal renge geri dön**
                renderer.material.color = originalColor;
            });
    }
}


    private void Update()
    {
        if (playerMovement == null) return;

        float currentSpeed = playerMovement.GetCurrentSpeed();
        AdjustNextBlockDistance(currentSpeed);
    }

    private void AdjustNextBlockDistance(float speed)
    {
        if (speed > 21.5f)
        {
            NextBlockDistance = 14f;
        }
        else if (speed > 17.5f)
        {
            NextBlockDistance = 12.5f;
        }
        else if (speed > 12.5f)
        {
            NextBlockDistance = 10f;
        }
        else
        {
            NextBlockDistance = 9f;
        }
    }
}
