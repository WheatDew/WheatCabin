using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Binding.Adapters
{
    [CreateAssetMenu(menuName = "Unity Weld/Runtime Editor/Adapter options/LogType adapter options")]
    public class LogTypeAdapterOptions : AdapterOptions
    {
        [SerializeField]
        public Sprite m_infoIcon = null;

        [SerializeField]
        private Sprite m_warningIcon = null;

        [SerializeField]
        private Sprite m_errorIcon = null;

        [SerializeField]
        private Color m_infoColor = Color.white;

        [SerializeField]
        private Color m_warningColor = Color.yellow;

        [SerializeField]
        private Color m_errorColor = Color.red;

        public Sprite InfoIcon
        {
            get { return m_infoIcon; }
        }

        public Sprite WarningIcon
        {
            get { return m_warningIcon; }
        }

        public Sprite ErrorIcon
        {
            get { return m_errorIcon; }
        }

        public Color InfoColor
        {
            get { return m_infoColor; }
        }

        public Color WarningColor
        {
            get { return m_warningColor; }
        }

        public Color ErrorColor
        {
            get { return m_errorColor; }
        }
    }
}

