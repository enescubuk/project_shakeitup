using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    public GameObject BlockPrefab;
    public List<GameObject> Blocks = new List<GameObject>();

    public float NextBlockDistance = 5f; // Blokların arasındaki mesafe

    void Start()
    {
        if (Blocks.Count > 1)
        {
            float initialDistance = Blocks[1].transform.position.z - Blocks[0].transform.position.z;
        }

        for (int i = 0; i < 15; i++)
        {
            SpawnNextBlock();
        }
    }

    public void SpawnNextBlock()
    {
        if (Blocks.Count == 0) return;

        // Son blok referansını al
        GameObject lastBlock = Blocks[Blocks.Count - 1];
        Vector3 lastBlockPos = lastBlock.transform.position;

        // 45 derecelik koni içinde rastgele bir açı seç (-22.5 ile 22.5 arasında)
        float randomAngle = Random.Range(-22.5f, 22.5f);

        // Açıyı radyana çevir
        float angleRad = randomAngle * Mathf.Deg2Rad;

        // Yeni bloğun konumunu hesapla
        float offsetX = Mathf.Sin(angleRad) * NextBlockDistance; // X ekseninde kayma
        float offsetZ = Mathf.Cos(angleRad) * NextBlockDistance; // Z ekseninde ileri hareket

        Vector3 newBlockPos = new Vector3(
            lastBlockPos.x + offsetX, 
            lastBlockPos.y, 
            lastBlockPos.z + offsetZ
        );

        // Yeni bloğun yönünü ayarla (gittiği açıyı baz alarak)
        Quaternion newBlockRotation = Quaternion.LookRotation(new Vector3(offsetX, 0, offsetZ));

        // Yeni bloğu oluştur ve listeye ekle
        GameObject newBlock = Instantiate(BlockPrefab, newBlockPos, newBlockRotation);
        Blocks.Add(newBlock);
    }
}
