using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
    public int numberOfSegments = 10;
    public Transform player;
    private float spawnZ = 20f;

    void Start()
    {
        return; // Dừng hẳn việc sinh map tự động
    }

    float GetPrefabLength(GameObject prefab)
    {
        // Tự động quét kích thước thực tế trên trục Z của Prefab
        Renderer renderer = prefab.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size.z;
        }
        return 60f;
    }
}