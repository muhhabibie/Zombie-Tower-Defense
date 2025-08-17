using System.IO;
using UnityEngine;

public class MapGenerator : MonoBehaviour
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

    private TileType[,] grid;

    void Start()
    {
        GenerateMapData();
    }

    void InstantiateTiles()
    {
        Debug.Log("Instantiating COMPLETE SMART tiles...");

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == TileType.Ground)
                {
                    InstantiateTile(groundTilePrefab, new Vector3(x * tileSize, 0, y * tileSize), Quaternion.identity, Vector3.one);
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

    void GenerateMapData()
    {
        Debug.Log("Starting map data generation...");

        grid = new TileType[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = TileType.Ground;
            }
        }

        CreatePath();
        InstantiateTiles();
        PlaceDecorations();

        Debug.Log("Map Instantiation complete!");
    }

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

        grid[currentX, currentY] = TileType.Path;
        if (Random.value > 0.5f)
        {
            Debug.Log("Path Ending Style: Corner");
            grid[currentX + 1, currentY] = TileType.Path;
            if (currentY > height / 2 && currentY > 1) currentY--; else currentY++;
            grid[currentX + 1, currentY] = TileType.Path;
            grid[currentX + 2, currentY] = TileType.Path;
        }
        else
        {
            Debug.Log("Path Ending Style: Straight");
            grid[currentX + 1, currentY] = TileType.Path;
            grid[currentX + 2, currentY] = TileType.Path;
        }
    }

    void PlaceDecorations()
    {
        if (decorationPrefabs == null || decorationPrefabs.Length == 0)
        {
            Debug.Log("Tidak ada prefab dekorasi yang ditetapkan.");
            return;
        }

        Debug.Log("Placing decorations...");

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == TileType.Ground)
                {
                    if (Random.value < decorationDensity)
                    {
                        int randomIndex = Random.Range(0, decorationPrefabs.Length);
                        GameObject prefabToUse = decorationPrefabs[randomIndex];

                        if (prefabToUse != null)
                        {
                            Vector3 position = new Vector3(x * tileSize, 1f, y * tileSize);
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