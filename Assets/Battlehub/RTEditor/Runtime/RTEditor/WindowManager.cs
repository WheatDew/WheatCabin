using Battlehub.RTCommon;
using Battlehub.RTHandles;
using Battlehub.UIControls.Dialogs;
using Battlehub.UIControls.DockPanels;
using Battlehub.UIControls.MenuControl;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    ///<summary>
    /// The window manager allows you to create complex windows, such as an inspector or scene, and simple dialogs, such as a message or confirmation window.
    /// 
    /// The difference between a dialog and a window is quite subtle.The content of a dialog can be anything and it cannot be docked. 
    /// To be considered a window or dialog box, a RuntimeWindow component must be attached to the game object. 
    /// When RuntimeWindow is activated, other windows are deactivated. A dialog cannot deactivate a window.
    /// </summary>
    public interface IWindowManager
    {
        /// <summary>
        /// Returns true if dialog opened.
        /// </summary>
        bool IsDialogOpened
        {
            get;
        }

        /// <summary>
        /// Root panel for popups and floating windows.
        /// </summary>
        RectTransform PopupRoot
        {
            get;
        }

        /// <summary>
        /// The root transform for additional window components. Each window can have one or more additional components that must not have UI Canvas as a parent. 
        /// </summary>
        Transform ComponentsRoot
        {
            get;
        }

        /// <summary>
        /// Triggers when the layout stage is complete, when all the windows are positioned and ready to use.
        /// </summary>
        event Action<IWindowManager> AfterLayout;

        /// <summary>
        /// Triggers after creating a new window.
        /// </summary>
        event Action<Transform> WindowCreated;

        /// <summary>
        /// Triggers after the window is destroyed.
        /// </summary>
        event Action<Transform> WindowDestroyed;

        /// <summary>
        /// IWindowManager can be used to work with multiple workspaces.
        /// </summary>
        Workspace ActiveWorkspace
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a LayoutInfo object for the window of <paramref name="windowTypeName"/> (creates a window as a side effect).
        /// 
        /// This function and its overrides should be used when creating different "layouts". See BuiltInWindowNames for common window type names.
        /// </summary>
        /// <param name="windowTypeName">Type of window to be created</param>
        /// <returns>LayoutInfo.</returns>
        LayoutInfo CreateLayoutInfo(string windowTypeName);

        /// <summary>
        /// Creates a LayoutInfo object for the window of <paramref name="windowTypeName"/> (creates a window as a side effect).
        /// </summary>
        /// <param name="windowTypeName">Type of window to be created.</param>
        /// <param name="args">Optional arguments can be obtained using the RuntimeWindow.Args property.</param>
        /// <returns>LayoutInfo.</returns>
        LayoutInfo CreateLayoutInfo(string windowTypeName, string args);

        /// <summary>
        /// Creates a LayoutInfo object for the window (creates a window as a side effect).
        /// </summary>
        /// <param name="content">Transform of the window.</param>
        /// <param name="desc">Object that describes an icon, a header, a tab and provides information about how many windows of that type can be created.</param>
        /// <returns>LayoutInfo.</returns>
        LayoutInfo CreateLayoutInfo(Transform content, WindowDescriptor desc);

        /// <summary>
        /// Creates a LayoutInfo object for the window (creates a window as a side effect).
        /// </summary>
        /// <param name="content">Transform of the window.</param>
        /// <param name="header">Header text.</param>
        /// <param name="icon">Header icon.</param>
        /// <returns>LayoutInfo.</returns>
        LayoutInfo CreateLayoutInfo(Transform content, string header, Sprite icon);

        /// <summary>
        /// Validates layout.
        /// </summary>
        /// <param name="layout">LayoutInfo object.</param>
        /// <returns>True if layout is valid.</returns>
        bool ValidateLayout(LayoutInfo layout);

        /// <summary>
        /// Overrides default layout builder function.
        /// </summary>
        /// <param name="layoutBuilderFunc">Layout builder function.</param>
        /// <param name="activateWindowOfType">Type of the window to activate after building layout.</param>
        void OverrideDefaultLayout(Func<IWindowManager, LayoutInfo> layoutBuilderFunc, string activateWindowOfType = null);

        /// <summary>
        /// Rebuilds the layout using the default layout builder function.
        /// </summary>
        void SetDefaultLayout();

        /// <summary>
        /// Builds layout using layout builder function.
        /// </summary>
        /// <param name="layoutBuilderFunc">Layout builder function.</param>
        /// <param name="activateWindowOfType">Type of the window to activate after building layout.</param>
        void SetLayout(Func<LayoutInfo> layoutBuilderFunc, string activateWindowOfType = null);

        /// <summary>
        /// Builds layout using layout builder function.
        /// </summary>
        /// <param name="layoutBuilderFunc">Layout builder function.</param>
        /// <param name="activateWindowOfType">Type of the window to activate after building layout.</param>
        void SetLayout(Func<IWindowManager, LayoutInfo> layoutBuilderFunc, string activateWindowOfType = null);

        /// <summary>
        /// Creates LayoutInfo object describing current layout.
        /// </summary>
        /// <returns></returns>
        LayoutInfo GetLayout();

        /// <summary>
        /// Overrides the WindowDescriptor of a registered window.
        /// </summary>
        /// <param name="windowTypeName">Window type name.</param>
        /// <param name="descriptor">Window descriptor.</param>
        void OverrideWindow(string windowTypeName, WindowDescriptor descriptor);

        /// <summary>
        /// Overrides the prefab of a registered window. 
        /// </summary>
        /// <param name="windowTypeName">Window type name.</param>
        /// <param name="descriptor">Window descriptor.</param>
        void OverrideWindow(string windowTypeName, GameObject prefab);

        /// <summary>
        /// Overrides header tools (play, pause, pivot mode and various toggles).
        /// </summary>
        /// <param name="toolsPrefab">Tools prefab transform.</param>
        void OverrideTools(Transform toolsPrefab);

        /// <summary>
        /// Sets header tools.
        /// </summary>
        /// <param name="tools">Content transform.</param>
        void SetTools(Transform tools);

        /// <summary>
        /// Sets left bar.
        /// </summary>
        /// <param name="leftBar">Left bar transform.</param>
        void SetLeftBar(Transform leftBar);

        /// <summary>
        /// Sets right bar.
        /// </summary>
        /// <param name="rightBar">Right bar transform.</param>
        void SetRightBar(Transform rightBar);

        /// <summary>
        /// Sets top bar.
        /// </summary>
        /// <param name="topBar">Top bar transform.</param>
        void SetTopBar(Transform topBar);

        /// <summary>
        /// Sets bottom bar.
        /// </summary>
        /// <param name="bottomBar">Bottom bar transform.</param>
        void SetBottomBar(Transform bottomBar);

        /// <summary>
        /// Determines whether a window of the type registered.
        /// </summary>
        /// <param name="windowTypeName">Window type name.</param>
        /// <returns>True if window of type registered.</returns>
        bool IsWindowRegistered(string windowTypeName);

        /// <summary>
        /// Registers window.
        /// </summary>
        /// <param name="desc">Window descriptor.</param>
        /// <returns>True if registration succeeded.</returns>
        bool RegisterWindow(CustomWindowDescriptor desc);

        /// <summary>
        /// Gets window descriptor.
        /// </summary>
        /// <param name="windowTypeName">Window type name.</param>
        /// <param name="isDialog">Is true if the window is a dialog.</param>
        /// <returns>Window descriptor.</returns>
        WindowDescriptor GetWindowDescriptor(string windowTypeName, out bool isDialog);
        
        /// <summary>
        /// Gets window type name.
        /// </summary>
        /// <param name="content">Transform of the window.</param>
        /// <returns>Window type name</returns>
        string GetWindowTypeName(Transform content);

        /// <summary>
        /// Gets window transform by type.
        /// </summary>
        /// <param name="windowTypeName">Window type name.</param>
        /// <returns>Window transform.</returns>
        Transform GetWindow(string windowTypeName);

        /// <summary>
        /// Gets transforms of all windows .
        /// </summary>
        /// <returns>Array of transforms.</returns>
        Transform[] GetWindows();

        /// <summary>
        /// Gets transfroms of windows of type.
        /// </summary>
        /// <param name="windowTypeName">Window type name.</param>
        /// <returns>Array of transforms.</returns>
        Transform[] GetWindows(string windowTypeName);

        /// <summary>
        /// Gets extra components associated with the window.
        /// </summary>
        /// <param name="content">Transform of the window.</param>
        /// <returns>Array of extra components.</returns>
        Transform[] GetComponents(Transform content);

        /// <summary>
        /// Checks if window of type exists.
        /// </summary>
        /// <param name="windowTypeName">Window type name.</param>
        /// <returns>True if window exists.</returns>
        bool Exists(string windowTypeName);

        /// <summary>
        /// Checks if the window of this type is in an active state (e.g., if it can receive input).
        /// </summary>
        /// <param name="windowTypeName">Window type name.</param>
        /// <returns>True if window is active.</returns>
        bool IsActive(string windowTypeName);

        /// <summary>
        /// Checks if the window in in active state (e.g., if it can receive input).
        /// </summary>
        /// <param name="content">Transform of the window.</param>
        /// <returns>True if window is active.</returns>
        bool IsActive(Transform content);

        /// <summary>
        /// Finds the uppermost window to which pointer is pointing.
        /// </summary>
        /// <param name="exceptWindow">Except window.</param>
        /// <returns>Transform of the uppermost window to which pointer is pointing.</returns>
        Transform FindPointerOverWindow(RuntimeWindow exceptWindow);

        /// <summary>
        /// Activates window of type.
        /// </summary>
        /// <param name="windowTypeName">Window type name.</param>
        /// <returns>True if the window is activated.</returns>
        bool ActivateWindow(string windowTypeName);

        /// <summary>
        /// Activates window of type.
        /// </summary>
        /// <param name="windowTypeName">Window type name.</param>
        /// <returns>True if the window is activated.</returns>
        bool ActivateWindow(Transform content);

        /// <summary>
        /// Creates window of type (this function is mainly used when creating layouts).
        /// </summary>
        /// <param name="windowTypeName">Window type name. See BuiltInWindowNames for common window type names.</param>
        /// <param name="wd">The descriptor of the created window.</param>
        /// <returns>Transform of the created window.</returns>
        Transform CreateWindow(string windowTypeName, out WindowDescriptor wd);

        /// <summary>
        /// Creates window of type (this function is mainly used when creating layouts).
        /// </summary>
        /// <param name="windowTypeName">Window type name. See BuiltInWindowNames for common window type names.</param>
        /// <param name="wd">The descriptor of the created window.</param>
        /// <param name="isDialog">True if created window is dialog.</param>
        /// <returns>Transform of the created window.</returns>
        Transform CreateWindow(string windowTypeName, out WindowDescriptor wd, out bool isDialog);
        //[Obsolete]
        Transform CreateWindow(string windowTypeName, out WindowDescriptor wd, out GameObject content, out bool isDialog);

        /// <summary>
        /// Creates window of type.
        /// </summary>
        /// <param name="windowTypeName">Window type name.</param>
        /// <param name="isFree">True if windows are to be created in the floating state, and false if in the docked state.</param>
        /// <param name="splitType">Determines how to dock the window.</param>
        /// <param name="flexibleSize">Window size relative to the docked neighbor [0, 1].</param>
        /// <param name="parentWindow">Transform of the window with which a group of tabs will be formed or which will be the neighbor of the created window.</param>
        /// <returns>Transform of the created window.</returns>
        Transform CreateWindow(string windowTypeName, bool isFree = true, RegionSplitType splitType = RegionSplitType.None, float flexibleSize = 0.3f, Transform parentWindow = null);

        /// <summary>
        /// Creates popup window of type.
        /// </summary>
        /// <param name="windowTypeName">Window type name.</param>
        /// <param name="canResize">True to be able to resize the popup.</param>
        /// <param name="minWidth">Minimum width of the pop-up window.</param>
        /// <param name="minHeight">Minimum height of the pop-up window.</param>
        /// <returns>Transform of the created popup window.</returns>
        Transform CreatePopup(string windowTypeName, bool canResize = false, float minWidth = 10, float minHeight = 10);

        /// <summary>
        /// Transform a screen space point to a position in the local space of a RectTransform that is on the plane of its rectangle.
        /// </summary>
        /// <param name="rectTransform">The RectTransform to find a point inside.</param>
        /// <param name="screenPoint">Screen space position.</param>
        /// <param name="position">Point in local space of the rect transform.</param>
        /// <returns>true if the plane of the RectTransform is hit, regardless of whether the point is inside the rectangle.</returns>
        bool ScreenPointToLocalPointInRectangle(RectTransform rectTransform, Vector3 screenPoint, out Vector2 position);

        /// <summary>
        /// Creates popup window of type
        /// </summary>
        /// <param name="windowTypeName">Window type name.</param>
        /// <param name="position">Position in IWindowManager.PopupRoot coordinates.</param>
        /// <param name="canResize">True to be able to resize the popup.</param>
        /// <param name="minWidth">Minimum width of the pop-up window.</param>
        /// <param name="minHeight">Minimum height of the pop-up window.</param>
        /// <returns>Transform of the created popup window.</returns>
        Transform CreatePopup(string windowTypeName, Vector2 position, bool canResize = false, float minWidth = 10, float minHeight = 10);

        /// <summary>
        /// Creates dropdown window of type.
        /// </summary>
        /// <param name="windowTypeName">Window type name.</param>
        /// <param name="anchor">The RectTransform of dropdown anchor.</param>
        /// <param name="canResize">True to be able to resize the dropdown.</param>
        /// <param name="minWidth">Minimum width of the pop-up window.</param>
        /// <param name="minHeight">Minimum height of the pop-up window.</param>
        /// <returns></returns>
        Transform CreateDropdown(string windowTypeName, RectTransform anchor, bool canResize = false, float minWidth = 10, float minHeight = 10);

        /// <summary>
        /// Sets window arguments.
        /// </summary>
        /// <param name="content">Transform of the window.</param>
        /// <param name="args">Arguments.</param>
        void SetWindowArgs(Transform content, string args);

        /// <summary>
        /// Destroys window.
        /// </summary>
        /// <param name="conent">Transform of the window.</param>
        void DestroyWindow(Transform conent);

        /// <summary>
        /// Destroys window of type.
        /// </summary>
        /// <param name="windowTypeName">Window type name.</param>
        void DestroyWindowsOfType(string windowTypeName);

        /// <summary>
        /// Creates dialog window.
        /// </summary>
        /// <param name="windowTypeName">Window type name.</param>
        /// <param name="header">Dialog header text.</param>
        /// <param name="okAction">The action to take when the OK button is clicked.</param>
        /// <param name="cancelAction">The action to take when the Cancel button is clicked.</param>
        /// <param name="canResize">True to be able to resize the dialog.</param>
        /// <param name="minWidth">Minimum width of the pop-up window.</param>
        /// <param name="minHeight">Minimum height of the pop-up window.</param>
        /// <returns></returns>
        Transform CreateDialogWindow(string windowTypeName, string header, DialogAction<DialogCancelArgs> okAction, DialogAction<DialogCancelArgs> cancelAction,
             bool canResize,
             float minWidth = 50,
             float minHeight = 50);

        /// <summary>
        /// Creates dialog window.
        /// </summary>
        /// <param name="windowTypeName">Window type name.</param>
        /// <param name="header">Dialog header text.</param>
        /// <param name="okAction">The action to take when the OK button is clicked.</param>
        /// <param name="cancelAction">he action to take when the Cancel button is clicked.</param>
        /// <param name="minWidth">Minimum width of the pop-up window.</param>
        /// <param name="minHeight">Minimum height of the pop-up window.</param>
        /// <param name="preferredWidth">Perferred width of the pop-up window.</param>
        /// <param name="preferredHeight">Perferred height of the pop-up window.</param>
        /// <param name="canResize">True to be able to resize the dialog.</param>
        /// <returns></returns>
        Transform CreateDialogWindow(string windowTypeName, string header, DialogAction<DialogCancelArgs> okAction = null, DialogAction<DialogCancelArgs> cancelAction = null,
             float minWidth = 250,
             float minHeight = 250,
             float preferredWidth = 700,
             float preferredHeight = 400,
             bool canResize = true);

        /// <summary>
        /// Destroys the topmost dialog window.
        /// </summary>
        void DestroyDialogWindow();

        /// <summary>
        /// Creates message box.
        /// </summary>
        /// <param name="header">Header text.</param>
        /// <param name="text">Message text.</param>
        /// <param name="ok">Ok action.</param>
        void MessageBox(string header, string text, DialogAction<DialogCancelArgs> ok = null);

        /// <summary>
        /// Creates message box.
        /// </summary>
        /// <param name="icon">Header icon.</param>
        /// <param name="header">Header text.</param>
        /// <param name="text">Message text.</param>
        /// <param name="ok">Ok action.</param>
        void MessageBox(Sprite icon, string header, string text, DialogAction<DialogCancelArgs> ok = null);

        /// <summary>
        /// Creates confirmation dialog.
        /// </summary>
        /// <param name="header">Header text.</param>
        /// <param name="text">Message text.</param>
        /// <param name="ok">Ok action.</param>
        /// <param name="cancel">Cancel action.</param>
        /// <param name="okText">Ok button text.</param>
        /// <param name="cancelText">Cancel button text.</param>
        void Confirmation(string header, string text, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel");

        /// <summary>
        /// Creates confirmation dialog.
        /// </summary>
        /// <param name="icon">Header icon.</param>
        /// <param name="header">Header text.</param>
        /// <param name="text">Mesasge text.</param>
        /// <param name="ok">Ok action.</param>
        /// <param name="cancel">Cancel action.</param>
        /// <param name="okText">Ok button text.</param>
        /// <param name="cancelText">Cancel button text.</param>
        void Confirmation(Sprite icon, string header, string text, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel");

        /// <summary>
        /// Creates 3 button confirmation dialog.
        /// </summary>
        /// <param name="header">Header text.</param>
        /// <param name="text">Message text.</param>
        /// <param name="ok">Ok action.</param>
        /// <param name="cancel">Cancel action.</param>
        /// <param name="alt">Alternative action.</param>
        /// <param name="okText">Ok button text.</param>
        /// <param name="cancelText">Cancel button text.</param>
        /// <param name="altText">Alternative button text.</param>
        void Confirmation(string header, string text, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, DialogAction<DialogCancelArgs> alt, string okText = "OK", string cancelText = "Cancel", string altText = "Close");

        /// <summary>
        /// Creates 3 button confirmation dialog.
        /// </summary>
        /// <param name="icon">Header icon.</param>
        /// <param name="header">Header text.</param>
        /// <param name="text">Message text.</param>
        /// <param name="ok">Ok action.</param>
        /// <param name="cancel">Cancel action.</param>
        /// <param name="alt">Alternative action.</param>
        /// <param name="okText">Ok button text.</param>
        /// <param name="cancelText">Cancel button text.</param>
        /// <param name="altText">Alternative button text.</param>
        void Confirmation(Sprite icon, string header, string text, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, DialogAction<DialogCancelArgs> alt, string okText = "OK", string cancelText = "Cancel", string altText = "Close");

        /// <summary>
        /// Creates prompt dialog.
        /// </summary>
        /// <param name="header">Header text.</param>
        /// <param name="text">Input field text.</param>
        /// <param name="ok">Ok action.</param>
        /// <param name="cancel">Cancel action.</param>
        /// <param name="okText">Ok button text.</param>
        /// <param name="cancelText">Cancel button text.</param>
        void Prompt(string header, string text, DialogAction<PromptDialogArgs> ok, DialogAction<PromptDialogArgs> cancel = null, string okText = "OK", string cancelText = "Cancel");

        /// <summary>
        /// Creates prompt dialog.
        /// </summary>
        /// <param name="icon">Header icon.</param>
        /// <param name="header">Header text.</param>
        /// <param name="text">Input field text.</param>
        /// <param name="ok">Ok action.</param>
        /// <param name="cancel">Cancel action.</param>
        /// <param name="okText">Ok button text.</param>
        /// <param name="cancelText">Cancel button text.</param>
        void Prompt(Sprite icon, string header, string text, DialogAction<PromptDialogArgs> ok, DialogAction<PromptDialogArgs> cancel = null, string okText = "OK", string cancelText = "Cancel");

        /// <summary>
        /// Creates dialog.
        /// </summary>
        /// <param name="icon">Header icon.</param>
        /// <param name="header">Header text.</param>
        /// <param name="content">Dialog content transform.</param>
        /// <param name="ok">Ok Action.</param>
        /// <param name="okText">Ok button text.</param>
        /// <param name="cancel">Cancel action.</param>
        /// <param name="cancelText">Cancel button text.</param>
        /// <param name="canResize">True to be able to resize the dialog.</param>
        /// <param name="minWidth">Minimum width of the dialog.</param>
        /// <param name="minHeight">Minimum height of the dialog.</param>
        void Dialog(Sprite icon, string header, Transform content, DialogAction<DialogCancelArgs> ok, string okText = "OK", DialogAction<DialogCancelArgs> cancel = null, string cancelText = "Cancel",
             bool canResize = false,
             float minWidth = 150,
             float minHeight = 100);

        /// <summary>
        /// Creates dialog.
        /// </summary>
        /// <param name="header">Header text.</param>
        /// <param name="content">Dialog content transform.</param>
        /// <param name="ok">Ok Action.</param>
        /// <param name="okText">Ok button text.</param>
        /// <param name="cancel">Cancel action.</param>
        /// <param name="cancelText">Cancel button text.</param>
        /// <param name="canResize">True to be able to resize the dialog.</param>
        /// <param name="minWidth">Minimum width of the dialog.</param>
        /// <param name="minHeight">Minimum height of the dialog.</param>
        void Dialog(string header, Transform content, DialogAction<DialogCancelArgs> ok, string okText = "OK", DialogAction<DialogCancelArgs> cancel = null, string cancelText = "Cancel",
            bool canResize = false,
            float minWidth = 150,
            float minHeight = 100);

        /// <summary>
        /// Creates dialog.
        /// </summary>
        void Dialog(string header, Transform content, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel",
             float minWidth = 150,
             float minHeight = 150,
             float preferredWidth = 700,
             float preferredHeight = 400,
             bool canResize = true);

        /// <summary>
        /// Creates dialog.
        /// </summary>
        void Dialog(Sprite icon, string header, Transform content, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel",
             float minWidth = 150,
             float minHeight = 150,
             float preferredWidth = 700,
             float preferredHeight = 400,
             bool canResize = true);

        /// <summary>
        /// Copies source window transform to target window transform.
        /// </summary>
        /// <param name="targetConent">Target window transform.</param>
        /// <param name="sourceContent">Source window transform.</param>
        void CopyTransform(Transform targetConent, Transform sourceContent);

        /// <summary>
        /// Sets anchoredPosition, anchorMin, anchorMax and size delta.
        /// </summary>
        /// <param name="content">Target window transform.</param>
        /// <param name="anchoredPosition">Anchored position.</param>
        /// <param name="anchorMin">Anchor min.</param>
        /// <param name="anchorMax">Anchor max.</param>
        /// <param name="sizeDelta">Size delta.</param>
        void SetTransform(Transform content, Vector2 anchoredPosition, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta);

        /// <summary>
        /// Gets header text of the window.
        /// </summary>
        /// <param name="content">Window transform.</param>
        /// <returns>Header text.</returns>
        string GetHeaderText(Transform content);

        /// <summary>
        /// Sets header text of the window.
        /// </summary>
        /// <param name="content">Window transform.</param>
        /// <param name="headerText">Header text.</param>
        void SetHeaderText(Transform content, string headerText);
        
        /// <summary>
        /// Gets header icon.
        /// </summary>
        /// <param name="content">Window transform.</param>
        /// <returns>Header icon.</returns>
        Sprite GetHeaderIcon(Transform content);

        /// <summary>
        /// Sets header icon.
        /// </summary>
        /// <param name="content">Window transform.</param>
        /// <param name="icon">Header icon.</param>
        void SetHeaderIcon(Transform content, Sprite icon);

        void ForceLayoutUpdate();

        [Obsolete("Use ILayoutStorageModel.DefaultLayoutName")]
        string DefaultPersistentLayoutName
        {
            get;
        }

        [Obsolete("Use ILayoutStorageModel.LayoutExists")]
        bool LayoutExist(string name);

        [Obsolete("Use ILayoutStorageModel.SaveLayout")]
        void SaveLayout(string name);

        [Obsolete("Use ILayoutStorageModel.GetLayout")]
        LayoutInfo GetLayout(string name, GameObject tabPrefab = null);

        [Obsolete("Use SetLayout in combination with ILayoutStorageModel.GetLayout")]
        void LoadLayout(string name, GameObject tabPrefab = null);

        [Obsolete("Use ILayoutStorageModel.DeleteLayout")]
        void DeleteLayout(string name);
    }

    public static class IWindowManagerExt
    {
        public static LayoutInfo GetBuiltInDefaultLayout(this IWindowManager wm)
        {
            WindowDescriptor sceneWd;
            GameObject sceneContent;
            bool isDialog;
            wm.CreateWindow(RuntimeWindowType.Scene.ToString(), out sceneWd, out sceneContent, out isDialog);

            WindowDescriptor gameWd;
            GameObject gameContent;
            wm.CreateWindow(RuntimeWindowType.Game.ToString(), out gameWd, out gameContent, out isDialog);

            WindowDescriptor inspectorWd;
            GameObject inspectorContent;
            wm.CreateWindow(RuntimeWindowType.Inspector.ToString(), out inspectorWd, out inspectorContent, out isDialog);

            WindowDescriptor consoleWd;
            GameObject consoleContent;
            wm.CreateWindow(RuntimeWindowType.Console.ToString(), out consoleWd, out consoleContent, out isDialog);

            WindowDescriptor hierarchyWd;
            GameObject hierarchyContent;
            wm.CreateWindow(RuntimeWindowType.Hierarchy.ToString(), out hierarchyWd, out hierarchyContent, out isDialog);

            WindowDescriptor projectWd;
            GameObject projectContent;
            wm.CreateWindow(RuntimeWindowType.Project.ToString(), out projectWd, out projectContent, out isDialog);

            WindowDescriptor animationWd;
            GameObject animationContent;
            wm.CreateWindow(RuntimeWindowType.Animation.ToString(), out animationWd, out animationContent, out isDialog);

            LayoutInfo scene = wm.CreateLayoutInfo(sceneContent.transform, sceneWd);
            scene.IsHeaderVisible = RenderPipelineInfo.Type == RPType.Standard || RenderPipelineInfo.UseForegroundLayerForUI;

            LayoutInfo animation = (RenderPipelineInfo.Type == RPType.Standard || RenderPipelineInfo.UseForegroundLayerForUI) ?
                    new LayoutInfo(
                        wm.CreateLayoutInfo(gameContent.transform, gameWd),
                        wm.CreateLayoutInfo(animationContent.transform, animationWd)) :
                    wm.CreateLayoutInfo(animationContent.transform, animationWd);

            LayoutInfo layout = new LayoutInfo(false,
                new LayoutInfo(false,
                    new LayoutInfo(true,
                        wm.CreateLayoutInfo(inspectorContent.transform, inspectorWd),
                        wm.CreateLayoutInfo(consoleContent.transform, consoleWd),
                        0.5f),
                    new LayoutInfo(true,
                        scene,
                        animation,
                        0.75f),
                    0.25f),
                new LayoutInfo(true,
                    wm.CreateLayoutInfo(hierarchyContent.transform, hierarchyWd),
                    wm.CreateLayoutInfo(projectContent.transform, projectWd),
                    0.5f),
                0.75f);

            return layout;
        }

        public static void RegisterWindow(this IWindowManager wm, string typeName, string header, Sprite icon, GameObject prefab, bool isDialog, int maxWindows = 1)
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

        public static Transform CreateDropdown(this IWindowManager wm, string windowTypeName, Transform anchor, bool canResize = false, float minWidth = 50, float minHeight = 50)
        {
            return wm.CreateDropdown(windowTypeName, (RectTransform)anchor, canResize, minWidth, minHeight);
        }

    }

    [Serializable]
    public class WindowDescriptor
    {
        public Sprite Icon;
        public string Header;
        public GameObject TabPrefab;
        public GameObject ContentPrefab;

        public string Args;
        public int MaxWindows = 1;
        [ReadOnly]
        public int Created = 0;
    }

    [Serializable]
    public class CustomWindowDescriptor
    {
        public string TypeName;
        public bool IsDialog;
        public WindowDescriptor Descriptor;
    }

    [DefaultExecutionOrder(-89)]
    public class WindowManager : MonoBehaviour, IWindowManager
    {
        public event Action<IWindowManager> AfterLayout;
        public event Action<Transform> WindowCreated;
        public event Action<Transform> WindowDestroyed;

        [SerializeField]
        [FieldDisplayName("Use Legacy Built-In Windows")]
        private bool m_useLegacyBuiltInWindows = false;
        internal bool UseLegacyBuiltInWindows
        { 
            get { return m_useLegacyBuiltInWindows; }
        }

        #region Obsolete
        [SerializeField, Obsolete]
        private DialogManager m_dialogManager = null;

        [SerializeField]
        private WindowDescriptor m_sceneWindow = null;

        [SerializeField]
        private WindowDescriptor m_gameWindow = null;

        [SerializeField]
        private WindowDescriptor m_hierarchyWindow = null;

        [SerializeField]
        private WindowDescriptor m_inspectorWindow = null;

        [SerializeField]
        private WindowDescriptor m_projectWindow = null;

        [SerializeField]
        private WindowDescriptor m_consoleWindow = null;

        [SerializeField]
        private WindowDescriptor m_animationWindow = null;

        [SerializeField]
        private WindowDescriptor m_saveSceneDialog = null;

        [SerializeField]
        private WindowDescriptor m_saveAssetDialog = null;

        [SerializeField]
        private WindowDescriptor m_openProjectDialog = null;

        [SerializeField]
        private WindowDescriptor m_selectAssetLibraryDialog = null;

        [SerializeField]
        private WindowDescriptor m_toolsWindow = null;

        [SerializeField]
        private WindowDescriptor m_importAssetsDialog = null;

        [SerializeField]
        private WindowDescriptor m_aboutDialog = null;

        [SerializeField]
        private WindowDescriptor m_selectObjectDialog = null;

        [SerializeField]
        private WindowDescriptor m_selectColorDialog = null;

        [SerializeField]
        private WindowDescriptor m_selectAnimationPropertiesDialog = null;

        [SerializeField]
        private WindowDescriptor m_saveFileDialog = null;

        [SerializeField]
        private WindowDescriptor m_openFileDialog = null;

        [SerializeField]
        private WindowDescriptor m_emptyDialog = null;

        [SerializeField]
        private WindowDescriptor m_empty = null;
        
        [SerializeField]
        private CustomWindowDescriptor[] m_customWindows = null;
  
        [SerializeField, Obsolete, HideInInspector]
        private DockPanel m_dockPanels = null;

        [SerializeField, Obsolete]
        private Transform m_componentsRoot = null;

        [SerializeField, Obsolete]
        private RectTransform m_toolsRoot = null;

        [SerializeField, Obsolete]
        private RectTransform m_topBar = null;

        [SerializeField, Obsolete]
        private RectTransform m_bottomBar = null;

        [SerializeField, Obsolete]
        private RectTransform m_leftBar = null;

        [SerializeField, Obsolete]
        private RectTransform m_rightBar = null;

        [SerializeField, Obsolete]
        private RectTransform m_inputDialog = null;

        #endregion

        [SerializeField, HideInInspector]
        private Workspace m_activeWorkspace;

        public Workspace ActiveWorkspace
        {
            get  { return m_activeWorkspace;  }
            set
            {

                if(m_activeWorkspace != null)
                {
                    m_activeWorkspace.AfterLayout -= OnAfterLayout;
                    m_activeWorkspace.WindowCreated -= OnWindowCreated;
                    m_activeWorkspace.WindowDestroyed -= OnWindowDestroyed;
                    m_activeWorkspace.DeferUpdate -= OnDeferUpdate;
                }

                m_activeWorkspace = value;

                if (m_activeWorkspace != null)
                {
                    m_activeWorkspace.AfterLayout += OnAfterLayout;
                    m_activeWorkspace.WindowCreated += OnWindowCreated;
                    m_activeWorkspace.WindowDestroyed += OnWindowDestroyed;
                    m_activeWorkspace.DeferUpdate += OnDeferUpdate;
                }
            }
        }

        private IInput Input
        {
            get { return m_editor.Input; }
        }

        private RuntimeWindow ActiveWindow
        {
            get { return m_editor.ActiveWindow; }
        }

        private RuntimeWindow[] Windows
        {
            get { return m_editor.Windows; }
        }

        private IUIRaycaster Raycaster
        {
            get { return m_editor.Raycaster; }
        }

        private bool IsInputFieldFocused
        {
            get { return m_editor.IsInputFieldFocused; }
        }

        public bool IsDialogOpened
        {
            get { return ActiveWorkspace.DialogManager.IsDialogOpened; }
        }

        public Transform ComponentsRoot
        {
            get { return ActiveWorkspace.ComponentsRoot; }
        }

        public RectTransform PopupRoot
        {
            get { return (RectTransform)ActiveWorkspace.DockPanel.Free; }
        }

        private IRTE m_editor;
        private IRTEInputModule m_inputModule;
        private float m_zAxis;
        private bool m_skipUpdate;
        public readonly Dictionary<string, CustomWindowDescriptor> m_typeToCustomWindow = new Dictionary<string, CustomWindowDescriptor>();

        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();

            #pragma warning disable CS0612
            if (m_componentsRoot == null)
            {
                m_componentsRoot = transform;
            }

            if (m_activeWorkspace == null)
            {
                if (m_dockPanels != null)
                {
                    if (m_dialogManager == null)
                    {
                        m_dialogManager = FindObjectOfType<DialogManager>();
                    }
                    InitActiveWorkspace_Internal();
                }
                else
                {
                    Debug.LogError("m_activeWorkspace is null");
                    return;
                }
            }
        }

        internal void InitActiveWorkspace_Internal()
        {
            Workspace activeWorkspace = gameObject.AddComponent<Workspace>();
            activeWorkspace.ComponentsRoot = m_componentsRoot;
            activeWorkspace.ToolsRoot = m_toolsRoot;
            activeWorkspace.TopBar = m_topBar;
            activeWorkspace.BottomBar = m_bottomBar;
            activeWorkspace.LeftBar = m_leftBar;
            activeWorkspace.RightBar = m_rightBar;
            activeWorkspace.DockPanel = m_dockPanels;
            activeWorkspace.DialogManager = m_dialogManager;
            activeWorkspace.InputDialog = m_inputDialog;
            activeWorkspace.Init();
            ActiveWorkspace = activeWorkspace;
        }
        #pragma warning restore

        private void Start()
        {
            m_inputModule = IOC.Resolve<IRTEInputModule>();
            if (m_inputModule != null)
            {
                m_inputModule.Update += OnInputModuleUpdate;
            }

            for (int i = 0; i < m_customWindows.Length; ++i)
            {
                CustomWindowDescriptor customWindow = m_customWindows[i];
                if(customWindow.TypeName == null)
                {
                    continue;
                }
                if (customWindow != null && customWindow.Descriptor != null && !m_typeToCustomWindow.ContainsKey(customWindow.TypeName.ToLower()))
                {
                    m_typeToCustomWindow.Add(customWindow.TypeName.ToLower(), customWindow);
                }
            }

            m_sceneWindow.MaxWindows = m_editor.CameraLayerSettings.MaxGraphicsLayers;

            SetDefaultLayout();

            WindowDescriptor wd;
            GameObject content;
            bool isDialog;

            Transform tools = CreateWindow(RuntimeWindowType.ToolsPanel.ToString().ToLower(), out wd, out content, out isDialog);
            if (tools != null)
            {
                SetTools(tools);
            }
        }

        private void OnDestroy()
        {
            if (m_activeWorkspace != null)
            {
                m_activeWorkspace.AfterLayout -= OnAfterLayout;
                m_activeWorkspace.WindowCreated -= OnWindowCreated;
                m_activeWorkspace.WindowDestroyed -= OnWindowDestroyed;
                m_activeWorkspace.DeferUpdate -= OnDeferUpdate;
            }

            if (m_inputModule != null)
            {
                m_inputModule.Update -= OnInputModuleUpdate;
            }
        }

        private void OnInputModuleUpdate()
        {
            if (m_skipUpdate)
            {
                m_skipUpdate = false;
                return;
            }

            if (!m_editor.IsInputFieldActive)
            {
                if (IsDialogOpened)
                {
                    if (m_editor.Input.GetKeyDown(KeyCode.Escape))
                    {
                        ActiveWorkspace.DialogManager.CloseDialog();
                    }
                }
            }

            m_editor.UpdateCurrentInputField();
            EnableOrDisableRaycasts();

            bool mwheel = false;
            if (m_zAxis != Mathf.CeilToInt(Mathf.Abs(Input.GetAxis(InputAxis.Z))))
            {
                mwheel = m_zAxis == 0;
                m_zAxis = Mathf.CeilToInt(Mathf.Abs(Input.GetAxis(InputAxis.Z)));
            }

            bool pointerDownOrUp = Input.GetPointerDown(0) ||
                Input.GetPointerDown(1) ||
                Input.GetPointerDown(2) ||
                Input.GetPointerUp(0);

            bool canActivate = pointerDownOrUp ||
                mwheel ||
                Input.IsAnyKeyDown() && !IsInputFieldFocused;

            if (canActivate)
            {
                List<RaycastResult> results = new List<RaycastResult>();
                Raycaster.Raycast(results);

                RectTransform activeRectTransform = GetRegionTransform(ActiveWindow);
                bool activeWindowContainsScreenPoint = activeRectTransform != null && RectTransformUtility.RectangleContainsScreenPoint(activeRectTransform, Input.GetPointerXY(0), Raycaster.eventCamera);

                if (!results.Any(r => r.gameObject.GetComponent<Menu>() || r.gameObject.GetComponent<WindowOverlay>()))
                {
                    var regions = results.Select(r => r.gameObject.GetComponentInParent<Region>()).Where(r => r != null);

                    foreach (Region region in regions)
                    {
                        RuntimeWindow window = region.ActiveContent != null ? region.ActiveContent.GetComponentInChildren<RuntimeWindow>() : region.ContentPanel.GetComponentInChildren<RuntimeWindow>();
                        if (window != null && (!activeWindowContainsScreenPoint || window.Depth >= ActiveWindow.Depth))
                        {
                            if (m_editor.Contains(window))
                            {
                                if (pointerDownOrUp || window.ActivateOnAnyKey)
                                {
                                    if (window != null)
                                    {
                                        IEnumerable<Selectable> selectables = results.Select(r => r.gameObject.GetComponent<Selectable>()).Where(s => s != null);
                                        int count = selectables.Count();
                                        if (count >= 1)
                                        {
                                            RuntimeSelectionComponentUI selectionComponentUI = selectables.First() as RuntimeSelectionComponentUI;
                                            if (selectionComponentUI != null)
                                            {
                                                selectionComponentUI.Select();
                                            }
                                        }

                                        IEnumerable<Resizer> resizer = results.Select(r => r.gameObject.GetComponent<Resizer>()).Where(r => r != null);
                                        if (resizer.Any())
                                        {
                                            break;
                                        }
                                    }

                                    
                                    if (window != ActiveWindow)
                                    {
                                        if(ActiveWorkspace.ActivateWindow(region.ActiveContent))
                                        {
                                            if (window != ActiveWindow)
                                            {
                                                m_editor.ActivateWindow(window);
                                            }
                                            region.MoveRegionToForeground();
                                        }

                                        //26.07.2021 activate dialog window
                                        else if (region.GetComponentInChildren<Dialog>() != null) 
                                        {
                                            if (window != ActiveWindow)
                                            {
                                                m_editor.ActivateWindow(window);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void Update()
        {
            if (m_inputModule == null)
            {
                OnInputModuleUpdate();
            }
        }

        private void OnAfterLayout(Workspace obj)
        {
            if(AfterLayout != null)
            {
                AfterLayout(this);
            }
        }

        private void OnWindowCreated(Transform window)
        {
            if(WindowCreated != null)
            {
                WindowCreated(window);
            }
        }

        private void OnWindowDestroyed(Transform window)
        {
            if(WindowDestroyed != null)
            {
                WindowDestroyed(window);
            }
        }

        private void OnDeferUpdate()
        {
            m_skipUpdate = true;
        }

        private RectTransform GetRegionTransform(RuntimeWindow window)
        {
            if (window == null)
            {
                return null;
            }

            Region region = window.GetComponentInParent<Region>();
            if (region == null)
            {
                return null;
            }

            return region.GetDragRegion() as RectTransform;
        }

        private void EnableOrDisableRaycasts()
        {
            if (ActiveWindow != null)
            {
                if (ActiveWorkspace.IsPointerOver(ActiveWindow) && !IsOverlapped(ActiveWindow))
                {
                    if (!ActiveWorkspace.IsPointerOverActiveWindow)
                    {
                        ActiveWorkspace.IsPointerOverActiveWindow = true;
                        
                        RuntimeWindow[] windows = Windows;

                        for (int i = 0; i < windows.Length; ++i)
                        {
                            RuntimeWindow window = windows[i];
                            window.DisableRaycasts();
                        }
                    }
                }
                else
                {
                    if (ActiveWorkspace.IsPointerOverActiveWindow)
                    {
                        ActiveWorkspace.IsPointerOverActiveWindow = false;

                        RuntimeWindow[] windows = Windows;

                        for (int i = 0; i < windows.Length; ++i)
                        {
                            RuntimeWindow window = windows[i];
                            window.EnableRaycasts();
                        }
                    }
                }
            }
        }

        private bool IsOverlapped(RuntimeWindow testWindow, RuntimeWindow exceptWindow = null)
        {
            for (int i = 0; i < Windows.Length; ++i)
            {
                RuntimeWindow window = Windows[i];
                if (window == testWindow)
                {
                    continue;
                }

                if(window == null)
                {
                    continue;
                }

                if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)window.transform, Input.GetPointerXY(0), Raycaster.eventCamera))
                {
                    if (testWindow.Depth < window.Depth && exceptWindow != window)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public Transform FindPointerOverWindow(RuntimeWindow exceptWindow = null)
        {
            return ActiveWorkspace.FindPointerOverWindow(exceptWindow);
        }

        public bool IsWindowRegistered(string windowTypeName)
        {
            return GetWindowDescriptor(windowTypeName, out _) != null;
        }

        public bool RegisterWindow(CustomWindowDescriptor desc)
        {
            if (m_typeToCustomWindow.ContainsKey(desc.TypeName.ToLower()))
            {
                return false;
            }

            m_typeToCustomWindow.Add(desc.TypeName.ToLower(), desc);
            return true;
        }

        public LayoutInfo CreateLayoutInfo(string windowTypeName)
        {
            return CreateLayoutInfo(windowTypeName, null);
        }

        public LayoutInfo CreateLayoutInfo(string windowTypeName, string args)
        {
            if (!IsWindowRegistered(windowTypeName))
            {
                throw new ArgumentException($"{windowTypeName} is not registered", nameof(windowTypeName));
            }

            var window = CreateWindow(windowTypeName, out WindowDescriptor desc);
            if(args != null)
            {
                SetWindowArgs(window, args);
            }
            
            return CreateLayoutInfo(window, desc);
        }

        public LayoutInfo CreateLayoutInfo(Transform content, WindowDescriptor desc)
        {
            return ActiveWorkspace.CreateLayoutInfo(content, desc.TabPrefab, desc.Args, desc.Header, desc.Icon);
        }

        public LayoutInfo CreateLayoutInfo(Transform content, string header, Sprite icon)
        {
            return ActiveWorkspace.CreateLayoutInfo(content, header, icon);
        }

        public bool ValidateLayout(LayoutInfo layoutInfo)
        {
            return ActiveWorkspace.ValidateLayout(layoutInfo);
        }

        public void OverrideDefaultLayout(Func<IWindowManager, LayoutInfo> buildLayoutCallback, string activateWindowOfType = null)
        {
            if(ActiveWorkspace == null)
            {
                //QuickFix - will be removed 
                InitActiveWorkspace_Internal();
            }

            ActiveWorkspace.OverrideDefaultLayout(buildLayoutCallback, activateWindowOfType);
        }

        public void SetDefaultLayout()
        {
            ActiveWorkspace.SetDefaultLayout();
        }

        public void OverrideWindow(string windowTypeName, GameObject prefab)
        {
            OverrideWindow(windowTypeName, new WindowDescriptor { ContentPrefab = prefab });
        }

        public void OverrideWindow(string windowTypeName, WindowDescriptor descriptor)
        {
            windowTypeName = windowTypeName.ToLower();

            if (!m_useLegacyBuiltInWindows && m_typeToCustomWindow.TryGetValue(windowTypeName, out CustomWindowDescriptor cwd))
            {
                cwd.Descriptor = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.Scene.ToString().ToLower())
            {
                m_sceneWindow = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.Game.ToString().ToLower())
            {
                m_gameWindow = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.Hierarchy.ToString().ToLower())
            {
                m_hierarchyWindow = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.Inspector.ToString().ToLower())
            {
                m_inspectorWindow = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.Project.ToString().ToLower())
            {
                m_projectWindow = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.Console.ToString().ToLower())
            {
                m_consoleWindow = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.Animation.ToString().ToLower())
            {
                m_animationWindow = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.SaveScene.ToString().ToLower())
            {
                m_saveSceneDialog = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.SaveAsset.ToString().ToLower())
            {
                m_saveAssetDialog = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.OpenProject.ToString().ToLower())
            {
                m_openProjectDialog = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.ToolsPanel.ToString().ToLower())
            {
                m_toolsWindow = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.SelectAssetLibrary.ToString().ToLower())
            {
                m_selectAssetLibraryDialog = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.ImportAssets.ToString().ToLower())
            {
                m_importAssetsDialog = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.About.ToString().ToLower())
            {
                m_aboutDialog = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.SelectObject.ToString().ToLower())
            {
                m_selectObjectDialog = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.SelectColor.ToString().ToLower())
            {
                m_selectColorDialog = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.SelectAnimationProperties.ToString().ToLower())
            {
                m_selectAnimationPropertiesDialog = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.SaveFile.ToString().ToLower())
            {
                m_saveFileDialog = descriptor;
            }
            else if (windowTypeName == RuntimeWindowType.OpenFile.ToString().ToLower())
            {
                m_openFileDialog = descriptor;
            }
            else if(windowTypeName == RuntimeWindowType.Empty.ToString().ToLower())
            {
                m_empty = descriptor;
            }
            else if(windowTypeName == RuntimeWindowType.EmptyDialog.ToString().ToLower())
            {
                m_emptyDialog = descriptor;
            }
        }

        public void OverrideTools(Transform contentPrefab)
        {
            string windowTypeName = RuntimeWindowType.ToolsPanel.ToString().ToLower();

            if (!m_useLegacyBuiltInWindows && m_typeToCustomWindow.TryGetValue(windowTypeName, out CustomWindowDescriptor cwd) && cwd.Descriptor  != null)
            {
                cwd.Descriptor.ContentPrefab = contentPrefab != null ? contentPrefab.gameObject : null;
            }
            else
            {
                m_toolsWindow.ContentPrefab = contentPrefab != null ? contentPrefab.gameObject : null;
            }
        }

        public void SetTools(Transform tools)
        {
            ActiveWorkspace.SetTools(tools);
        }

        public void SetLeftBar(Transform tools)
        {
            ActiveWorkspace.SetLeftBar(tools);
        }

        public void SetRightBar(Transform tools)
        {
            ActiveWorkspace.SetRightBar(tools);
        }

        public void SetTopBar(Transform tools)
        {
            ActiveWorkspace.SetTopBar(tools);
        }

        public void SetBottomBar(Transform tools)
        {
            ActiveWorkspace.SetBottomBar(tools);
        }

        public void SetLayout(Func<LayoutInfo> buildLayoutCallback, string activateWindowOfType = null)
        {
            if (ActiveWorkspace == null)
            {
                //QuickFix - will be removed 
                InitActiveWorkspace_Internal();
            }
            ActiveWorkspace.SetLayout(wm => buildLayoutCallback(), activateWindowOfType);
        }

        public void SetLayout(Func<IWindowManager, LayoutInfo> buildLayoutCallback, string activateWindowOfType = null)
        {
            if(ActiveWorkspace == null)
            {
                //QuickFix - will be removed 
                InitActiveWorkspace_Internal();
            }
            ActiveWorkspace.SetLayout(buildLayoutCallback, activateWindowOfType);
        }

        public LayoutInfo GetLayout()
        {
            return ActiveWorkspace.GetLayout();
        }

        public bool Exists(string windowTypeName)
        {
            return GetWindow(windowTypeName) != null;
        }

        public string GetWindowTypeName(Transform content)
        {
            return ActiveWorkspace.GetWindowTypeName(content);
        }

        public Transform GetWindow(string windowTypeName)
        {
            return ActiveWorkspace.GetWindow(windowTypeName);
        }

        public Transform[] GetWindows()
        {
            return ActiveWorkspace.GetWindows();
        }

        public Transform[] GetWindows(string windowTypeName)
        {
            return ActiveWorkspace.GetWindows(windowTypeName);
        }

        public Transform[] GetComponents(Transform content)
        {
            return ActiveWorkspace.GetComponents(content);
        }

        public bool IsActive(string windowTypeName)
        {
            return ActiveWorkspace.IsActive(windowTypeName);
        }

        public bool IsActive(Transform content)
        {
            return ActiveWorkspace.IsActive(content);
        }

        public bool ActivateWindow(string windowTypeName)
        {
            return ActiveWorkspace.ActivateWindow(windowTypeName);
        }

        public bool ActivateWindow(Transform content)
        {
            return ActiveWorkspace.ActivateWindow(content);
        }

        public Transform CreateWindow(string windowTypeName, out WindowDescriptor wd)
        {
            return ActiveWorkspace.CreateWindow(windowTypeName, out wd, out _, out _);
        }
        public Transform CreateWindow(string windowTypeName, out WindowDescriptor wd, out bool isDialog)
        {
            return ActiveWorkspace.CreateWindow(windowTypeName, out wd, out _, out isDialog);
        }

        public Transform CreateWindow(string windowTypeName, out WindowDescriptor wd, out GameObject content, out bool isDialog)
        {
            return ActiveWorkspace.CreateWindow(windowTypeName, out wd, out content, out isDialog);
        }

        public Transform CreateWindow(string windowTypeName, bool isFree = true, RegionSplitType splitType = RegionSplitType.None, float flexibleSize = 0.3f, Transform parentWindow = null)
        {
            return ActiveWorkspace.CreateWindow(windowTypeName, isFree, splitType, flexibleSize, parentWindow);
        }

        public bool ScreenPointToLocalPointInRectangle(RectTransform rectTransform, Vector3 screenPoint, out Vector2 position)
        {
            return ActiveWorkspace.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, out position);
        }

        public Transform CreatePopup(string windowTypeName, Vector2 position, bool canResize, float minWidth, float minHeight)
        {
            return ActiveWorkspace.CreatePopup(windowTypeName, position, canResize, minWidth, minHeight);
        }

        public Transform CreatePopup(string windowTypeName, bool canResize, float minWidth, float minHeight)
        {
            return ActiveWorkspace.CreatePopup(windowTypeName, canResize, minWidth, minHeight);
        }
        public Transform CreateDropdown(string windowTypeName, RectTransform anchor, bool canResize, float minWidth, float minHeight)
        {
            return ActiveWorkspace.CreateDropdown(windowTypeName, anchor, canResize, minWidth, minHeight);
        }

        public void SetWindowArgs(Transform content, string args)
        {
            ActiveWorkspace.SetWindowArgs(content, args);
        }

        public void DestroyWindow(Transform content)
        {
            ActiveWorkspace.DestroyWindow(content);
        }

        public void DestroyWindowsOfType(string windowTypeName)
        {
            ActiveWorkspace.DestroyWindowsOfType(windowTypeName);
        }

        public Transform CreateDialogWindow(string windowTypeName, string header, DialogAction<DialogCancelArgs> okAction, DialogAction<DialogCancelArgs> cancelAction,
            bool canResize,
            float minWidth,
            float minHeight)
        {
            return ActiveWorkspace.CreateDialogWindow(windowTypeName, header, okAction, cancelAction, canResize, minWidth, minHeight);
        }

        public Transform CreateDialogWindow(string windowTypeName, string header, DialogAction<DialogCancelArgs> okAction, DialogAction<DialogCancelArgs> cancelAction,
             float minWidth,
             float minHeight,
             float preferredWidth,
             float preferredHeight,
             bool canResize = true)
        {
            return ActiveWorkspace.CreateDialogWindow(windowTypeName, header, okAction, cancelAction, minWidth, minHeight, preferredWidth, preferredHeight, canResize);
        }

        public void DestroyDialogWindow()
        {
            ActiveWorkspace.DestroyDialogWindow();
        }

        public WindowDescriptor GetWindowDescriptor(string windowTypeName, out bool isDialog)
        {
            WindowDescriptor wd = null;
            isDialog = false;
            if (windowTypeName == null)
            {
                return null;
            }

            windowTypeName = windowTypeName.ToLower();

            if (!m_useLegacyBuiltInWindows && m_typeToCustomWindow.TryGetValue(windowTypeName, out CustomWindowDescriptor cwd))
            {
                wd = cwd.Descriptor;
                isDialog = cwd.IsDialog;
            }
            else
            {
                if (windowTypeName == RuntimeWindowType.Scene.ToString().ToLower())
                {
                    wd = m_sceneWindow;
                }
                else if (windowTypeName == RuntimeWindowType.Game.ToString().ToLower())
                {
                    wd = m_gameWindow;
                }
                else if (windowTypeName == RuntimeWindowType.Hierarchy.ToString().ToLower())
                {
                    wd = m_hierarchyWindow;
                }
                else if (windowTypeName == RuntimeWindowType.Inspector.ToString().ToLower())
                {
                    wd = m_inspectorWindow;
                }
                else if (windowTypeName == RuntimeWindowType.Project.ToString().ToLower())
                {
                    wd = m_projectWindow;
                }
                else if (windowTypeName == RuntimeWindowType.Console.ToString().ToLower())
                {
                    wd = m_consoleWindow;
                }
                else if (windowTypeName == RuntimeWindowType.Animation.ToString().ToLower())
                {
                    wd = m_animationWindow;
                }
                else if (windowTypeName == RuntimeWindowType.ToolsPanel.ToString().ToLower())
                {
                    wd = m_toolsWindow;
                }
                else if (windowTypeName == RuntimeWindowType.SaveScene.ToString().ToLower())
                {
                    wd = m_saveSceneDialog;
                    isDialog = true;
                }
                else if (windowTypeName == RuntimeWindowType.SaveAsset.ToString().ToLower())
                {
                    wd = m_saveAssetDialog;
                    isDialog = true;
                }
                else if (windowTypeName == RuntimeWindowType.OpenProject.ToString().ToLower())
                {
                    wd = m_openProjectDialog;
                    isDialog = true;
                }
                else if (windowTypeName == RuntimeWindowType.SelectAssetLibrary.ToString().ToLower())
                {
                    wd = m_selectAssetLibraryDialog;
                    isDialog = true;
                }
                else if (windowTypeName == RuntimeWindowType.ImportAssets.ToString().ToLower())
                {
                    wd = m_importAssetsDialog;
                    isDialog = true;
                }
                else if (windowTypeName == RuntimeWindowType.About.ToString().ToLower())
                {
                    wd = m_aboutDialog;
                    isDialog = true;
                }
                else if (windowTypeName == RuntimeWindowType.SelectObject.ToString().ToLower())
                {
                    wd = m_selectObjectDialog;
                    isDialog = true;
                }
                else if (windowTypeName == RuntimeWindowType.SelectColor.ToString().ToLower())
                {
                    wd = m_selectColorDialog;
                    isDialog = true;
                }
                else if (windowTypeName == RuntimeWindowType.SelectAnimationProperties.ToString().ToLower())
                {
                    wd = m_selectAnimationPropertiesDialog;
                    isDialog = true;
                }
                else if (windowTypeName == RuntimeWindowType.SaveFile.ToString().ToLower())
                {
                    wd = m_saveFileDialog;
                    isDialog = true;
                }
                else if (windowTypeName == RuntimeWindowType.OpenFile.ToString().ToLower())
                {
                    wd = m_openFileDialog;
                    isDialog = true;
                }
                else if(windowTypeName == RuntimeWindowType.EmptyDialog.ToString().ToLower())
                {
                    wd = m_emptyDialog;
                    isDialog = true;
                }
                else if(windowTypeName == RuntimeWindowType.Empty.ToString().ToLower())
                {
                    wd = m_empty;
                }
                else
                {
                    if (m_typeToCustomWindow.TryGetValue(windowTypeName, out cwd))
                    {
                        wd = cwd.Descriptor;
                        isDialog = cwd.IsDialog;
                    }
                }
            }

            return wd;
        }

        public void MessageBox(string header, string text, DialogAction<DialogCancelArgs> ok = null)
        {
            ActiveWorkspace.MessageBox(header, text, ok);
        }

        public void MessageBox(Sprite icon, string header, string text, DialogAction<DialogCancelArgs> ok = null)
        {
            ActiveWorkspace.MessageBox(icon, header, text, ok);
        }

        public void Confirmation(string header, string text, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel")
        {
            ActiveWorkspace.Confirmation(header, text, ok, cancel, okText, cancelText);
        }

        public void Confirmation(Sprite icon, string header, string text, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel")
        {
            ActiveWorkspace.Confirmation(icon, header, text, ok, cancel, okText, cancelText);
        }

        public void Confirmation(string header, string text, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, DialogAction<DialogCancelArgs> alt, string okText = "OK", string cancelText = "Cancel", string altText = "Close")
        {
            ActiveWorkspace.Confirmation(header, text, ok, cancel, alt, okText, cancelText, altText);
        }

        public void Confirmation(Sprite icon, string header, string text, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, DialogAction<DialogCancelArgs> alt, string okText = "OK", string cancelText = "Cancel", string altText = "Close")
        {
            ActiveWorkspace.Confirmation(icon, header, text, ok, cancel, alt, okText, cancelText, altText);
        }

        public void Prompt(string header, string text, DialogAction<PromptDialogArgs> ok, DialogAction<PromptDialogArgs> cancel, string okText = "OK", string cancelText = "Cancel")
        {
            ActiveWorkspace.Prompt(header, text, ok, cancel, okText, cancelText);
        }
        
        public void Prompt(Sprite icon, string header, string text, DialogAction<PromptDialogArgs> ok, DialogAction<PromptDialogArgs> cancel, string okText = "OK", string cancelText = "Cancel")
        {
            ActiveWorkspace.Prompt(icon, header, text, ok, cancel, okText, cancelText);
        }

        public void Dialog(Sprite icon, string header, Transform content, DialogAction<DialogCancelArgs> ok, string okText = "OK", DialogAction<DialogCancelArgs> cancel = null,  string cancelText = "Cancel",
           bool canResize = false,
           float minWidth = 150,
           float minHeight = 100)
        {
            RectTransform rt = content as RectTransform;
            float preferredWidth = rt != null ? rt.rect.width : minWidth;
            float preferredHeight = rt != null ? rt.rect.height : minHeight;
            Dialog(icon, header, content, ok, cancel, okText, cancelText, minWidth, minHeight, preferredWidth, preferredHeight, canResize);
        }

        public void Dialog(string header, Transform content, DialogAction<DialogCancelArgs> ok, string okText = "OK", DialogAction<DialogCancelArgs> cancel = null,  string cancelText = "Cancel",
            bool canResize = false,
            float minWidth = 150,
            float minHeight = 100)
        {
            RectTransform rt = content as RectTransform;
            float preferredWidth = rt != null ? rt.rect.width : minWidth;
            float preferredHeight = rt != null ? rt.rect.height : minHeight;
            Dialog(header, content, ok, cancel, okText, cancelText, minWidth, minHeight, preferredWidth, preferredHeight, canResize);
        }

        public void Dialog(string header, Transform content, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel",
            float minWidth = 150,
            float minHeight = 150,
            float preferredWidth = 700,
            float preferredHeight = 400,
            bool canResize = true)
        {
            ActiveWorkspace.Dialog(header, content, ok, cancel, okText, cancelText, minWidth, minHeight, preferredWidth, preferredHeight, canResize);
        }

        public void Dialog(Sprite icon, string header, Transform content, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, string okText = "OK", string cancelText = "Cancel",
            float minWidth = 150,
            float minHeight = 150,
            float preferredWidth = 700,
            float preferredHeight = 400,
            bool canResize = true)
        {
            ActiveWorkspace.Dialog(icon, header, content, ok, cancel, okText, cancelText, minWidth, minHeight, preferredWidth, preferredHeight, canResize);
        }

        public void CopyTransform(Transform targetConent, Transform sourceContent)
        {
            ActiveWorkspace.CopyTransform(targetConent, sourceContent);
        }

        public void SetTransform(Transform content, Vector2 anchoredPosition, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
        {
            ActiveWorkspace.SetTransform(content, anchoredPosition, anchorMin, anchorMax, sizeDelta);
        }

        public string GetHeaderText(Transform content)
        {
            return ActiveWorkspace.GetHeaderText(content);
        }

        public void SetHeaderText(Transform content, string headerText)
        {
            ActiveWorkspace.SetHeaderText(content, headerText);
        }

        public Sprite GetHeaderIcon(Transform content)
        {
            return ActiveWorkspace.GetHeaderIcon(content);
        }

        public void SetHeaderIcon(Transform content, Sprite icon)
        {
            ActiveWorkspace.SetHeaderIcon(content, icon);
        }

        public void ForceLayoutUpdate()
        {
            ActiveWorkspace.ForceLayoutUpdate();
        }

        [Obsolete]
        public string DefaultPersistentLayoutName
        {
            get { return "Persistent_Layout"; }
        }

        [Obsolete]
        public bool LayoutExist(string name)
        {
            return ActiveWorkspace.LayoutExist(name);
        }

        [Obsolete]
        public void SaveLayout(string name)
        {
            ActiveWorkspace.SaveLayout(name);
        }

        [Obsolete]
        public LayoutInfo GetLayout(string name, GameObject tabPrefab = null)
        {
            return ActiveWorkspace.GetLayout(name, tabPrefab);
        }

        [Obsolete]
        public void LoadLayout(string name, GameObject tabPrefab = null)
        {
            ActiveWorkspace.LoadLayout(name, tabPrefab);
        }

        [Obsolete]
        public void DeleteLayout(string name)
        {
            ActiveWorkspace.DeleteLayout(name);
        }
    }
}

