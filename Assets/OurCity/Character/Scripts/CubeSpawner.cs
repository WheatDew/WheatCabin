using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public GameObject unitPrefab;
    public GameObject spawnCube; // ���ó����е� Cube
    public float safeDistance = 1.0f; // �ϰ��ﰲȫ����
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
        Bounds bounds = spawnCube.GetComponent<Renderer>().bounds; // ��ȡ Cube �ı߽�

        for (int i = 0; i < numberOfUnits; i++)
        {
            for(int n = 0; n < maxCheckCount; n++)
            {
                Vector3 randomPosition = RandomPositionInBounds(bounds);
                if (randomPosition != Vector3.zero) // ���λ���Ƿ���Ч
                {
                    Instantiate(unitPrefab, randomPosition, Quaternion.identity);
                    break;
                }
                else
                {
                    if (n == 9)
                    {
                        Debug.LogError("ѭ����������");
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

            // �������λ�ø����Ƿ����ϰ���
            Collider[] hitColliders = Physics.OverlapSphere(randomPoint, safeDistance,~LayerMask.GetMask("Range"));
            if (hitColliders.Length == 0)
            {
                return randomPoint; // ���û���ϰ���������λ��
            }
        }
        return Vector3.zero; // ����Ҳ������ʵ�λ�ã�����������
    }
}
