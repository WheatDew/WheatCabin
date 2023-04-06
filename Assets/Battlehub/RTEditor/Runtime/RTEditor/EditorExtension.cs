using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.RTHandles;
using Battlehub.UIControls.DockPanels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Battlehub.RTEditor
{
    //For backward compatibility
    public class EditorOverride : EditorExtension
    {

    }

    public class EditorExtension : MonoBehaviour
    {
        private IRTEState m_rteState;
        private IRTE m_editor;
        private bool m_isInitialized;

        protected virtual void Awake()
        {
            m_rteState = IOC.Resolve<IRTEState>();
            if (m_rteState != null)
            {
                if (m_rteState.IsCreated)
                {
                    OnInit();
                    m_isInitialized = true;
                    OnEditorExist();
                    
                }
                else
                {
                    m_rteState.Created += OnEditorCreated;
                }
            }
            else
            {
                OnInit();
                m_isInitialized = true;
                OnEditorExist();
            }
        }

        protected virtual void OnDestroy()
        {
            if(m_isInitialized)
            {
                m_isInitialized = false;
                OnCleanup();
            }
            
            if (m_rteState != null)
            {
                m_rteState.Created -= OnEditorCreated;
            }

            if(m_editor != null)
            {
                m_editor.IsOpenedChanged -= OnIsOpenedChanged;
            }
        }

        /*[Obsolete("Use OnInit instead")]* TODO: change to private 03.03.2021*/ 
        protected virtual void OnEditorExist()
        {
            m_editor = IOC.Resolve<IRTE>();
            m_editor.IsOpenedChanged += OnIsOpenedChanged;
            if (m_editor.IsOpened)
            {
                OnEditorOpened();
            }
        }

        private void OnIsOpenedChanged()
        {
            if (m_editor.IsOpened)
            {
                OnEditorOpened();
            }
            else
            {
                m_editor.IsOpenedChanged -= OnIsOpenedChanged;
                OnEditorClosed();
                if (m_isInitialized)
                {
                    m_isInitialized = false;
                    OnCleanup();
                }
            }
        }

        protected virtual void OnEditorCreated(object obj)
        {
            OnInit();
            m_isInitialized = true;
            OnEditorExist();
        }

        protected virtual void OnEditorOpened()
        {

        }

        /*[Obsolete("Use OnCleanup") instead]* TODO: change to private 03.03.2021*/
        protected virtual void OnEditorClosed()
        {

        }

        protected void RunNextFrame(Action action)
        {
            StartCoroutine(CoWaitForEndOfFrame(action));
        }

        private IEnumerator CoWaitForEndOfFrame(Action action)
        {
            yield return new WaitForEndOfFrame();
            action();
        }

        protected virtual void OnInit()
        {

        }

        protected virtual void OnCleanup()
        {

        }

        protected void EnableStyling(GameObject prefab)
        {
            IRTEAppearance appearance = IOC.Resolve<IRTEAppearance>();
            if (appearance != null && prefab != null)
            {
                appearance.ApplyColors(prefab);
                appearance.RegisterPrefab(prefab);
            }
        }
    }


    [DefaultExecutionOrder(-91)]
    public class LayoutExtension : EditorExtension
    {
        private IWindowManager m_wm;
        private ILayoutStorageModel m_layoutStorage;

        [SerializeField, FormerlySerializedAs("m_savedLayoutName")]
        private string m_persitentLayoutName;
        protected string PersistentLayoutName
        {
            get { return m_persitentLayoutName; }
            set { m_persitentLayoutName = value; }
        }

        [SerializeField, FormerlySerializedAs("m_saveLayout")]
        private bool m_persistentLayout = false;

        protected bool PersistentLayout
        {
            get { return m_persistentLayout; }
            set { m_persistentLayout = value; }
        }

        private bool m_setLayout = true;
        protected override void OnEditorCreated(object obj)
        {
            m_setLayout = false;
            base.OnEditorCreated(obj);
        }

        protected override void OnInit()
        {          
            m_wm = IOC.Resolve<IWindowManager>();
            m_wm.AfterLayout += OnAfterLayout;
            m_layoutStorage = IOC.Resolve<ILayoutStorageModel>();
            if (string.IsNullOrEmpty(m_persitentLayoutName))
            {
                m_persitentLayoutName = m_layoutStorage.DefaultLayoutName;
            }

            base.OnInit();
        }

        protected override void OnEditorExist()
        {           
            base.OnEditorExist();

            OnRegisterWindows(m_wm);
            OverrideDefaultLayout();

            if(m_setLayout)
            {
                IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
                if (editor.IsOpened)
                {
                    m_wm = IOC.Resolve<IWindowManager>();
                    m_wm.SetLayout(DefaultLayout, RuntimeWindowType.Scene.ToString());
                }
            }
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();
            if (m_persistentLayout)
            {
                m_layoutStorage.SaveLayout(m_persitentLayoutName, m_wm.GetLayout());
            }

            if(m_wm != null)
            {
                m_wm.AfterLayout -= OnAfterLayout;
                m_wm = null;
            }

            m_layoutStorage = null;
            m_setLayout = true;
        }


        private void OnApplicationQuit()
        {
            if (m_wm != null && m_persistentLayout)
            {
                m_layoutStorage.SaveLayout(m_persitentLayoutName, m_wm.GetLayout());
            }
        }

        private void OverrideDefaultLayout()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.OverrideDefaultLayout(DefaultLayout, RuntimeWindowType.Scene.ToString());
        }

        private LayoutInfo DefaultLayout(IWindowManager wm)
        {
            OnBeforeBuildLayout(wm);

            if(m_persistentLayout)
            {
                if (m_layoutStorage.LayoutExists(m_persitentLayoutName))
                {
                    LayoutInfo layout = GetSavedLayoutInfo(wm);
                    if (layout != null)
                    {
                        if (wm.ValidateLayout(layout))
                        {
                            return layout;
                        }
                        else
                        {
                            Debug.LogWarning("Saved layout is corrupted. Restoring default layout");
                            Transform[] allWindows = wm.GetWindows();
                            for (int i = 0; i < allWindows.Length; ++i)
                            {
                                wm.DestroyWindow(allWindows[i]);
                            }
                        }
                    }
                }
            }

            return GetLayoutInfo(m_wm);
        }

        protected virtual void OnRegisterWindows(IWindowManager wm)
        {

        }

        protected virtual void OnBeforeBuildLayout(IWindowManager wm)
        {

        }

        protected virtual void OnAfterBuildLayout(IWindowManager wm)
        {

        }

        private async void OnAfterLayout(IWindowManager wm)
        {
            m_wm.AfterLayout -= OnAfterLayout;
            await Task.Yield();
            await Task.Yield();
            OnAfterBuildLayout(wm);
        }

        protected virtual LayoutInfo GetSavedLayoutInfo(IWindowManager wm)
        {
            return m_layoutStorage.LoadLayout(m_persitentLayoutName);
        }

        protected virtual LayoutInfo GetLayoutInfo(IWindowManager wm)
        {
            return wm.GetBuiltInDefaultLayout();
        }

        protected virtual void DeleteLayout()
        {
            m_layoutStorage.DeleteLayout(m_persitentLayoutName);
        }

        protected void RegisterWindow(IWindowManager wm, string typeName, string header, Sprite icon, GameObject prefab, bool isDialog = false, int maxWindows = int.MaxValue)
        {
            wm.RegisterWindow(new CustomWindowDescriptor 
            {
                IsDialog = isDialog,
                TypeName = typeName,
                Descriptor = new WindowDescriptor
                {
                    Header = header,
                    Icon = icon,
                    MaxWindows = maxWindows,
                    ContentPrefab = prefab
                }
            });
        }

        protected void RegisterWindow(IWindowManager wm, string typeName, string header, Sprite icon, GameObject tabPrefab, GameObject prefab, bool isDialog = false, int maxWindows = int.MaxValue)
        {
            wm.RegisterWindow(new CustomWindowDescriptor
            {
                IsDialog = isDialog,
                TypeName = typeName,
                Descriptor = new WindowDescriptor
                {
                    Header = header,
                    Icon = icon,
                    MaxWindows = maxWindows,
                    ContentPrefab = prefab,
                    TabPrefab = tabPrefab
                }
            });
        }
    }

    public abstract class RuntimeWindowExtension : EditorExtension
    {
        private IWindowManager m_wm;
        private Dictionary<Transform, RuntimeWindow> m_toRuntimeWindow; 

        public abstract string WindowTypeName
        {
            get;
        }

        protected override void OnInit()
        {
            base.OnInit();

            m_toRuntimeWindow = new Dictionary<Transform, RuntimeWindow>();

            m_wm = IOC.Resolve<IWindowManager>();

            m_wm.WindowCreated += OnWindowCreated;
            m_wm.WindowDestroyed += OnWindowDestroyed;
            m_wm.AfterLayout += OnAfterLayout;
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();

            m_toRuntimeWindow = null;

            m_wm.WindowCreated -= OnWindowCreated;
            m_wm.WindowDestroyed -= OnWindowDestroyed;
            m_wm.AfterLayout -= OnAfterLayout;

            m_wm = null;
        }

 
        private void OnWindowCreated(Transform content)
        {
            RuntimeWindow window = content.GetComponentInChildren<RuntimeWindow>(true);
            if (window != null && m_wm.GetWindowTypeName(content) == WindowTypeName.ToLower())
            {
                m_toRuntimeWindow.Add(content, window);
                Extend(window);
            }
        }

        private void OnWindowDestroyed(Transform content)
        {
            if (m_toRuntimeWindow.TryGetValue(content, out RuntimeWindow window))
            {
                Cleanup(window);
#pragma warning disable CS0618
                CleanUp(window);
#pragma warning restore CS0618
            }
        }

        private void OnAfterLayout(IWindowManager wm)
        {
            foreach (Transform windowTransform in wm.GetWindows(WindowTypeName))
            {
                RuntimeWindow window = windowTransform.GetComponentInChildren<RuntimeWindow>(true);
                if (window != null && wm.GetWindowTypeName(windowTransform) == WindowTypeName.ToLower())
                {
                    m_toRuntimeWindow.Add(windowTransform, window);
                    Extend(window);
                }
            }
        }

        protected abstract void Extend(RuntimeWindow window);

        protected virtual void Cleanup(RuntimeWindow window)
        {
        }

        [Obsolete("Cleanup")]
        protected virtual void CleanUp(RuntimeWindow window)
        {
        }
    }

    public class SceneComponentExtension : EditorExtension
    {
        private IRTE m_editor;
        private IRuntimeSceneComponent m_sceneComponent;

        protected override void OnInit()
        {
            base.OnInit();
            m_editor = IOC.Resolve<IRTE>();
            m_editor.ActiveWindowChanged += OnActiveWindowChanged;
            OnActiveWindowChanged(m_editor.ActiveWindow);
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();
            m_editor.ActiveWindowChanged -= OnActiveWindowChanged;
            if (m_sceneComponent != null)
            {
                OnSceneDeactivated(m_sceneComponent);
            }
        }

        private void OnActiveWindowChanged(RuntimeWindow deactivatedWindow)
        {
            if (m_sceneComponent != null)
            {
                OnSceneDeactivated(m_sceneComponent);
            }

            if (m_editor.ActiveWindow != null)
            {
                m_sceneComponent = m_editor.ActiveWindow.IOCContainer.Resolve<IRuntimeSceneComponent>();
                if (m_sceneComponent != null)
                {
                    OnSceneActivated(m_sceneComponent);
                }
            }
        }

        protected virtual void OnSceneActivated(IRuntimeSceneComponent sceneComponent)
        {
        }

        protected virtual void OnSceneDeactivated(IRuntimeSceneComponent sceneComponent)
        {
        }
    }
}

