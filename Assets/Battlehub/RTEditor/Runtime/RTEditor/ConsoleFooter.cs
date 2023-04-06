using Battlehub.RTCommon;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class ConsoleFooter : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private Sprite m_infoIcon = null;

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

        [SerializeField]
        private Image m_image = null;

        [SerializeField]
        private TextMeshProUGUI m_text = null;

        private IRuntimeConsole m_console;

        private void Awake()
        {
            m_image.gameObject.SetActive(false);
            m_text.gameObject.SetActive(false);

            m_console = IOC.Resolve<IRuntimeConsole>();
            m_console.MessageAdded += OnMessageAdded;
            m_console.Cleared += OnCleared;
        }

        private void OnDestroy()
        {
            if(m_console != null)
            {
                m_console.MessageAdded -= OnMessageAdded;
                m_console.Cleared -= OnCleared;
            }
        }

        private void OnMessageAdded(IRuntimeConsole console, ConsoleLogEntry arg)
        {
            m_image.gameObject.SetActive(true);
            m_text.gameObject.SetActive(true);

            switch (arg.LogType)
            {
                case LogType.Log:
                    m_image.sprite = m_infoIcon;
                    m_image.color = m_infoColor;
                    break;
                case LogType.Warning:
                    m_image.sprite = m_warningIcon;
                    m_image.color = m_warningColor;
                    break;
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    m_image.sprite = m_errorIcon;
                    m_image.color = m_errorColor;
                    break;
            }

            m_text.text = arg.Condition.Replace('\n', ' ').Replace("\r", string.Empty);
        }

        private void OnCleared(IRuntimeConsole console)
        {
            m_image.gameObject.SetActive(false);
            m_text.gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(m_text != null && m_text.gameObject.activeSelf)
            {
                IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
                editor.CreateOrActivateWindow(RuntimeWindowType.Console.ToString());
            }
        }
    }
}

