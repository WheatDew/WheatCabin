using Battlehub.RTCommon;
using Battlehub.RTHandles;
using Battlehub.RTSL.Interface;
using Battlehub.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor
{
    [AddComponentMenu(""), /*System.Obsolete*/]
    public class SceneViewImpl : MonoBehaviour
    {
        private Plane m_dragPlane;
        private IProjectAsync m_project;
        private GameObject m_prefabInstance;
        private HashSet<Transform> m_prefabInstanceTransforms;
        private Vector3 m_point;
        private GameObject m_dropTarget;
        private ProjectItem m_dragItem;

        protected GameObject PrefabInstance
        {
            get { return m_prefabInstance; }
        }

        private RuntimeWindow m_window;
        protected RuntimeWindow Window
        {
            get { return m_window; }
        }

        protected IRTE Editor
        {
            get { return m_window.Editor; }
        }

        protected IOCContainer IOCContainer
        {
            get { return m_window.IOCContainer; }
        }

        protected Camera Camera
        {
            get { return m_window.Camera; }
        }

        protected Pointer Pointer
        {
            get { return m_window.Pointer; }
        }

        protected Plane DragPlane
        {
            get { return m_dragPlane; }
            set { m_dragPlane = value; }
        }

        protected virtual void Start()
        {
            m_project = IOC.Resolve<IProjectAsync>();

            m_window = GetComponent<RuntimeWindow>();

            m_window.DragEnterEvent += OnDragEnter;
            m_window.DragLeaveEvent += OnDragLeave;
            m_window.DragEvent += OnDrag;
            m_window.DropEvent += OnDrop;   
        }

        protected virtual void OnDestroy()
        {
            if(m_window != null)
            {
                m_window.DragEnterEvent -= OnDragEnter;
                m_window.DragLeaveEvent -= OnDragLeave;
                m_window.DragEvent -= OnDrag;
                m_window.DropEvent -= OnDrop;
            }
        }

        protected async virtual void OnDragEnter(PointerEventData pointerEventData)
        {
            if (m_prefabInstance != null)
            {
                return;
            }

            if (ProjectItem.IsAssetItem(Editor.DragDrop.DragObjects[0]))
            {
                ProjectItem assetItem = (ProjectItem)Editor.DragDrop.DragObjects[0];
                if (m_project.Utils.ToType(assetItem) == typeof(GameObject))
                {
                    Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
                    
                    Editor.IsBusy = true;
                    var obj = await m_project.Safe.LoadAsync(new[] { assetItem });
                    Editor.IsBusy = false;
                    
                    OnAssetItemLoaded(Error.NoError, obj);

                    m_dragItem = null;
                }
                else if (m_project.Utils.ToType(assetItem) == typeof(Material))
                {
                    m_dragItem = assetItem;
                }
            }
            else if(Editor.DragDrop.DragObjects[0] is IToolCmd)
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
            }
        }

        protected virtual void OnAssetItemLoaded(Error error, Object[] obj)
        {
            if(error.HasError)
            {
                Debug.LogError(error.ToString());
                m_prefabInstance = null;
                m_prefabInstanceTransforms = null;
            }
            else
            {
                if (obj[0] is GameObject)
                {
                    CreateDragPlane();

                    GameObject prefab = (GameObject)obj[0];
                    bool wasPrefabEnabled = prefab.activeSelf;
                    prefab.SetActive(false);

                    Vector3 point;
                    if (GetPointOnDragPlane(out point))
                    {
                        m_prefabInstance = InstantiatePrefab(prefab, point, prefab.GetComponent<Transform>().rotation);
                    }
                    else
                    {
                        m_prefabInstance = InstantiatePrefab(prefab, Vector3.zero, prefab.GetComponent<Transform>().rotation);
                    }

                    Editor.AddGameObjectToHierarchy(m_prefabInstance);
                    
                    m_prefabInstanceTransforms = new HashSet<Transform>(m_prefabInstance.GetComponentsInChildren<Transform>(true));

                    prefab.SetActive(wasPrefabEnabled);

                    ExposeToEditor exposeToEditor = ExposePrefabInstance(m_prefabInstance);
                    exposeToEditor.SetName(obj[0].name);

                    OnActivatePrefabInstance(m_prefabInstance);

                    if (!Editor.DragDrop.InProgress)
                    {
                        RecordUndo();
                        m_prefabInstance = null;
                        m_prefabInstanceTransforms = null;
                    }
                }
            }
        }

        protected virtual ExposeToEditor ExposePrefabInstance(GameObject prefabInstance)
        {
            ExposeToEditor exposeToEditor = prefabInstance.GetComponent<ExposeToEditor>();
            if(exposeToEditor == null)
            {
                exposeToEditor = prefabInstance.AddComponent<ExposeToEditor>();
            }
            return exposeToEditor;
        }

        private void CreateDragPlane()
        {
            IScenePivot scenePivot = IOCContainer.Resolve<IScenePivot>();
            Vector3 up;
            if (Mathf.Abs(Vector3.Dot(Camera.transform.up, Vector3.up)) > Mathf.Cos(Mathf.Deg2Rad))
            {
                up = Vector3.Cross(Camera.transform.right, Vector3.up);
            }
            else
            {
                up = Vector3.up;
            }
            m_dragPlane = GetDragPlane(scenePivot, up);
        }

        protected virtual GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Instantiate(prefab, position, rotation);
        }

        protected virtual void OnActivatePrefabInstance(GameObject prefabInstance)
        {
            prefabInstance.SetActive(true);
        }

        protected virtual void OnDragLeave(PointerEventData pointerEventData)
        {
            if (!Editor.IsBusy)
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
            }

            if (m_prefabInstance != null)
            {
                Destroy(m_prefabInstance);
                m_prefabInstance = null;
                m_prefabInstanceTransforms = null;
            }

            m_dragItem = null;
            m_dropTarget = null;
        }


        protected virtual void OnDrag(PointerEventData pointerEventData)
        {
            Vector3 point;
            if (GetPointOnDragPlane(out point))
            {
                m_point = point;
                if (m_prefabInstance != null)
                {

                    m_prefabInstance.transform.position = m_point;

                    RaycastHit hit = Physics.RaycastAll(Pointer).Where(h => !m_prefabInstanceTransforms.Contains(h.transform)).FirstOrDefault();
                    if (hit.transform != null)
                    {
                        m_prefabInstance.transform.position = hit.point;
                    }
                }
            }

            if (m_dragItem != null)
            {
                RaycastHit hitInfo;
                if (Physics.Raycast(Pointer, out hitInfo, float.MaxValue, Editor.CameraLayerSettings.RaycastMask))
                {
                    MeshRenderer renderer = hitInfo.collider.GetComponentInChildren<MeshRenderer>();
                    SkinnedMeshRenderer sRenderer = hitInfo.collider.GetComponentInChildren<SkinnedMeshRenderer>();

                    if (renderer != null || sRenderer != null)
                    {
                        Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
                        m_dropTarget = hitInfo.transform.gameObject;
                    }
                    else
                    {
                        Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
                        m_dropTarget = null;
                    }
                }
                else
                {
                    Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
                    m_dropTarget = null;
                }
            }
        }

        private async void LoadAsync()
        {
            MeshRenderer renderer = m_dropTarget.GetComponentInChildren<MeshRenderer>();
            SkinnedMeshRenderer sRenderer = m_dropTarget.GetComponentInChildren<SkinnedMeshRenderer>();

            if (renderer != null || sRenderer != null)
            {
                ProjectItem assetItem = (ProjectItem)Editor.DragDrop.DragObjects[0];
                Editor.IsBusy = true;
                Object[] obj;
                try
                {
                    obj = await m_project.Safe.LoadAsync(new[] { assetItem }); 
                }
                catch(System.Exception e)
                {
                    IWindowManager wm = IOC.Resolve<IWindowManager>();
                    if (wm != null)
                    {
                        wm.MessageBox("Unable to load asset item ", e.Message);
                        Debug.LogException(e);
                    }
                    return;
                }
                finally
                {
                    Editor.IsBusy = false;
                }
                
                if (obj[0] is Material)
                {
                    if (renderer != null)
                    {
                        Editor.Undo.BeginRecordValue(renderer, Strong.PropertyInfo((MeshRenderer x) => x.sharedMaterials, "sharedMaterials"));
                        Material[] materials = renderer.sharedMaterials;
                        for (int i = 0; i < materials.Length; ++i)
                        {
                            materials[i] = (Material)obj[0];
                        }
                        renderer.sharedMaterials = materials;
                    }

                    if (sRenderer != null)
                    {
                        Editor.Undo.BeginRecordValue(sRenderer, Strong.PropertyInfo((SkinnedMeshRenderer x) => x.sharedMaterials, "sharedMaterials"));
                        Material[] materials = sRenderer.sharedMaterials;
                        for (int i = 0; i < materials.Length; ++i)
                        {
                            materials[i] = (Material)obj[0];
                        }
                        sRenderer.sharedMaterials = materials;
                    }

                    if (renderer != null || sRenderer != null)
                    {
                        Editor.Undo.BeginRecord();
                    }

                    if (renderer != null)
                    {
                        Editor.Undo.EndRecordValue(renderer, Strong.PropertyInfo((MeshRenderer x) => x.sharedMaterials, "sharedMaterials"));
                    }

                    if (sRenderer != null)
                    {
                        Editor.Undo.EndRecordValue(sRenderer, Strong.PropertyInfo((SkinnedMeshRenderer x) => x.sharedMaterials, "sharedMaterials"));
                    }

                    if (renderer != null || sRenderer != null)
                    {
                        Editor.Undo.EndRecord();
                    }
                }
                return;
            }
        }

        protected virtual void OnDrop(PointerEventData pointerEventData)
        {
            if (m_prefabInstance != null)
            {
                RecordUndo();
                m_prefabInstance = null;
                m_prefabInstanceTransforms = null;
            }

            if (m_dropTarget != null)
            {
                LoadAsync();
                m_dropTarget = null;
                m_dragItem = null;
            }

            
            IToolCmd cmd = Editor.DragDrop.DragObjects.OfType<IToolCmd>().FirstOrDefault();
            if (cmd != null)
            {
                GameObject go = cmd.Run() as GameObject;
                if (go != null)
                {
                    CreateDragPlane();
                    Vector3 point;
                    if (GetPointOnDragPlane(out point))
                    {
                        m_point = point;

                        RaycastHit hit = Physics.RaycastAll(Pointer).FirstOrDefault();
                        if (hit.transform != null)
                        {
                            m_point = hit.point;
                        }

                        ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
                        go.transform.position = m_point + Vector3.up * exposeToEditor.Bounds.extents.y;

                        IRuntimeSelectionComponent selectionComponent = IOCContainer.Resolve<IRuntimeSelectionComponent>();
                        if (selectionComponent.CanSelect)
                        {
                            bool wasEnabled = Editor.Undo.Enabled;
                            Editor.Undo.Enabled = false;
                            Editor.Selection.activeGameObject = null;
                            Editor.Selection.activeGameObject = go;
                            Editor.Undo.Enabled = wasEnabled;
                        }    
                    }
                }
            }
        }

        protected virtual void RecordUndo()
        {
            ExposeToEditor exposeToEditor = m_prefabInstance.GetComponent<ExposeToEditor>();

            Editor.Undo.BeginRecord();
            Editor.Undo.RegisterCreatedObjects(new[] { exposeToEditor });

            IRuntimeSelectionComponent selectionComponent = IOCContainer.Resolve<IRuntimeSelectionComponent>();
            if(selectionComponent.CanSelect)
            {
                Editor.Selection.activeGameObject = m_prefabInstance;
            }
            Editor.Undo.EndRecord();
        }

        protected virtual Plane GetDragPlane(IScenePivot scenePivot, Vector3 up)
        {
            return new Plane(up, scenePivot.SecondaryPivot);
        }

        protected virtual bool GetPointOnDragPlane(out Vector3 point)
        {
            Ray ray = m_window.Pointer;
            float distance;
            if (m_dragPlane.Raycast(ray, out distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }
            point = Vector3.zero;
            return false;
        }
    }

}

