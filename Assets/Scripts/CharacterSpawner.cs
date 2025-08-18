using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    [Header("Referensi")]
    public MapGenerator2 mapGenerator; 
    public GameObject characterPrefab;
    public IsoCameraFollow isoCameraFollow;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (mapGenerator == null || characterPrefab == null)
        {
            Debug.LogError("Referensi di CharacterSpawner belum diatur!");
            return;
        }

        Debug.Log("CharacterSpawner MEMBACA endPointPosition: " + mapGenerator.endPointPosition);
        Vector3 spawnPosition = mapGenerator.endPointPosition;


        spawnPosition.y += 2f;
        spawnPosition.x -= 3f;


        GameObject spawnedCharacter = Instantiate(characterPrefab, spawnPosition, Quaternion.identity);
        Debug.Log("Karakter di-spawn di: " + spawnPosition);

        isoCameraFollow.target = spawnedCharacter.transform;
        Debug.Log("Kamera sekarang menargetkan: " + spawnedCharacter.name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
