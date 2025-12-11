using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] shipPrefabs;
    
    [SerializeField] private float spawnDelay = 2f;
    [SerializeField] private Vector2 cameraArea = new(53f, 30f);
    [SerializeField] private float minDistanceBetweenSpawns = 5f;
    [SerializeField] private int maxSpawnAttempts = 10;
    
    [SerializeField] private Transform turret;
    [SerializeField] private int[] pointsPerShip = { 10, 20, 30, 40, 50 };
    
    private float MaxXPos => cameraArea.x / 2;
    private float MaxZPos => cameraArea.y / 2;
    private float MinZPos => -cameraArea.y / 2;
    private float MinXPos => -cameraArea.x / 2;
    
    private int _totalSpawned = 0;
    private bool _isSpawning = false;

    private void Start()
    {
        if (shipPrefabs == null || shipPrefabs.Length == 0)
        {
            Debug.LogError("EnemySpawner: No hay naves asignadas!");
            return;
        }
        
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        _isSpawning = true;
        
        while (_isSpawning)
        {
            SpawnRandomShip();
            _totalSpawned++;
            
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnRandomShip()
    {
        int randomIndex = Random.Range(0, shipPrefabs.Length);
        GameObject selectedPrefab = shipPrefabs[randomIndex];
        
        if (selectedPrefab == null)
        {
            Debug.LogError($"Prefab en índice {randomIndex} es null!");
            return;
        }
        
        Vector3 spawnPosition = Vector3.zero;
        bool validPositionFound = false;
        
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector3 testPosition = GetRandomEdgePosition();
            
            if (IsPositionClearOfShips(testPosition))
            {
                spawnPosition = testPosition;
                validPositionFound = true;
                break;
            }
        }
        
        if (!validPositionFound)
        {
            spawnPosition = GetRandomEdgePosition();
            Debug.LogWarning("No se encontró posición ideal, spawneando de todas formas");
        }
        
        GameObject shipInstance = Instantiate(
            selectedPrefab,
            spawnPosition,
            Quaternion.identity,
            transform);
        
        if (turret != null)
        {
            Vector3 directionToTurret = (turret.position - spawnPosition).normalized;
            directionToTurret.y = 0;
            
            if (directionToTurret != Vector3.zero)
            {
                shipInstance.transform.rotation = Quaternion.LookRotation(directionToTurret);
            }
        }
        
        var enemyShip = shipInstance.GetComponent<EnemyShip>();
        if (enemyShip != null)
        {
            int points = (randomIndex < pointsPerShip.Length) ? pointsPerShip[randomIndex] : 10;
            enemyShip.SetPointsOnDestroy(points);
            enemyShip.Initialize(spawnPosition);
            
            Debug.Log(
                $"Nave #{_totalSpawned + 1} generada (Tipo {randomIndex}, {points} puntos) en {spawnPosition}");
        }
        
        ConfigureShipCollisions(shipInstance);
    }

    private bool IsPositionClearOfShips(Vector3 position)
    {
        EnemyShip[] allShips = FindObjectsByType<EnemyShip>(FindObjectsSortMode.None);
        
        foreach (var ship in allShips)
        {
            float distance = Vector3.Distance(position, ship.transform.position);
            if (distance < minDistanceBetweenSpawns)
            {
                return false;
            }
        }
        
        return true;
    }

    private void ConfigureShipCollisions(GameObject ship)
    {
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer == -1)
        {
            Debug.LogWarning(
                "No existe la capa 'Enemy'. Créala en Edit > Project Settings > Tags & Layers");
            return;
        }
        
        ship.layer = enemyLayer;
        
        Collider[] colliders = ship.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.gameObject.layer = enemyLayer;
        }
        
        Physics.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
    }

    private Vector3 GetRandomEdgePosition()
    {
        int randomEdge = Random.Range(0, 3);
        
        Vector3 positionOnEdge = randomEdge switch
        {
            0 => new(MaxXPos, transform.position.y, Random.Range(MinZPos, MaxZPos)),
            1 => new(MinXPos, transform.position.y, Random.Range(MinZPos, MaxZPos)),
            2 => new(Random.Range(MinXPos, MaxXPos), transform.position.y, MaxZPos),
            _ => new(Random.Range(MinXPos, MaxXPos), transform.position.y, MinZPos),
        };

        return positionOnEdge;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(cameraArea.x, 0f, cameraArea.y));
        
        Gizmos.color = Color.yellow;
        Vector3[] corners = new[]
        {
            new Vector3(MaxXPos, transform.position.y, MaxZPos),
            new Vector3(MaxXPos, transform.position.y, MinZPos),
            new Vector3(MinXPos, transform.position.y, MaxZPos),
            new Vector3(MinXPos, transform.position.y, MinZPos),
        };
        
        foreach (var corner in corners)
        {
            Gizmos.DrawWireSphere(corner, minDistanceBetweenSpawns);
        }
    }
}