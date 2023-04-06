using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene3
{
    /// <summary>
    ///  The class containing the reference to InputField and the parent panel
    /// </summary>
    public class Layer
    {
        public RectTransform Transform
        {
            get;
            set;
        }
        public TMP_InputField Annotation
        {
            get;
            set;
        }

        public Layer(RectTransform transform, TMP_InputField annotation)
        {
            Transform = transform;
            Annotation = annotation;

            //Rename game object on onEndEdit and record changes to undo stack
            Annotation.onEndEdit.AddListener(text =>
            {
                IRTE editor = IOC.Resolve<IRTE>();
                GameObject activeGo = editor.Selection.activeGameObject;
                if (activeGo != null)
                {
                    ExposeToEditor exposeToEditor = activeGo.GetComponent<ExposeToEditor>();
                  
                    //This callback is called to restore the input file text after an undo or redo.
                    Action onUndoRedo = () => annotation.text = activeGo.name;

                    //Record the old name of the game object
                    editor.Undo.BeginRecordValue(exposeToEditor, Strong.MemberInfo((ExposeToEditor x) => x.Name));

                    //Update game object name
                    exposeToEditor.Name = text;

                    //Record new name of the game object
                    editor.Undo.EndRecordValue(exposeToEditor, Strong.MemberInfo((ExposeToEditor x) => x.Name), onUndoRedo, onUndoRedo);
                }
            });
        }
    }

    /// <summary>
    /// This is a more complex example of a layer over a window (For a simpler example, see SimpleLayerExample.cs).
    /// In this example we create a layer that allows you to rename the selected object using InputField.
    /// </summary>
    public class AnnotationsLayerExample : RuntimeWindowExtension
    {
        /// <summary>
        /// InputField prefab
        /// </summary>
        [SerializeField]
        private TMP_InputField m_annotationPrefab;

        /// <summary>
        ///  Mapping RuntimeWindows to layers (Required because several windows of the same type can coexist simultaneously)
        /// </summary>
        private readonly Dictionary<RuntimeWindow, Layer> m_windowToLayer = new Dictionary<RuntimeWindow, Layer>();

        /// <summary>
        /// Selected renderers (needed to calculate the BoundingBox and annotation position over the selected objects)
        /// </summary>
        private IList<Renderer> m_renderers = new Renderer[0];

        /// <summary>
        /// Selected transforms
        /// </summary>
        private IList<Transform> m_transforms = new Transform[0];

        /// <summary>
        /// Reference to RuntimeEditor
        /// </summary>
        private IRTE m_editor;

        /// <summary>
        /// Type of window on top of which the layer will be created
        /// </summary>
        public override string WindowTypeName => BuiltInWindowNames.Scene;


        /// <summary>
        /// Initialize extension
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            m_editor = IOC.Resolve<IRTE>();
            m_editor.Selection.SelectionChanged += OnEditorSelectionChanged;
            m_editor.Object.NameChanged += OnObjectNameChanged;
        }

        /// <summary>
        /// Cleanup extension
        /// </summary>
        protected override void OnCleanup()
        {
            base.OnCleanup();

            if (m_editor.Selection != null)
            {
                m_editor.Selection.SelectionChanged -= OnEditorSelectionChanged;
                m_editor.Object.NameChanged -= OnObjectNameChanged;
                m_editor = null;
            }
        }

        /// <summary>
        /// Initialization code called for each window
        /// </summary>
        /// <param name="window"></param>
        protected override void Extend(RuntimeWindow window)
        {
            //Create a new layer
            GameObject layerGO = new GameObject("AnnotationsLayerExample");
            layerGO.transform.SetParent(window.ViewRoot);

            //Stretch the layer to fill the scene window
            RectTransform layerRT = layerGO.AddComponent<RectTransform>();
            layerRT.Stretch();

            //Create annotation
            var annotation = Instantiate(m_annotationPrefab, layerRT);
            annotation.gameObject.SetActive(false);

            //Save window -> (layer, annotation) mapping  for later use
            m_windowToLayer.Add(window, new Layer(layerRT, annotation));
        }


        /// <summary>
        /// Cleanup code called for each window
        /// </summary>
        /// <param name="window"></param>
        protected override void Cleanup(RuntimeWindow window)
        {
            if (m_windowToLayer.TryGetValue(window, out Layer layer))
            {
                //Destroy layer
                Destroy(layer.Transform.gameObject);

                //Remove window from m_windowToLayer dictionary
                m_windowToLayer.Remove(window);
            }
        }

        private void LateUpdate()
        {
            if (m_renderers.Count == 0)
            {
                return;
            }

            //Calculate bounding box of selected objects
            Bounds bounds = m_renderers[0].bounds;
            for(int i = 1; i < m_renderers.Count; ++i)
            {
                bounds.Encapsulate(m_renderers[i].bounds);
            }

            Vector3 position;
            if(m_transforms.Count == 1)
            {
                position = m_transforms[0].position;
                position.y = bounds.center.y + bounds.extents.y * 1.25f;
            }
            else
            {
                position = bounds.center + Vector3.up * bounds.extents.y * 1.25f;
            }
            
            //Set annotation position in each window
            foreach (KeyValuePair<RuntimeWindow, Layer> kvp in m_windowToLayer)
            {
                RuntimeWindow window = kvp.Key;
                Layer layer = kvp.Value;

                SetAnnotationPosition(window.Camera, layer.Transform, layer.Annotation.transform, position);
            }
        }


        /// <summary>
        ///Handling changes of the runtime editor selection
        /// </summary>
        private void OnEditorSelectionChanged(object[] unselectedObjects)
        {
            m_transforms = m_editor.Selection.GetTransforms();
            if(m_transforms == null)
            {
                m_transforms = new Transform[0];
            }

            HashSet<Transform> roots = new HashSet<Transform>();
            for(int i = 0; i < m_transforms.Count; ++i)
            {
                Transform tr = m_transforms[i];
                while (tr.parent != null)
                {
                    tr = tr.parent;
                }
                roots.Add(tr);
            }

            //Get selected rendeerers
            HashSet<Renderer> renderersHs = new HashSet<Renderer>();
            foreach(Renderer renderer in roots.SelectMany(t => t.GetComponentsInChildren<Renderer>()))
            {
                renderersHs.Add(renderer);
            }
            m_renderers = renderersHs.ToArray();

            //Initialize annotation text
            foreach (Layer layer in m_windowToLayer.Values)
            {
                layer.Transform.gameObject.SetActive(m_renderers.Count > 0);

                SetAnnotationText(layer, m_transforms);
            }
        }

        /// <summary>
        /// Handling the object name change event to synchronize annotations across multiple scenes
        /// </summary>
        /// <param name="obj"></param>
        private void OnObjectNameChanged(ExposeToEditor obj)
        {
            if(obj.gameObject == m_editor.Selection.activeGameObject)
            {
                foreach (Layer layer in m_windowToLayer.Values)
                {
                    SetAnnotationText(layer, m_transforms);
                }
            }
        }

        private void SetAnnotationPosition(Camera camera, RectTransform layer, Transform annotation, Vector3 position)
        {
            Vector2 screenPoint = camera.WorldToScreenPoint(position);

            bool isVisible = RectTransformUtility.ScreenPointToLocalPointInRectangle(layer, screenPoint, null, out Vector2 localPosition);  
            if (isVisible)
            {
                annotation.localPosition = localPosition;
            }

            annotation.gameObject.SetActive(isVisible);
        }

        private void SetAnnotationText(Layer layer, IList<Transform> selection)
        {
            layer.Annotation.text = selection.Count == 1 ? $"{selection[0].name}" : $"{selection.Count} objects selected";
            layer.Annotation.readOnly = selection.Count != 1;
        }

    }

}
