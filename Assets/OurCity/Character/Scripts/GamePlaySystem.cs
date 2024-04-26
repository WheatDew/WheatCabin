using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class GamePlaySystem : MonoBehaviour
{
    private static GamePlaySystem _s;
    public static GamePlaySystem s { get { return _s; } }

    void Awake()
    {
        if (_s == null)
            _s = this;
    }

    public Text ammoText;
    public Text killedText;
    private int killed = 0;
    public void ResetScene()
    {
        // ��ȡ��ǰ����������
        string sceneName = SceneManager.GetActiveScene().name;

        // ���¼��ص�ǰ����
        SceneManager.LoadScene(sceneName);
    }

    private void Update()
    {
        killedText.text = killed.ToString();
    }

    public void ScoreGain()
    {
        killed++;
    }

    public void AmmoDisplay(int current,int max)
    {
        ammoText.text = string.Format("{0}/{1}", current, max);
    }
}
