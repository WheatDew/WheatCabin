using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    public Transform character; // ���ǵ�Transform
    public GameObject unitPrefab; // ��λ��Ԥ����
    public float spawnRadius = 20.0f; // ���ɰ뾶
    public int numberOfUnits = 1; // Ҫ���ɵĵ�λ����
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
        randomDirection.y = character.position.y; // ��������ε�ˮƽ�߶���ͬ���Ա���ȷ�ؼ�����θ߶�
        return randomDirection;
    }
}

