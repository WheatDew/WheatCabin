using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public class DuplicateObjectsHandler : EditorExtension
    {
        private IRTE m_rte;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();

            m_rte = IOC.Resolve<IRTE>();
            m_rte.ObjectsDuplicated += OnObjectsDuplicated;
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            m_rte.ObjectsDuplicated -= OnObjectsDuplicated;
        }

        private void OnObjectsDuplicated(GameObject[] original)
        {
            GameObject[] gameObjects = m_rte.Selection.gameObjects;
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject go = gameObjects[i];
                if (go != null)
                {

                    WireframeMesh wireframe = go.GetComponentInChildren<WireframeMesh>(true);
                    if (wireframe != null)
                    {
                        DestroyImmediate(wireframe.gameObject);

                    }
                    PBMesh pbMesh = go.GetComponent<PBMesh>();
                    if (pbMesh != null)
                    {
                        pbMesh.Refresh();
                    }
                }
            }
        }
    }
}
