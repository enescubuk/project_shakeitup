using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class MultiMountainMover : MonoBehaviour
{
    public List<Terrain> terrains = new List<Terrain>();
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
        FindTerrains();
        ResetAllTerrains();
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
            foreach (var obj in trackedObjects) AdjustHeight(obj, true);
        }
        if (Input.GetKey(KeyCode.S))
        {
            foreach (var obj in trackedObjects) AdjustHeight(obj, false);
        }
    }

    void FindTerrains()
    {
        terrains.AddRange(FindObjectsOfType<Terrain>());
    }

    void FindChildObjects()
    {
        trackedObjects.Clear();
        foreach (Transform child in transform)
        {
            trackedObjects.Add(child);
            previousPositions[child] = child.position;
        }
    }

    void InitializeHeightOffsets()
    {
        foreach (var obj in trackedObjects) heightOffsets[obj] = 0f;
    }

    Terrain GetTerrainAtPosition(Vector3 position)
    {
        foreach (var terrain in terrains)
        {
            Vector3 terrainPos = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;
            
            if (position.x >= terrainPos.x && position.x <= terrainPos.x + terrainSize.x &&
                position.z >= terrainPos.z && position.z <= terrainPos.z + terrainSize.z)
            {
                return terrain;
            }
        }
        return null;
    }

    public void AdjustHeight(Transform obj, bool increase)
    {
        if (!heightOffsets.ContainsKey(obj)) return;

        float targetHeight = heightOffsets[obj] + (increase ? heightChangeSpeed : -heightChangeSpeed);
        if (tweenerDict.ContainsKey(obj)) tweenerDict[obj].Kill();

        tweenerDict[obj] = DOTween.To(
            () => heightOffsets[obj],
            x => heightOffsets[obj] = x,
            Mathf.Clamp(targetHeight, 0f, maxMountainHeight),
            smoothingDuration
        );
    }

    void ApplyHeightChange(Transform obj)
    {
        Terrain terrain = GetTerrainAtPosition(obj.position);
        if (terrain == null) return;

        TerrainData terrainData = terrain.terrainData;
        int x = Mathf.RoundToInt((obj.position.x - terrain.transform.position.x) / terrainData.size.x * terrainData.heightmapResolution);
        int z = Mathf.RoundToInt((obj.position.z - terrain.transform.position.z) / terrainData.size.z * terrainData.heightmapResolution);

        float[,] heights = terrainData.GetHeights(x - mountainSize / 2, z - mountainSize / 2, mountainSize, mountainSize);

        for (int i = 0; i < mountainSize; i++)
        {
            for (int j = 0; j < mountainSize; j++)
            {
                float dx = (i - mountainSize / 2) / (float)(mountainSize / 2);
                float dz = (j - mountainSize / 2) / (float)(mountainSize / 2);
                float distance = Mathf.Sqrt(dx * dx + dz * dz);

                float gaussianFactor = Mathf.Exp(-distance * distance / (2 * gaussianSpread * gaussianSpread));
                float smoothFactor = Mathf.SmoothStep(1f, 0f, distance);
                float targetHeight = smoothFactor * gaussianFactor * heightOffsets[obj] * maxMountainHeight;

                heights[i, j] = Mathf.Clamp(targetHeight, 0, 1);
            }
        }

        terrainData.SetHeights(x - mountainSize / 2, z - mountainSize / 2, heights);
        previousPositions[obj] = obj.position;
    }

    void ResetPreviousHeight(Transform obj)
    {
        if (!previousPositions.ContainsKey(obj)) return;

        Terrain terrain = GetTerrainAtPosition(previousPositions[obj]);
        if (terrain == null) return;

        TerrainData terrainData = terrain.terrainData;
        int x = Mathf.RoundToInt((previousPositions[obj].x - terrain.transform.position.x) / terrainData.size.x * terrainData.heightmapResolution);
        int z = Mathf.RoundToInt((previousPositions[obj].z - terrain.transform.position.z) / terrainData.size.z * terrainData.heightmapResolution);

        float[,] heights = terrainData.GetHeights(x - mountainSize / 2, z - mountainSize / 2, mountainSize, mountainSize);

        for (int i = 0; i < mountainSize; i++)
        {
            for (int j = 0; j < mountainSize; j++)
            {
                heights[i, j] = 0f;
            }
        }

        terrainData.SetHeights(x - mountainSize / 2, z - mountainSize / 2, heights);
    }

    void ResetAllTerrains()
    {
        foreach (var terrain in terrains)
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
}
