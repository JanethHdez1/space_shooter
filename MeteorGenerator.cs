using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class MeteorGenerator : MonoBehaviour
{
    [SerializeField] private GameObject meteorPrefab;
    [SerializeField] private Vector2 cameraArea = new(5f, 5f);
    [SerializeField] private int meteorsPerSecond = 1; // Cuantos meteoritos por segundo
    [SerializeField] private int maxMeteors = 50; // Máximo a generar en total
    [SerializeField] private Transform turret;

    private int spawnedCount = 0;

    private float MaxXPos => cameraArea.x / 2;
    private float MaxYPos => cameraArea.y / 2;
    private float MinYPos => -cameraArea.y / 2;
    private float MinXPos => -cameraArea.x / 2;

    private IEnumerator Start()
    {
        while (spawnedCount < maxMeteors)
        {
            var instance = Instantiate(meteorPrefab, transform);
            instance.transform.position = GetRandomEdgePosition();
            var vectorToTurret = (turret.position - instance.transform.position).normalized;

            var meteor = instance.GetComponent<MeteorBehaviour>();
            meteor.Direction = vectorToTurret;

            spawnedCount++;

            var delay = meteorsPerSecond > 0 ? 1f / meteorsPerSecond : 1f;
            yield return new WaitForSeconds(delay);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(cameraArea.x, 0f, cameraArea.y));
    }

    private Vector3 GetRandomEdgePosition()
    {
        var randomEdge = Random.Range(0, 3); // 0:right, 1:left, 2:top
        var positionOnEdge = randomEdge switch
        {
            0 => new Vector3(MaxXPos, transform.position.y, Random.Range(MinYPos, MaxYPos)),
            1 => new Vector3(MinXPos, transform.position.y, Random.Range(MinYPos, MaxYPos)),
            2 => new Vector3(Random.Range(MinXPos, MaxXPos), transform.position.y, MaxYPos),
            _ => new Vector3(Random.Range(MinXPos, MaxXPos), transform.position.y, MinYPos),
        };

        return positionOnEdge;
    }
}
