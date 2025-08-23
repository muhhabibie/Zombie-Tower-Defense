using UnityEngine;

public class CastleSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Contoh di dalam CastleSpawner.cs
    public GameObject castlePrefab;
    public MapGenerator3 mapGenerator;
    void Start()
    {
        // Dapatkan koordinat grid dari titik akhir
        Vector2Int endCoord = mapGenerator.endPointGridCoord;
        Quaternion rotation = Quaternion.Euler(0, -90, 0);
        // Hitung posisi tengah dari area 3x3
        // (x + 2) adalah tengah horizontal, (y) adalah tengah vertikal
        Vector3 castlePosition = new Vector3((endCoord.x + 2) * mapGenerator.tileSize, 0, endCoord.y * mapGenerator.tileSize);

        // Spawn kastil
        Instantiate(castlePrefab, castlePosition, rotation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
