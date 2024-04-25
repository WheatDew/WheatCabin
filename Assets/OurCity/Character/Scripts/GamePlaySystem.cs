using UnityEngine.SceneManagement;
using UnityEngine;

public class GamePlaySystem : MonoBehaviour
{
    public void ResetScene()
    {
        // 获取当前场景的名称
        string sceneName = SceneManager.GetActiveScene().name;

        // 重新加载当前场景
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
