using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class MultiMountainMover : MonoBehaviour
{
    public Terrain terrain;
    public float maxMountainHeight = 0.3f;
    public int mountainSize = 50;
    public float heightChangeSpeed = 0.1f;
    public float gaussianSpread = 10f;
    public float smoothingDuration = 0.5f;

    private Dictionary<Transform, float> heightOffsets = new Dictionary<Transform, float>();
    private Dictionary<Transform, Tweener> tweenerDict = new Dictionary<Transform, Tweener>();
    private List<Transform> trackedObjects = new List<Transform>();
    private Dictionary<Transform, Vector3> previousPositions = new Dictionary<Transform, Vector3>();

    void Start()
    {
        ResetTerrain();
        FindChildObjects();
        InitializeHeightOffsets();
    }

    void FixedUpdate()
    {
        foreach (var obj in trackedObjects)
        {
            ResetPreviousHeight(obj);
            ApplyHeightChange(obj);
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            foreach (var obj in trackedObjects)
                AdjustHeight(obj, true);
        }
        if (Input.GetKey(KeyCode.S))
        {
            foreach (var obj in trackedObjects)
                AdjustHeight(obj, false);
        }
    }

    void FindChildObjects()
    {
        trackedObjects.Clear();
        foreach (Transform child in transform)
        {
            trackedObjects.Add(child);
            previousPositions[child] = child.position; // İlk pozisyonları kaydet
        }
    }

    void InitializeHeightOffsets()
    {
        foreach (var obj in trackedObjects)
        {
            heightOffsets[obj] = 0f;
        }
    }

    public void AdjustHeight(Transform obj, bool increase)
    {
        if (!heightOffsets.ContainsKey(obj)) return;

        float targetHeight = heightOffsets[obj] + (increase ? heightChangeSpeed : -heightChangeSpeed);

        if (tweenerDict.ContainsKey(obj))
            tweenerDict[obj].Kill();

        tweenerDict[obj] = DOTween.To(() => heightOffsets[obj], x => heightOffsets[obj] = x, Mathf.Clamp(targetHeight, 0f, maxMountainHeight), smoothingDuration);
    }

    void ApplyHeightChange(Transform obj)
    {
        TerrainData terrainData = terrain.terrainData;
        int terrainWidth = terrainData.heightmapResolution;
        int terrainHeight = terrainData.heightmapResolution;

        int x = Mathf.RoundToInt((obj.position.x / terrainData.size.x) * terrainWidth);
        int z = Mathf.RoundToInt((obj.position.z / terrainData.size.z) * terrainHeight);

        float[,] heights = terrainData.GetHeights(x - mountainSize / 2, z - mountainSize / 2, mountainSize, mountainSize);

        for (int i = 0; i < mountainSize; i++)
        {
            for (int j = 0; j < mountainSize; j++)
            {
                float dx = (i - mountainSize / 2) / (float)(mountainSize / 2);
                float dz = (j - mountainSize / 2) / (float)(mountainSize / 2);
                float distance = Mathf.Sqrt(dx * dx + dz * dz);

                float circularFactor = Mathf.Cos(distance * Mathf.PI / 2);
                circularFactor = Mathf.Clamp01(circularFactor);

                float gaussianFactor = Mathf.Exp(-distance * distance / (2 * gaussianSpread * gaussianSpread));

                heights[i, j] = Mathf.Clamp(gaussianFactor * circularFactor * heightOffsets[obj], 0, 1);
            }
        }

        terrainData.SetHeights(x - mountainSize / 2, z - mountainSize / 2, heights);

        // Yeni pozisyonu kaydet
        previousPositions[obj] = obj.position;
    }

    void ResetPreviousHeight(Transform obj)
    {
        if (!previousPositions.ContainsKey(obj)) return;

        TerrainData terrainData = terrain.terrainData;
        int terrainWidth = terrainData.heightmapResolution;
        int terrainHeight = terrainData.heightmapResolution;

        int x = Mathf.RoundToInt((previousPositions[obj].x / terrainData.size.x) * terrainWidth);
        int z = Mathf.RoundToInt((previousPositions[obj].z / terrainData.size.z) * terrainHeight);

        float[,] heights = terrainData.GetHeights(x - mountainSize / 2, z - mountainSize / 2, mountainSize, mountainSize);

        // Önceki yüksekliği sıfırla
        for (int i = 0; i < mountainSize; i++)
        {
            for (int j = 0; j < mountainSize; j++)
            {
                heights[i, j] = 0f;
            }
        }

        terrainData.SetHeights(x - mountainSize / 2, z - mountainSize / 2, heights);
    }

    void ResetTerrain()
    {
        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;

        float[,] resetHeights = new float[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                resetHeights[i, j] = 0f;
            }
        }

        terrainData.SetHeights(0, 0, resetHeights);
    }
}
