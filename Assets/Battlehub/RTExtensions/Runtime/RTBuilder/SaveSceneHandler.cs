using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTSL.Interface;
using Battlehub.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battlehub.RTBuilder
{
    public class SaveSceneHandler : EditorExtension
    {
        private IProjectAsync m_project;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();

            m_project = IOC.Resolve<IProjectAsync>();
            m_project.Events.BeginSave += BeginSave;
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();

            if(m_project != null)
            {
                m_project.Events.BeginSave -= BeginSave;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (m_project != null)
            {
                m_project.Events.BeginSave -= BeginSave;
            }
        }

        private void BeginSave(object sender, ProjectEventArgs<object[]> e)
        {
            object[] result = e.Payload;
            if (result != null && result.Length > 0 && result[0] is Scene)
            {
                IEnumerable<PBMesh> pbMeshes = Resources.FindObjectsOfTypeAll<PBMesh>().Where(mesh => !mesh.gameObject.IsPrefab());
                foreach (PBMesh pbMesh in pbMeshes)
                {
                    MeshFilter filter = pbMesh.GetComponent<MeshFilter>();
                    if (filter != null)
                    {
                        //Do not save probuilderized meshes
                        filter.sharedMesh.hideFlags = HideFlags.DontSave;
                    }
                }
            }
        }
    }
}
