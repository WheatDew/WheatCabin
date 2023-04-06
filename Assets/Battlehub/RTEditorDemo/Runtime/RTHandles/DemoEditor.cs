using Battlehub.RTCommon;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTHandles.Demo
{
    [DefaultExecutionOrder(-10)]
    public class DemoEditor : SimpleEditor, IRTEState
    {
        [SerializeField]
        private Button m_focusButton = null;

        [SerializeField]
        private Button m_deleteButton = null;

        [SerializeField]
        private Button m_play = null;

        [SerializeField]
        private Button m_stop = null;

        [SerializeField]
        private GameObject m_components = null;

        [SerializeField]
        private GameObject m_ui = null;

        [SerializeField]
        private GameObject m_prefabSpawnPoints = null;

        [SerializeField]
        private GameObject m_editorCamera = null;

        [SerializeField]
        private GameObject m_gameCamera = null;

        private ResourcePreviewUtility m_resourcePreview;

        public bool IsCreated
        {
            get { return true; }
        }

        protected override void Awake()
        {
            base.Awake();

            IOC.Register<IRTEState>(this);
            m_resourcePreview = gameObject.AddComponent<ResourcePreviewUtility>();
            IOC.Register<IResourcePreviewUtility>(m_resourcePreview);
        }

        protected override void Start()
        {
            base.Start();
            Editor.IsOpened = true;
            Editor.IsPlaying = false;
            OnPlaymodeStateChanged();

            Editor.PlaymodeStateChanged += OnPlaymodeStateChanged;
            Editor.Selection.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(Editor != null)
            {
                Editor.PlaymodeStateChanged -= OnPlaymodeStateChanged;
                Editor.Selection.SelectionChanged -= OnSelectionChanged;
            }

            IOC.Unregister<IRTEState>(this);
            IOC.Unregister<IResourcePreviewUtility>(m_resourcePreview);
        }

        protected virtual void Update()
        {
            if(Editor.Input.GetKeyDown(KeyCode.Delete))
            {
                DeleteSelected();
            }
        }
        protected override void SubscribeUIEvents()
        {
            base.SubscribeUIEvents();

            if (m_play != null)
            {
                m_play.onClick.AddListener(OnPlayClick);
            }
            if(m_stop != null)
            {
                m_stop.onClick.AddListener(OnStopClick);
            }
            if(m_focusButton != null)
            {
                m_focusButton.onClick.AddListener(OnFocusClick);
            }  
            if(m_deleteButton != null)
            {
                m_deleteButton.onClick.AddListener(OnDeleteClick);
            }
        }

        protected override void UnsubscribeUIEvents()
        {
            base.UnsubscribeUIEvents();

            if (m_play != null)
            {
                m_play.onClick.RemoveListener(OnPlayClick);
            }
            if (m_stop != null)
            {
                m_stop.onClick.RemoveListener(OnStopClick);
            }
            if (m_focusButton != null)
            {
                m_focusButton.onClick.RemoveListener(OnFocusClick);
            }
            if(m_deleteButton != null)
            {
                m_deleteButton.onClick.RemoveListener(OnDeleteClick);
            }
        }

        protected virtual void OnPlaymodeStateChanged()
        {
            if (m_components != null)
            {
                m_components.SetActive(!Editor.IsPlaying);
            }
            if (m_ui != null)
            {
                m_ui.SetActive(!Editor.IsPlaying);
            }
            if(m_prefabSpawnPoints != null)
            {
                m_prefabSpawnPoints.SetActive(!Editor.IsPlaying);
            }
            if(m_stop != null)
            {
                m_stop.gameObject.SetActive(Editor.IsPlaying);
            }
            if(m_editorCamera != null)
            {
                m_editorCamera.SetActive(!Editor.IsPlaying);
            }
            if(m_gameCamera != null)
            {
                m_gameCamera.SetActive(Editor.IsPlaying);
            }
        }

        private void OnSelectionChanged(Object[] unselectedObjects)
        {
            if(m_focusButton != null)
            {
                m_focusButton.interactable = Editor.Selection.Length > 0;
            }

            if(m_deleteButton != null)
            {
                m_deleteButton.interactable = Editor.Selection.Length > 0;
            }
        }

        private void OnFocusClick()
        {
            IScenePivot scenePivot = Editor.GetWindow(RuntimeWindowType.Scene).IOCContainer.Resolve<IScenePivot>();
            scenePivot.Focus(FocusMode.Default);
        }

        private void OnPlayClick()
        {
            Editor.Undo.Purge();
            StartCoroutine(CoPlay());
        }

        private IEnumerator CoPlay()
        {
            yield return new WaitForEndOfFrame(); 
            Editor.IsPlaying = true;
        }

        private void OnStopClick()
        {
            Editor.IsPlaying = false;
        }

        private void OnDeleteClick()
        {
            DeleteSelected();
        }

        private void DeleteSelected()
        {
            if (Editor.Selection.Length > 0)
            {
                ExposeToEditor[] exposed = Editor.Selection.gameObjects
                    .Where(o => o != null)
                    .Select(o => o.GetComponent<ExposeToEditor>())
                    .Where(o => o != null)
                    .ToArray();

                Editor.Undo.BeginRecord();
                Editor.Selection.objects = null;
                Editor.Undo.DestroyObjects(exposed);
                Editor.Undo.EndRecord();
            }
        }

        #region IRTEState implementation
        public event System.Action<object> Created;
        public event System.Action<object> Destroyed;
        private void Use()
        {
            Created(null);
            Destroyed(null);
        }
        #endregion
    }

}

