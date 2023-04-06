using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTEditor.Demo;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public interface ITerrainAreaTool
    {
        bool IsActive
        {
            get;
            set;
        }

        Texture2D Brush
        {
            get;
            set;
        }

        float BrushOpacity
        {
            get;
            set;
        }

        IEnumerable<TerrainAreaHandle> Handles
        {
            get;
        }
    }

    [DefaultExecutionOrder(1)]
    public class TerrainAreaTool : CustomHandleExtension<TerrainAreaHandle>, ITerrainAreaTool
    {
        private ITerrainAreaProjector m_projector;

        public Texture2D Brush
        {
            get { return m_projector.Brush; }
            set 
            {
                if(m_projector == null)
                {
                    m_projector = IOC.Resolve<ITerrainAreaProjector>();
                }

                m_projector.Brush = value; 
            }
        }

        public float BrushOpacity
        {
            get { return m_projector.BrushOpacity * 100; }
            set
            {
                if (m_projector == null)
                {
                    m_projector = IOC.Resolve<ITerrainAreaProjector>();
                }

                m_projector.BrushOpacity = value / 100;
            }
        }

        protected override void Activate()
        {
            base.Activate();
            m_projector = IOC.Resolve<ITerrainAreaProjector>();
        }

        protected override void Deactivate()
        {
            base.Deactivate();
            if(m_projector != null)
            {
                m_projector.Destroy();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            IOC.RegisterFallback<ITerrainAreaTool>(this);
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            IOC.UnregisterFallback<ITerrainAreaTool>(this);
        }

        protected override TerrainAreaHandle CreateHandle(RuntimeWindow scene)
        {
            //TerrainAreaHandle exisitingHandle = GetComponentInChildren<TerrainAreaHandle>();
            TerrainAreaHandle handle = base.CreateHandle(scene);

            IRTE rte = IOC.Resolve<IRTE>();
            if(rte.Selection.gameObjects != null)
            {
                handle.Targets = rte.Selection.gameObjects.Select(go => go.transform).ToArray();
            }

            //if (exisitingHandle != null)
            //{
            //    handle.Position = exisitingHandle.Position;
            //}

            ISettingsComponent settingsComponent = IOC.Resolve<ISettingsComponent>();
            if(settingsComponent != null)
            {
                handle.HandleScale = 4.5f * settingsComponent.UIScale;
            }

            IRenderPipelineCameraUtility cameraUtility = IOC.Resolve<IRenderPipelineCameraUtility>();
            if (cameraUtility != null)
            {
                cameraUtility.RequiresDepthTexture(scene.Camera, true);
            }

            return handle;
        }
        

        protected override void SetCurrentTool(RuntimeTool tool)
        {
            
        }
    }
}
