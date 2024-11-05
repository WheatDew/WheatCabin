using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public GameObject unitPrefab;
    public GameObject spawnCube; // 引用场景中的 Cube
    public float safeDistance = 1.0f; // 障碍物安全距离
    public int numberOfUnits = 10;
    public float interval = 5;
    private float timer = 5;
    public int maxCheckCount = 10;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > interval)
        {
            SpawnUnitsAroundCharacter();
            timer = 0;
        }
    }

    void SpawnUnitsAroundCharacter()
    {
        Bounds bounds = spawnCube.GetComponent<Renderer>().bounds; // 获取 Cube 的边界

        for (int i = 0; i < numberOfUnits; i++)
        {
            for(int n = 0; n < maxCheckCount; n++)
            {
                Vector3 randomPosition = RandomPositionInBounds(bounds);
                if (randomPosition != Vector3.zero) // 检查位置是否有效
                {
                    Instantiate(unitPrefab, randomPosition, Quaternion.identity);
                    break;
                }
                else
                {
                    if (n == 9)
                    {
                        Debug.LogError("循环次数过多");
                        return;
                    }
                    continue;
                }
            }

        }
    }

    Vector3 RandomPositionInBounds(Bounds bounds)
    {
        for (int tries = 0; tries < 10; tries++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );

            // 检查生成位置附近是否有障碍物
            Collider[] hitColliders = Physics.OverlapSphere(randomPoint, safeDistance,~LayerMask.GetMask("Range"));
            if (hitColliders.Length == 0)
            {
                return randomPoint; // 如果没有障碍物，返回这个位置
            }
        }
        return Vector3.zero; // 如果找不到合适的位置，返回零向量
    }
}
