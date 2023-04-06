using Battlehub.UIControls;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class AnimationCreateView : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_label = null;

        [SerializeField]
        private Button m_createButton = null;

        public string Text
        {
            get { return m_label.text; }
            set { m_label.text = value; }
        }

        public event Action Click;

        public void Awake()
        {
            UnityEventHelper.AddListener(m_createButton, button => button.onClick, OnClick);
        }

        public void OnDestroy()
        {
            UnityEventHelper.RemoveListener(m_createButton, button => button.onClick, OnClick);
        }

        private void OnClick()
        {
            if (Click != null)
            {
                Click();
            }
        }

    }

}

