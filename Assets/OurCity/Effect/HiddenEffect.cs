using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenEffect : MonoBehaviour
{
    public float life;
    private float m_life = 0;
    void OnEnable()
    {
        m_life = 0;
    }

    // Update is called once per frame
    void Update()
    {
        m_life += Time.deltaTime;
        if (m_life > life)
        {
            gameObject.SetActive(false);
        }
    }
}
