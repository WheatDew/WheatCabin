
using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTGizmos
{
    [DefaultExecutionOrder(-55)]
    public class LightGizmo : RTEComponent
    {
        private Light m_light;
        private LightType m_lightType;

        [SerializeField, HideInInspector]
        private RTEComponent m_gizmo;

        protected override void Awake()
        {
            base.Awake();        
            m_light = GetComponent<Light>();
            if(m_light == null)
            {
                Destroy(this);
            }
            else
            {
                m_lightType = m_light.type;
                CreateGizmo();
            }
        }

        protected override void Start()
        {
            base.Start();
            if(m_gizmo != null)
            {
                m_gizmo.Window = Window;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        
            if (m_gizmo != null)
            {
                Destroy(m_gizmo);
                m_gizmo = null;
            }
        }

        private void Update()
        {
            if(m_light == null)
            {
                Destroy(this);
            }
            else
            {
                if(m_lightType != m_light.type)
                {
                    m_lightType = m_light.type;
                    CreateGizmo();
                }
            }
        }

        private void CreateGizmo()
        {
            if(m_gizmo != null)
            {
                Destroy(m_gizmo);
                m_gizmo = null;
            }

            if(m_lightType == LightType.Point)
            {
                if(m_gizmo == null)
                {
                    PointLightGizmo gizmo = gameObject.AddComponent<PointLightGizmo>();
                    gizmo.Reset();
                    m_gizmo = gizmo;
                }
            }
            else if(m_lightType == LightType.Spot)
            {
                if(m_gizmo == null)
                {
                    SpotlightGizmo gizmo = gameObject.AddComponent<SpotlightGizmo>();
                    gizmo.Reset();
                    m_gizmo = gizmo;
                }
            }
            else if(m_lightType == LightType.Directional)
            {
                if(m_gizmo == null)
                {
                    DirectionalLightGizmo gizmo = gameObject.AddComponent<DirectionalLightGizmo>();
                    gizmo.Reset();
                    m_gizmo = gizmo;
                }
            }

            m_gizmo.Window = Window;
        }
    }

}
