using System.IO;
using UnityEngine;
using System.Collections.Generic; // ⬅️ Tambahan untuk List<Vector3>

public class MapGenerator3 : MonoBehaviour
{
    public enum TileType
    {
        Empty,
        Ground,
        Path
    }

    [Header("Map Dimensions")]
    public int width;
    public int height;
    public float tileSize = 5f;

    [Header("Tile Prefabs")]
    public GameObject groundTilePrefab;
    public GameObject slopePrefab;
    public GameObject pathSpawnPrefab;
    public GameObject pathStraightPrefab;
    public GameObject pathCornerPrefab;
    public GameObject pathSplitPrefab;
    public GameObject pathCrossingPrefab;
    public GameObject pathEndPrefab;

    [Header("Decoration Prefabs")]
    [Tooltip("Prefab-prefab yang akan ditempatkan secara acak di ubin tanah.")]
    public GameObject[] decorationPrefabs;

    [Tooltip("Seberapa padat dekorasi yang akan muncul. 0 = tidak ada, 1 = setiap ubin tanah ada dekorasi.")]
    [Range(0, 1)]
    public float decorationDensity = 0.15f;

    [Tooltip("Peluang sebuah ubin tanah menjadi titik awal dari sekelompok ubin tinggi.")]
    [Range(0, 1)]
    public float tallGroundGroupChance = 0.1f;

    public Vector3 endPointPosition { get; private set; }

    // 🆕 List Waypoints
    public List<Vector3> waypoints = new List<Vector3>();

    private TileType[,] grid;
    private int[,] groundVariationMap; // 0=Normal, 1=Tinggi, 2=Tanjakan

    void Start()
    {
        GenerateMapData();
    }

    // =============================================
    // BAGIAN CLUSTER TANAH TINGGI (tetap sama)
    // =============================================
    void GenerateTallGroundWithGuaranteedSlopes()
    {
        groundVariationMap = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == TileType.Ground && groundVariationMap[x, y] == 0 && Random.value < tallGroundGroupChance)
                {
                    int targetSize = Random.Range(6, 9);
                    var clusterPoints = GrowFlexibleCluster(x, y, targetSize);

                    if (clusterPoints.Count >= 3)
                    {
                        Vector2Int slopeLocation = FindOrCreateSlopeLocation(clusterPoints);

                        foreach (var point in clusterPoints)
                        {
                            groundVariationMap[point.x, point.y] = 1;
                        }
                        groundVariationMap[slopeLocation.x, slopeLocation.y] = 2;
                    }
                }
            }
        }
    }

    System.Collections.Generic.List<Vector2Int> GrowFlexibleCluster(int startX, int startY, int targetSize)
    {
        var cluster = new System.Collections.Generic.List<Vector2Int>();
        var frontier = new System.Collections.Generic.List<Vector2Int>();

        cluster.Add(new Vector2Int(startX, startY));
        var clusterSet = new System.Collections.Generic.HashSet<Vector2Int>(cluster);
        var frontierSet = new System.Collections.Generic.HashSet<Vector2Int>();
        AddNeighborsToFrontier(new Vector2Int(startX, startY), frontier, clusterSet, frontierSet);

        while (cluster.Count < targetSize && frontier.Count > 0)
        {
            int i = Random.Range(0, frontier.Count);
            Vector2Int candidate = frontier[i];
            frontier.RemoveAt(i);
            frontierSet.Remove(candidate);

            if (candidate.x >= 0 && candidate.x < width && candidate.y >= 0 && candidate.y < height &&
                grid[candidate.x, candidate.y] == TileType.Ground && groundVariationMap[candidate.x, candidate.y] == 0)
            {
                cluster.Add(candidate);
                clusterSet.Add(candidate);
                AddNeighborsToFrontier(candidate, frontier, clusterSet, frontierSet);
            }
        }
        return cluster;
    }

    Vector2Int FindOrCreateSlopeLocation(System.Collections.Generic.List<Vector2Int> clusterPoints)
    {
        var candidates = new System.Collections.Generic.List<Vector2Int>();
        var clusterSet = new System.Collections.Generic.HashSet<Vector2Int>(clusterPoints);

        foreach (var point in clusterPoints)
        {
            Vector2Int[] neighbors = {
                new Vector2Int(point.x + 1, point.y), new Vector2Int(point.x - 1, point.y),
                new Vector2Int(point.x, point.y + 1), new Vector2Int(point.x, point.y - 1)
            };
            foreach (var neighbor in neighbors)
            {
                if (neighbor.x >= 0 && neighbor.x < width && neighbor.y >= 0 && neighbor.y < height &&
                    grid[neighbor.x, neighbor.y] == TileType.Ground && !clusterSet.Contains(neighbor))
                {
                    candidates.Add(neighbor);
                }
            }
        }

        if (candidates.Count > 0)
        {
            return candidates[Random.Range(0, candidates.Count)];
        }
        else
        {
            Vector2Int sacrificedTile = clusterPoints[clusterPoints.Count - 1];
            clusterPoints.RemoveAt(clusterPoints.Count - 1);
            return sacrificedTile;
        }
    }

    void AddNeighborsToFrontier(Vector2Int point, System.Collections.Generic.List<Vector2Int> frontier, System.Collections.Generic.HashSet<Vector2Int> cluster, System.Collections.Generic.HashSet<Vector2Int> frontierSet)
    {
        Vector2Int[] neighbors = {
            new Vector2Int(point.x + 1, point.y), new Vector2Int(point.x - 1, point.y),
            new Vector2Int(point.x, point.y + 1), new Vector2Int(point.x, point.y - 1)
        };
        foreach (var n in neighbors)
        {
            if (!cluster.Contains(n) && !frontierSet.Contains(n))
            {
                frontier.Add(n);
                frontierSet.Add(n);
            }
        }
    }

    // =============================================
    // INSTANTIASI TILE (tetap sama)
    // =============================================
    void InstantiateTiles()
    {
        Debug.Log("Instantiating COMPLETE SMART tiles...");

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == TileType.Ground)
                {
                    int variation = groundVariationMap[x, y];
                    Vector3 position = new Vector3(x * tileSize, 0, y * tileSize);
                    Quaternion rotation = Quaternion.identity;
                    GameObject prefabToUse = groundTilePrefab;
                    Vector3 scaleToUse = Vector3.one;

                    switch (variation)
                    {
                        case 1: scaleToUse.y *= 4; break;
                        case 2:
                            prefabToUse = slopePrefab;
                            scaleToUse.y *= 2;
                            if (x + 1 < width && groundVariationMap[x + 1, y] == 1) rotation = Quaternion.Euler(0, -90, 0);
                            else if (x - 1 >= 0 && groundVariationMap[x - 1, y] == 1) rotation = Quaternion.Euler(0, 90, 0);
                            else if (y + 1 < height && groundVariationMap[x, y + 1] == 1) rotation = Quaternion.Euler(0, 180, 0);
                            else if (y - 1 >= 0 && groundVariationMap[x, y - 1] == 1) rotation = Quaternion.Euler(0, 0, 0);
                            break;
                    }

                    InstantiateTile(prefabToUse, position, rotation, scaleToUse);
                }
                else if (grid[x, y] == TileType.Path)
                {
                    Vector3 position = new Vector3(x * tileSize, 0, y * tileSize);

                    if (x == 0 && pathSpawnPrefab != null)
                    {
                        InstantiateTile(pathSpawnPrefab, position, Quaternion.Euler(0, 90, 0), Vector3.one);
                        continue;
                    }

                    bool pathUp = IsPathAt(x, y + 1);
                    bool pathDown = IsPathAt(x, y - 1);
                    bool pathRight = IsPathAt(x + 1, y);
                    bool pathLeft = IsPathAt(x - 1, y);

                    int neighborCount = 0;
                    if (pathUp) neighborCount++;
                    if (pathDown) neighborCount++;
                    if (pathRight) neighborCount++;
                    if (pathLeft) neighborCount++;

                    GameObject prefabToUse = pathStraightPrefab;
                    Quaternion rotation = Quaternion.identity;

                    if (neighborCount == 0 || neighborCount == 1)
                    {
                        prefabToUse = pathEndPrefab;
                        if (pathUp) rotation = Quaternion.Euler(0, 0, 0);
                        else if (pathDown) rotation = Quaternion.Euler(0, 180, 0);
                        else if (pathLeft) rotation = Quaternion.Euler(0, -90, 0);
                        else if (pathRight) rotation = Quaternion.Euler(0, 90, 0);
                        endPointPosition = position;
                        Debug.Log("MapGenerator MENYIMPAN endPointPosition: " + endPointPosition);
                    }
                    else if (neighborCount == 2)
                    {
                        if (pathUp && pathDown)
                        {
                            prefabToUse = pathStraightPrefab;
                            rotation = Quaternion.Euler(0, 0, 0);
                        }
                        else if (pathLeft && pathRight)
                        {
                            prefabToUse = pathStraightPrefab;
                            rotation = Quaternion.Euler(0, 90, 0);
                        }
                        else
                        {
                            prefabToUse = pathCornerPrefab;
                            if (pathUp && pathRight) rotation = Quaternion.Euler(0, 90, 0);
                            else if (pathRight && pathDown) rotation = Quaternion.Euler(0, 180, 0);
                            else if (pathDown && pathLeft) rotation = Quaternion.Euler(0, -90, 0);
                            else if (pathLeft && pathUp) rotation = Quaternion.Euler(0, 0, 0);
                        }
                    }

                    InstantiateTile(prefabToUse, position, rotation, Vector3.one);
                }
            }
        }
    }

    void InstantiateTile(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (prefab != null)
        {
            GameObject newTile = Instantiate(prefab, position, rotation);
            newTile.transform.parent = this.transform;
            newTile.transform.localScale = scale;
        }
    }

    bool IsPathAt(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return grid[x, y] == TileType.Path;
        }
        return false;
    }

    // =============================================
    // MAP GENERATION
    // =============================================
    void GenerateMapData()
    {
        Debug.Log("Starting map data generation...");

        grid = new TileType[width, height];
        waypoints.Clear(); // 🆕 Reset waypoints setiap generate baru

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = TileType.Ground;
            }
        }

        CreatePath();
        GenerateTallGroundWithGuaranteedSlopes();
        InstantiateTiles();
        PlaceDecorations();

        Debug.Log("Map Instantiation complete!");
    }

    // =============================================
    // CREATE PATH + WAYPOINTS
    // =============================================
    void CreatePath()
    {
        Debug.Log("Creating path with momentum and segments...");

        int currentX = 0;
        int currentY = Random.Range(3, height - 3);

        Vector2Int moveDirection = Vector2Int.right;
        int stepsInDirection = 0;
        int stepsToTake = Random.Range(2, 5);

        while (currentX < width - 3)
        {
            grid[currentX, currentY] = TileType.Path;

            // 🆕 Tambahkan waypoint setiap kali path dibuat
            waypoints.Add(new Vector3(currentX * tileSize, 0, currentY * tileSize));

            if (stepsInDirection >= stepsToTake)
            {
                if (moveDirection == Vector2Int.right)
                {
                    moveDirection = (currentY > height / 2) ? Vector2Int.down : Vector2Int.up;
                }
                else
                {
                    moveDirection = Vector2Int.right;
                }
                stepsInDirection = 0;
                stepsToTake = Random.Range(2, 5);
            }

            if ((currentY <= 1 && moveDirection.y < 0) || (currentY >= height - 2 && moveDirection.y > 0))
            {
                moveDirection = Vector2Int.right;
                stepsInDirection = 0;
                stepsToTake = Random.Range(2, 5);
            }

            currentX += moveDirection.x;
            currentY += moveDirection.y;
            stepsInDirection++;
        }

        // Akhir path
        grid[currentX, currentY] = TileType.Path;
        waypoints.Add(new Vector3(currentX * tileSize, 0, currentY * tileSize));

        if (Random.value > 0.5f)
        {
            Debug.Log("Path Ending Style: Corner");
            grid[currentX + 1, currentY] = TileType.Path;
            waypoints.Add(new Vector3((currentX + 1) * tileSize, 0, currentY * tileSize));

            if (currentY > height / 2 && currentY > 1) currentY--; else currentY++;
            grid[currentX + 1, currentY] = TileType.Path;
            waypoints.Add(new Vector3((currentX + 1) * tileSize, 0, currentY * tileSize));

            grid[currentX + 2, currentY] = TileType.Path;
            waypoints.Add(new Vector3((currentX + 2) * tileSize, 0, currentY * tileSize));
        }
        else
        {
            Debug.Log("Path Ending Style: Straight");
            grid[currentX + 1, currentY] = TileType.Path;
            waypoints.Add(new Vector3((currentX + 1) * tileSize, 0, currentY * tileSize));

            grid[currentX + 2, currentY] = TileType.Path;
            waypoints.Add(new Vector3((currentX + 2) * tileSize, 0, currentY * tileSize));
        }
    }

    // =============================================
    // DECORATIONS (tetap sama)
    // =============================================
    void PlaceDecorations()
    {
        if (decorationPrefabs == null || decorationPrefabs.Length == 0)
        {
            Debug.Log("Tidak ada prefab dekorasi yang ditetapkan.");
            return;
        }

        Debug.Log("Placing smart decorations...");

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == TileType.Ground)
                {
                    int variation = groundVariationMap[x, y];
                    if (variation == 2) continue;

                    if (Random.value < decorationDensity)
                    {
                        int randomIndex = Random.Range(0, decorationPrefabs.Length);
                        GameObject prefabToUse = decorationPrefabs[randomIndex];

                        if (prefabToUse != null)
                        {
                            float baseHeight = 0f;
                            if (variation == 1) baseHeight = 3f;

                            Vector3 position = new Vector3(x * tileSize, baseHeight + 1f, y * tileSize);
                            Quaternion rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                            InstantiateTile(prefabToUse, position, rotation, Vector3.one);
                        }
                    }
                }
            }
        }
    }

    void Update()
    {

    }
}
