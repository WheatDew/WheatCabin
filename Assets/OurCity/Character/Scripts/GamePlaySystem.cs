using UnityEngine.SceneManagement;
using UnityEngine;

public class GamePlaySystem : MonoBehaviour
{
    public void ResetScene()
    {
        // ��ȡ��ǰ����������
        string sceneName = SceneManager.GetActiveScene().name;

        // ���¼��ص�ǰ����
        SceneManager.LoadScene(sceneName);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ResetScene();
        }
    }
}
