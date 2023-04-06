using System;
using UnityEngine;
using UnityEngine.Events;

namespace Battlehub.RTEditor.Views
{
    public class SelectColorView : View
    {
        [SerializeField]
        private ColorPicker m_colorPicker = null;

        [NonSerialized]
        public UnityEvent ColorChanged = new UnityEvent();
        public Color Color
        {
            get { return m_colorPicker.CurrentColor; }
            set { m_colorPicker.CurrentColor = value; }
        }

        protected override void Awake()
        {
            base.Awake();

            if(m_colorPicker == null)
            {
                m_colorPicker = GetComponentInChildren<ColorPicker>(true);
            }

            m_colorPicker.onHSVChanged.AddListener(OnHSVChanged);
            m_colorPicker.onValueChanged.AddListener(OnColorChanged);
        }

        protected override void OnDestroy()
        {
            m_colorPicker.onHSVChanged.RemoveListener(OnHSVChanged);
            m_colorPicker.onValueChanged.RemoveListener(OnColorChanged);

            base.OnDestroy();
        }

        private void OnHSVChanged(float h, float s, float v)
        {
            ColorChanged?.Invoke();
        }

        private void OnColorChanged(Color color)
        {
            ColorChanged?.Invoke();
        }
    }
}


