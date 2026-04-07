using UnityEngine;

public class PalmFruitSpawner : MonoBehaviour
{
    public GameObject fruitPrefab;
    public Transform spawnPoint;

    void Start()
    {
        SpawnFruit();
    }

    void SpawnFruit()
    {
        if (fruitPrefab != null && spawnPoint != null)
        {
            Instantiate(
                fruitPrefab,
                spawnPoint.position,
                spawnPoint.rotation,
                spawnPoint
            );
        }
    }
}