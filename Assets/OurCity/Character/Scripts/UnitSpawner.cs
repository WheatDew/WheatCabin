using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    public Transform character; // 主角的Transform
    public GameObject unitPrefab; // 单位的预制体
    public float spawnRadius = 20.0f; // 生成半径
    public int numberOfUnits = 1; // 要生成的单位数量
    public float interval = 5;
    private float timer = 5;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > interval&&character.gameObject.layer!=0)
        {
            SpawnUnitsAroundCharacter();
            timer = 0;
        }
    }

    void SpawnUnitsAroundCharacter()
    {
        for (int i = 0; i < numberOfUnits; i++)
        {
            Vector3 randomPosition = RandomPositionAroundCharacter();
            randomPosition.y = Terrain.activeTerrain.SampleHeight(randomPosition) + Terrain.activeTerrain.transform.position.y;
            Instantiate(unitPrefab, randomPosition, Quaternion.identity);
        }
    }

    Vector3 RandomPositionAroundCharacter()
    {
        Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
        randomDirection += character.position;
        randomDirection.y = character.position.y; // 保持与地形的水平高度相同，以便正确地计算地形高度
        return randomDirection;
    }
}

