using UnityEngine;

public class TunnelSpawn : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject gatePrefab;
    public MapGenerator3 mapGenerator;
    void Start()
    {
        // Dapatkan koordinat grid dari titik awal
        Vector2Int spawnCoord = mapGenerator.spawnPointGridCoord;
        Quaternion rotation = Quaternion.Euler(0, 90, 0);
        // Buat satu baris gerbang di tengah area yang sudah dibersihkan
        for (int i = 0; i < mapGenerator.outerFrameSize; i++)
            {
                // -1 - i agar berbaris ke kiri dari x=-1
                Vector3 gatePosition = new Vector3((-1 - i) * mapGenerator.tileSize, 0, spawnCoord.y * mapGenerator.tileSize);
                Instantiate(gatePrefab, gatePosition, rotation);
            }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
