using Battlehub.RTCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class SceneSettingPanel : MonoBehaviour
    {
        [SerializeField]
        private Toggle m_2dModeToggle = null;

        [SerializeField]
        private RuntimeWindow m_window = null;

        [SerializeField]
        private Image m_border = null;

        private ISceneSettingsComponent m_sceneSettingsComponent;

        private void Awake()
        {
            m_sceneSettingsComponent = m_window.IOCContainer.Resolve<ISceneSettingsComponent>();
            if(m_sceneSettingsComponent != null)
            {
                m_sceneSettingsComponent.IsUserDefinedChanged += IsUserDefinedChanged;
                m_sceneSettingsComponent.Is2DModeChanged += OnIs2DModeChanged;

                gameObject.SetActive(m_sceneSettingsComponent.IsUserDefined);
                m_2dModeToggle.SetIsOnWithoutNotify(m_sceneSettingsComponent.Is2DMode);

                if(m_border != null)
                {
                    m_border.gameObject.SetActive(gameObject.activeSelf);
                }
            }
            
            m_2dModeToggle.onValueChanged.AddListener(On2DModeValueChanged);
        }

        private void OnDestroy()
        {
            if (m_sceneSettingsComponent != null)
            {
                m_sceneSettingsComponent.IsUserDefinedChanged -= IsUserDefinedChanged;
                m_sceneSettingsComponent.Is2DModeChanged -= OnIs2DModeChanged;
            }

            m_2dModeToggle.onValueChanged.RemoveListener(On2DModeValueChanged);
        }

        private void IsUserDefinedChanged(object sender, bool oldValue, bool newValue)
        {
            gameObject.SetActive(newValue);
        }

        private void OnIs2DModeChanged(object sender, bool oldValue, bool newValue)
        {
            m_2dModeToggle.SetIsOnWithoutNotify(newValue);
        }

        private void On2DModeValueChanged(bool value)
        {
            m_sceneSettingsComponent.Is2DMode = value;
        }

      
    }
}

