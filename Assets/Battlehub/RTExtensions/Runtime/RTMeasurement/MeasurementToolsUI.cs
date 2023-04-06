using Battlehub.RTCommon;
using Battlehub.UIControls;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTMeasurement
{
    public class MeasurementToolsUI : MonoBehaviour
    {
        [SerializeField]
        private Toggle m_toggleDistance = null;

        [SerializeField]
        private Toggle m_toggleAngle = null;

        private IMeasurementTools m_tools;

        private void Start()
        {
            m_tools = IOC.Resolve<IMeasurementTools>();
            m_tools.ToolChanged += OnMeasurementToolChanged;

            Subscribe();
        }

        private void OnDestroy()
        {
            if (m_tools != null)
            {
                m_tools.ToolChanged -= OnMeasurementToolChanged;
            }

            Unsubscribe();
        }

        private void Subscribe()
        {
            UnityEventHelper.AddListener(m_toggleDistance, tog => tog.onValueChanged, OnToggleDistance);
            UnityEventHelper.AddListener(m_toggleAngle, tog => tog.onValueChanged, OnToggleAngle);
        }

        private void Unsubscribe()
        {
            UnityEventHelper.RemoveListener(m_toggleDistance, tog => tog.onValueChanged, OnToggleDistance);
            UnityEventHelper.RemoveListener(m_toggleAngle, tog => tog.onValueChanged, OnToggleAngle);
        }

        private void OnToggleDistance(bool value)
        {
            IMeasurementTools tools = IOC.Resolve<IMeasurementTools>();
            if (value)
            {
                tools.Current = MeasurementToolType.Distance;
            }
            else
            {
                ResetCurrentTool(tools);
            }
        }

        private void OnToggleAngle(bool value)
        {
            IMeasurementTools tools = IOC.Resolve<IMeasurementTools>();
            if (value)
            {
                tools.Current = MeasurementToolType.Angle;
            }
            else
            {
                ResetCurrentTool(tools);
            }
        }

        private void ResetCurrentTool(IMeasurementTools tools)
        {
            m_handleMeasurementToolChanged = false;
            try
            {
                tools.Current = MeasurementToolType.None;
            }
            finally
            {
                m_handleMeasurementToolChanged = true;
            }
        }

        private bool m_handleMeasurementToolChanged = true;
        private void OnMeasurementToolChanged()
        {
            if(!m_handleMeasurementToolChanged)
            {
                return;
            }

            Unsubscribe();

            if(m_tools.Current == MeasurementToolType.None)
            {
                if (m_toggleDistance != null)
                {
                    m_toggleDistance.isOn = false;
                }

                if (m_toggleAngle != null)
                {
                    m_toggleAngle.isOn = false;
                }
            }
            else if (m_tools.Current == MeasurementToolType.Distance)
            {
                if (m_toggleDistance != null)
                {
                    m_toggleDistance.isOn = true;
                }
            }
            else if(m_tools.Current == MeasurementToolType.Angle)
            {
                if (m_toggleAngle != null)
                {
                    m_toggleAngle.isOn = true;
                }
            }

            Subscribe();
        }
    }
}
