using UnityEngine;

public class XRTreeSpawner : MonoBehaviour
{
    public GameObject treePrefab;
    public Terrain terrain;
    public int treeCount = 50;

    void Start()
    {
        SpawnTrees();
    }

    void SpawnTrees()
    {
        TerrainData data = terrain.terrainData;

        for (int i = 0; i < treeCount; i++)
        {
            float x = Random.Range(0, data.size.x);
            float z = Random.Range(0, data.size.z);

            float y = terrain.SampleHeight(new Vector3(x, 0, z));

            Vector3 position = new Vector3(
                x + terrain.transform.position.x,
                y,
                z + terrain.transform.position.z
            );

            Instantiate(
                treePrefab,
                position,
                Quaternion.Euler(0, Random.Range(0, 360), 0),
                transform
            );
        }
    }
}