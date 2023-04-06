##[RTEditor] - Today


##[RTEditor] - August 11, 2021
RTE: SceneViewImpl, SceneViewModel ExposePrefabInstance modified to not expose full hierarchy.

##[RTEditor] - July 28, 2021
UI: Set VirtualizingListBoxItem.m_toggle.interactable to true by default
UI: Added VirtualizingScrollRect.ItemsChanged event
UI: Added VirtualizingListBoxBinding.CanRemove property 
UI: Added VirtualizingListBoxBinding.CanReorder property 
UI: Added VirtualizingListBoxBinding.CanSelect property 
UI: Added VirtualizingListBoxBinding.Target property 
UI: Changed VirtualizingListBoxBinding base class from AbstractMemberBinding to ControlBinding


##[RTEditor] - July 26, 2021
RTE: Added code to activate Dialog windows WindowManager.OnInputModuleUpdate()
RTE: Added WindowOverlay to RuntimeEditor.prefab \UI\LayoutRoot\Middle\DockPanels\Mask\Modal

##[RTEditor] - July 23, 2021
RTE PeekMode: added Commands Panel
RTE PeekMode: added Group, Ungroup, CreatePrefab buttons to GameObjectView
RTE PeekMode: added "Begin Create" button

##[RTEditor] - July 21, 2021
RTE: Added Tools/Runtime Editor/Create Custom Window menu command

##[RTEditor] - July 20, 2021
RTE: Added ability to Create CustomView and CustomViewModel from template
RTE: UnityWeld.dll Auto Reference flag set to True

##[RTEditor] - July 18, 2021
RTH: Added code to update ExposeToEditor.LocalEulerAngles from RotationHandle 
RTE: Added support for Rotations > 360 degrees in Animation Editor

##[RTEditor] - July 16, 2021
RTE: Fixed AnimationViewModel undo & redo

##[RTEditor] - July 15, 2021
RTE: Fixed Legacy AnimationView undo & redo
RTE: Added InspectorModel
RTC: Deprecated RuntimeTools.ShowGizmos, ShowGizmosChanged, ShowSelectionGizmos, ShowSelectionGizmosChanged

##[RTEditor] - July 14, 2021
RTC: IOC.IsRegistered and IOC.IsFallbackRegistered methods added
RTE: Models\Placement\PlacementModel added to RuntimeEditor.prefab
RTE: Models\Grouping\GroupingModel added to RuntimeEditor.prefab
RTE: Deprecated IRuntimEditorExt 
RTE: Added IPlacementModel, PlacementModel
RTE: Added IGroupingModel, GroupingModel

##[RTEditor] - July 7, 2021
RTE PeekMode: Added PeekModeCommandsHandler
RTE PeekMode: Added CreatorWindow

##[RTEditor] - June 30, 2021
RTE:  Added Transform IWindowManager.Prompt(string header, string text, DialogAction<PromptDialogArgs> ok, DialogAction<PromptDialogArgs> cancel, string okText = "OK", string cancelText = "Cancel")
RTE:  Added Transform IWindowManager.Prompt(Sprite icon, string header, string text, DialogAction<PromptDialogArgs> ok, DialogAction<PromptDialogArgs> cancel, string okText = "OK", string cancelText = "Cancel")

##[RTEditor] - April 26, 2021
RTE: Added EmptyView and EmptyDialog
RTE: Added AutoUI

##[RTEditor] - April 26, 2021
RTE: added IProjectTreeViewModel, IProjectFolderViewModel as replacement for IProjectTree and IProjectFolder

##[RTEditor] - April 23, 2021
RTC: Added IRuntimeObjects.ComponentDestroyed event 
RTC: Added ExposeToEditor._ComponentDestroyed event 

##[RTEditor] - April 16, 2021
RTE: IWindowManager.CreatePopup, .CreateDropdown methods added
        
##[RTEditor] - April 15, 2021
UI: Added RaiseTransformChanged() call to Start method of RectTransformChangeListener (in order to correctly initialize VirtualizingScrollRect)
UI: Added Region.Destroy method
UI: Added DockPanel.AddDropdownRegion and .AddPopupRegion methods with various overloads.

##[RTEditor] - April 14, 2021
RTE: Added IRuntimeEditor.StopCoroutine
RTE: Added IWindowManager Confirmation overloads
	 void Confirmation(string header, string text, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, DialogAction<DialogCancelArgs> alt, string okText = "OK", string cancelText = "Cancel", string altText = "Close");
     void Confirmation(Sprite icon, string header, string text, DialogAction<DialogCancelArgs> ok, DialogAction<DialogCancelArgs> cancel, DialogAction<DialogCancelArgs> alt, string okText = "OK", string cancelText = "Cancel", string altText = "Close");
UI: Added DialogManager .ShowComplexDialog
UI: Added Dialog Alt button, Alt event and Alt action 

##[RTEditor] - April 12, 2021
RTH: Changed RuntimeHitTester DefaultExecutionOrder to -89

##[RTEditor] - April 10, 2021
RTB: Added support for Probuilder 4.4.0, 5.0.3

##[RTEditor] - April 9, 2021
RTC: Added IRuntimeUndo.AddComponentWithRequirements 

##[RTEditor] - April 8, 2021
RTH: Added MinOrthoSize, MaxOrthoSize properties to RuntimeSceneComponent
RTE PeekMode: Dropdowns, add component panel, add component viewmodel

##[RTEditor] - April 2, 2021
RTE PeekMode: GameObjectName toggle + styling
RTE PeekMode: ContextPanel using minimum distance = 50 when Empty Game Object selected
UI: Fixed NullReferenceException when opening a menu control with more than 2 levels of depth

##[RTEditor] - April 1, 2021
RTE PeekMode: Added ComponentViewModel
RTE PeekMode: Added ContextPanelViewModel

##[RTEditor] - March 31, 2021
RTE: Component Editor Icons added

##[RTEditor] - March 26, 2021
RTE: Added Default Component Editor Icon
RTE: Added ComponentEditorSettings.ShowIcon and ComponentEditorSettings.Icon fields
RTE: Added HeaderDescriptor.ShowIcon and HeaderDescriptor.Icon fields
RTE: Added Component Editor Icon 
RTE: Implemented PeekMode context panel visibility and positioning

##[RTEditor] - March 19, 2021
RTB: Highlight AutoUV with different color in Manual UV Editor window
RTB: Hidden position/rotation/scale handle axes if no faces selected in uv editing mode
RTB: Fixed bug where ManualUVRenderer was not refreshed correctly on mesh editing to uv editing mode change
RTB: Fixed bug where ManualUVRenderer was not refreshed correctly on vertex/edge/face mode change

##[RTEditor] - March 11, 2021
UI: Added SceneName parameter to  MenuDefinitionAttribute (defines scene which menu applies to)
RTE: Added PeekModeScene and PeekModeLayout
RTE: Added BuiltInWindowNames
RTE: Added LayoutExtension
UI:  Added LayoutInfo  LayoutInfo.Horizontal(LayoutInfo top, LayoutInfo bottom, float ratio = 0.5f);
UI:  Added LayoutInfo  LayoutInfo.Vertical(LayoutInfo left, LayoutInfo right, float ratio = 0.5f);
UI:  Added LayoutInfo  LayoutInfo.Group(params LayoutInfo[] tabGroup);
RTE: Added Transform IWindowManager.CreateWindow(string windowTypeName, out WindowDescriptor wd);
RTE: Added Transform IWindowManager.CreateWindow(string windowTypeName, out WindowDescriptor wd, out bool isDialog);
RTE: Added LayoutInfo IWindowManager.CreateLayoutInfo(Transform content, WindowDescriptor desc);
RTH: Added SelectionPicker (corresponding code removed from BoxSelection and BoxSelectionRenderer)

##[RTEditor] - March 10, 2021
RTE: Replaced UnityEvent HideInInspector with NonSerialized attribute 
RTE: Added ReplaceSceneViewAndViewModelExample
RTE: Added ability to overrided views

##[RTEditor] - March 9, 2021
RTE:  Added SceneViewModelOverride example
RTE:  Added ability to override view models
RTSL: Added support for objects with subassets imported from asset bundles.

##[RTEditor] - March 8, 2021
RTB: Material palette view tooltips added
RTE: Added LocalizedTooltip
RTC: SetCursor if m_currentCursorType != cursorType. CursorHelper.SetCursor return value changed from void to bool.
RTE: ImportFileDialog replaced
RTE: Fixed TimelineControl key already exists exceptions when adding row with ColumnCount = 1
RTE: AnimationSelectPropertiesDialog replaced
RTH: Fixed SceneGizmo.DoSceneGizmo
UI:  DockPanel.RootRegion.FrameImage.enabled = false
RTE: AnimationView replaced

##[RTEditor] - March 7, 2021
RTE: add BubbleUpPointerExit only if IsTouchSupported == true
RTT: TerrainEditor instantiated by TerrainView OnEditorSelectionChanged

RTT: store selected layer index. 
##[RTEditor] - March 4, 2021
RTSL: Fixed compatibility with 2.11 - 2.26 files
RTB:  ManualUVEditorView, ManualUVSceneComponent, ManualUVRenderer fixed to work with Universal and HD RP
RTSL: Updated PersistentSprite class to save textures which are not mapped.

##[RTEditor] - March 3, 2021
RTE: Replaced SaveAssetsDialog

##[RTEditor] - March 1, 2021
RTE: Replaced SaveFileDialog
RTE: Replaced OpenFileDialog

##[RTEditor] - March 1, 2021
RTE: ProjectTreeViewModel select assets folder when selected folder removed
RTE: ProjectTreeViewModel.SelectedItem accepts items for which CanDisplay returns true
RTH: Added SelectionMode { UseColliders - default, UseRenderers, UseRenderersBeforeColliders }. 
RTT: Removed ProjectItemView from LayerListBoxItem.prefab

##[RTEditor] - February 28, 2021
UI: VirtualizingTreeViewItem.ItemPresenter.Label Overflow set to Ellipsis
RTE: Added bool ShowLayers = true 
RTE: Added GameObjectEditorSettings 

##[RTEditor] - February 25, 2021
RTE: ZipConstants.DefaultCodePage = System.Text.Encoding.UTF8.CodePage;
RTE: Added 'select' parameter to public static void AddGameObjectToScene(this IRTE editor, GameObject go, bool select = true) method
RTC: Added ImputLowForRDP.cs
RTH: #pragma target 3.5 removed from OutlineBlur, OutlinePrepass, OutlineCompomosite

##[RTEditor] - February 24, 2021
RTE: private string[] m_hiddenValues added to EnumEditor
RTE: virtual modifer added to ComponentEditor.BuildEditor and .OnExpanded methods 
RTE: bool? IsExpandedByDefault added to ComponentEditorSettings (used to calculate defaultValue in IsComponentExpanded getter)

##[RTEditor] - February 23, 2021
RTE: Replaced ImportAssetsDialog
RTE: Replaced SelectAssetsDialog
RTE: Renderers with isPartOfStaticBatch == true excluded from BoxSelection
RTE: Using Screen.currentResolution.width instead of Display.main.systemWidth
RTE: BaseGizmoInput DefaultExecution order changed to -59
RTE: BaseGizmo public virtual Reset method

##[RTEditor] - February 17, 2021
RTE: Replaced ToolsPanel 
RTE: Replaced SettingsDialog
RTE: Fixed Default Layout procedure 

##[RTEditor] - February 15, 2021
RTE: Added RectTool text outline
RTE: Added HierarchicalDataView
RTE: Implemented HierarchicalDataViewModel common drag & drop handling

##[RTEditor] - February 12, 2021
RTE: Replaced ProjectView 
RTE: Fixed VirtualizingTreeView CanBeParent, CanReparent flags behaviour 

##[RTEditor] - February 10, 2021
RTE:  Added ability to disable foreground layer in Universal RP (If disabled, then the windows with the camera should not go to the floating mode).
RTE:  Added m_depthMaskOffset field to DepthMaskingBehaviour (presumably fixes window depth masking bug on MacOS?)
RTE:  Using the default UI shader from Unity 2020 #if UNITY_2019 (fixes incorrect transparency of the ui in windows placed above the scene)
RTSL: GetUOAssembliesAndTypes moved to ReflectionUtils.cs
RTSL: UnityWeld excluded from assemblies array in CodeGen.GetUOAssembliesAndTypes and EditorsMapWindow.GetUOAssembliesAndTypes

##[RTEditor] - February 09, 2021
RTE: Replaced SelectObjectDialog
RTE: Replaced SelectColorDialog
RTE: Added IWindowManager.CreateDialogWindow overload without preferredWidth, preferredHeight parameters
RTE: Replaced ManageProjectsDialog
RTSL: ProjectInfo .Name, .LastWriteTime fields replaced with properties
RTE: canResize = true param added to IWindowManager.Dialog methods
RTE: canResize = true param added to Workspace.Dialog methods

##[RTEditor] - February 08, 2021
RTE: Replaced SaveSceneDialog
RTE: Added DialogBinding
RTE: OneWayPropertyBindingSlim and TwoWayPropertyBindingSlim added to ControlBinding
RTSL: ProjectItem .Name, .Ext, .Children, .Parent fields replaced with properties

##[RTEditor] - February 03, 2021
RTE: Added missing functionality to write gameobject name changes to undo stack.
RTE: Activated, Deactivated events added to RuntimeWindow
RTE: AddComponentControl moved from InspectorView to GameObjectEditor
RTE: Replaced InspectorView

##[RTEditor] - February 03, 2021
RTE: Ability to override Inspector selection.
RTH: Added BoundingSphereRadius property to RuntimeSceneComponent 

##[RTEditor] - January 31, 2021
RTE: Added RuntimeGameWindow 
RTE: Replaced SceneView
RTE: Replaced ConsoleView
RTE: Replaced HierarchyView

##[RTEditor] - January 28, 2021
RTSL: Added .Load_Async<T> method with correct Task<T> return type.
RTSL: IProjectAsync .DeleteFolderAsync, .DeleteAsync don't throw an exception if ProjectItem doesn't exist

##[RTEditor] - January 27, 2021
RTE: View, ViewBinding added

##[RTEditor] - January 26, 2021
UI:  Added PrefabIndex to MenuItemInfo
UI:  Replaced Menu m_menuItemPrefab with m_menuItemPrefabs
RTE: Fixed Strong and EditorsMap classes (Universal Windows Platform)

##[RTEditor] - January 25, 2021
UI: Added VirtualizingTreeViewBinding .ItemClick, .ItemHold, .ItemDoubleClick, .ItemExpanded, .ItemCollapsed, .ItemDragEnter, .ItemDragExit events
UI: Added VirtualizingTreeViewBinding .Target property 
UI: VirtualizingItemsControl .ItemClick event no longer occurs after ItemDoubleClick operation.
UI: VirtualizingItemsControl .ItemClick event no longer occurs during a drag-and-drop operation.
   
##[RTEditor] - January 22, 2021
UI: Added VirtualizingTreeViewBinding DragItems, DropTargets
UI: Added VirtualizingTreeViewBinding Drag&Drop Unity Events
UI: Added ControlBinding, EventBindingSlim
RTE: Added Project LockAsync WebGL implementation

##[RTEditor] - January 18, 2021
UI: VirtualizingListBox binding added
UI: VirtualizingListBox added
UI: VirtualizingScroller continuous mode added

##[RTEditor] - January 15, 2021
RTC: Added .Name property to ExposeToEditor.

##[RTEditor] - January 14, 2021
RTE: Added BuiltInWindows component added to Runtime Editor prefab. 
RTE: Added WindowManager.m_useLegacyBuiltInWindows = false.
RTE: Added View prefabs, ViewModels and ViewModelTests.
RTE: Existing view and dialog prefabs moved to Battlehub\RTEditor\Content\Runtime\RTEditor\Prefabs\Legacy.
RTE: Existing views and dialogs moved to Battlehub\RTEditor\Runtime\RTEditor\Legacy.

##[RTEditor] - January 13, 2021
UI:  MenuItem quick fix for InputSystem and mouse support
RTE: InputSystem 1.0.1 package added
RTE: WindowManager will call Editor.ActivateWindow only if ActiveWorkspace.ActivateWindow returns true;
RTE: Workspace .ActivateWindow method fixed. Allow to activate window only in case if m_isTabDragInProgress == false;
UI: Region .Insert method fixed. Turn on the new tab *first* or else Unity's UI.Toggle.Set() won't let us turn off the tab that was previously active.
UI: Tab m_toggle.interactable = false; this will prevent the Toggle event from firing twice.

##[RTEditor] - January 12, 2021
RTE: Added HierarchyViewModel, HierarchyView prefab
RTE: Deprecated prefabs from RTEditor\Content\Runtime\Prefabs\View and Dialog folders
RTE: Deprecated AboutDialog, AssetLibraryImportDialog, AssetLibrarySelectDialog, ImportFileDialog, InputDialog, OpenFileDialog, ProjectsDialog, SaveAssetsDialog, SelectColorDialog, SelectObjectDialog, SettingsDialog
RTE: Deprecated ProjectFolderView, ProjectFolderViewImpl, ProjectItemView, ProjectTreeViewImpl, ProjectView, ProjectViewImpl
RTE: Deprecated SceneView, SceneViewImpl, GameView, HierarchyView, HierarchyViewImpl, InspectorView, ConsoleView, ToolsPanel
RTE: Scene cursor cannot be changed when popup is displayed.
RTE: Removed DragField from RangeEditor and RangeIntEditor prefabs.
RTE: URP, HDRP UIForeground transparency fix for unity 2019.

##[RTEditor] - January 11, 2021
UI: Added IHierarchicalData<T> interface.
UI: Added INotifyHierarchicalDataChanged interface.
UI: Added VirtualizingTreeViewBinding editor.

##[RTEditor] - January 10, 2021
UI: VirtualizingTreeViewBinding added.
RTE: Added UnityWeld.dll.

##[RTEditor] - January 4, 2021
RTE: Added protected virtual void Update() to InspectorView and AnimationView.

##[RTEditor] - December 21, 2020
RTSL: protected, internal, and private classes hidden from the Persistent Classes Window
RTSL: Persistent classes for Dropdown, ScrollRect, Scrollbar, Mask 

##[RTEditor] - December 17, 2020
RTE: Ability to disable rendering of UI to foreground layer.
RTE: Graphics Settings section does not appear in the SettingsDialog when using a user define RendererPipelineAsset.
RTE: .LoadBuiltInRendererPipelineAsset, .GetBuiltInRenderPipelneAssetName methods moved to RendererPipelineInfo.
RTE: SettingsComponent modified to prevent replacement of user defined RenderPipelineAsset.
RTE: Workspace will call ActivateWindow method only for most recent tab undocked by user (which is expected behavior).
UI:  Added DockPanel .ActivateBestTab method which activates most recent tab in the source region when selected tab is moved to another region.
UI:  Added Tab .m_actionCounter and .LastActionID property.

##[RTEditor] - December 16, 2020
RTE: ability to add touch support using main menu added.

##[RTEditor] - December 15, 2020
RTE: Ability to open context menu using tap & hold
UI: VirtualizingItemContainer .Hold event added
UI: VirtualizingItemsControl .Hold .ItemHold events added

##[RTEditor] - December 14, 2020
RTH: SceneGizmo and RectTool are no longer highlighted if TouchCount == 0
RTH: .BeginDrag .EndDrag method added to MobileSceneInput.
RTH: .UseMouse field added to RTEStandaloneInputModule.
UI:  Menu control touch support added.

##[RTEditor] - December 11, 2020
RTSL: Project deprecated and replaced with ProjectAsyncWrapper.

##[RTEditor] - December 9, 2020
RTSL: IProjectAsyncExtensions.TryGetValueAsync method added.
RTH: Assets\Battlehub\RTEditorDemo\Content\Runtime\RTHandles\RTHandles_Mobile scene added
RTH: Mobile input adjusted for orthographic camera

##[RTEditor] - December 8, 2020
RTH:  Ability to select objects using box selection in mobile scene added.
RTSL: RTSLSettings.SaveIncludedObjectsOnly field added.
RTSL: RTSLInclude monobehaviour added.

##[RTEditor] - December 7, 2020
RTSL: creating single RTSLTypeModel.dll for all build targets
RTE:  bubble up pointer exit event to WindowOverlay
RTSL: implementation of IProject using IProjectAsync added

##[RTEditor] - December 4, 2020
RTH: MobileSceneInput and MobileSceneControls added.
RTH: PositionHandle touch support added.
RTH: SceneGizmo touch support added.
RTH: RectTool touch support added.
RTE: WindowManager touch support added.
RTE: RTEStandaloneInputModule added.
RTE: MobileScene added.

##[RTEditor] - November 27, 2020
RTSL: CancellationToken parameter added to all async methods of IProjectAsync
RTSL: IIDGenerator<TID> CancellationToken parameter added to .GenerateAsync 

##[RTEditor] - November 25, 2020

RTSLVersion and package versions changed to 3.0.0 and will follow conventions described in following document https://docs.unity3d.com/Manual/upm-semver.html

##[RTEditor] - November 24, 2020
RTSL: IProjectAsync .LoadImportItemsAsync .UnloadImportItems .ImportAsync methods implemented
RTSL: IProjectAsync .ImportAsync argument type changed from ImportItem[] to ProjectItem[]

##[RTEditor] -  November 23, 2020
RTSL: IStorageAdapterAsync<TID> .GetValue & .GetValues methods fixed, SetGetValueUsageExample updated.
Extensions: Builder vertex\edge selection and polyshape fixed to work when geometry shaders are not supported.
RTH: RectTool using MeshTopology.Quads if SystemInfo.supportsGeometryShaders == false
RTH: Hidden/RTCommon/Point fallback added to PointBillboard shader
RTH: Hidden/RTCommon/UnitColor fallback added to LineBillboard shader 

##[RTEditor] -  November 18, 2020
RTE: IResourcePreviewUtility .PreviewWidth, .PreviewHeight properties added
RTSL: AssetLibraries with custom ids support added.
RTSL: ProjectItem .GetAssetLibraryIDs, ProjectItem .SetLibraryIDs methods added
RTSL: int AssetLibraryID field replaced with int[] AssetLibraryIDs
RTSL: IAssetDB<TID> .GetAssetLibraryID, .RegisterStaticResource, .UnregisterStaticResource, .UngregisterStaticResources method added
RTSL: IIDGenerator<TID>.IDGen property added to RTSLDeps
RTSL: ProjectAsyncImpl<TID>.GenerateIdentifiersAsync is not abstract anymore

##[RTEditor] -  November 17, 2020

RTE:  TryToAddColliders moved from ExposeToEditor.Awaked to ExposeToEditor.Started event handler.
RTE:  Layers editor fixed to display all layers except layers used by RuntimeEditor
RTSL: RTSLDepsGuids fixed.
RTSL: IProjectAsyncExtensions parity test added
RTSL: IProjectExtensions tests added
RTSL: ProjecItem  .Get, .GetOrCreate tests added
RTSL: IProjectExtensions fixed to use new versions of ProjectItem.Get, ProjectItem.GetOrCreateFolder methods
RTSL: ProjectItem .Get, .GetOrCreate folder methods fixed

##[RTEditor] -  November 16, 2020
RTE: RTEditorCustomID demo scene added to \Assets\Battlehub\RTEditorDemo\Content\Runtime\RTEditor
RTE: IRuntimeEditor.UpdatePreview method deprectated, use UpdatePreviewAsync instead
RTSL: ProjectItem .SetCustomDataOffset, .GetCustomDataOffset methods added
RTSL: FileSystemStorageAsync implementation
RTH: RuntimeSceneInputBase IsPointerOver usage removed for android and ios build target.

##[RTEditor] - November 13, 2020
RTSL: CodeGen updated to generate TypeModel for identifiers of custom type
RTSL: ProjectAsyncImpl .GenerateIdentifiers method renamed to GenerateIdentifiersAsync
RTSL: CustomID registration example added
RTSL: .IDTypes .RegisterID, .UnregisterID, .ClearIDs methods added to RTSLSettings


##[RTEditor] - November 12, 2020
RTSL: IProjectAsync divided into IProjectAsyncSafe, IProjectState and IProjectUtils.
RTSL: AssetItem type casts removed.
RTSL: IProject replaced with IProjectAsync.

##[RTEditor] - November 11, 2020

RTSL: LockProjectExample added.
RTSL: Lock, YieldLock - ability to orginize IProject and IProjectAsync method calls into atomic blocks.
RTSL: Forward IProject events through IProjectAsync.
RTSL: Project state shared between IProject and IProjectAsync implementations
RTSL: ProjectAsyncState added.

##[RTEditor] - November 10, 2020

RTSL: Compatibility tests updated
RTSL: AssetItem .ConvertToAssetItem, .ConvertToGenericAssetItem<TID> methods added
RTSL: ProjectItem is the base class for both AssetItem<T> and AssetItem 

##[RTEditor] - November 9, 2020

RTSL Tests: ProjectAsync tests added.
RTSL Tests: Compatibility tests added.
RTSL: Default TypeModel changed to be backward compatible with old save files and to support AssetItem<TID>
RTSL: ProjectAsync<long> is the base class for ProjectAsync (which temporarily contains import/load functionality for AssetBundles, AssetLibraries).
RTSL: AssetItem<long> is the base class for AssetItem.
RTSL: Preview is not a base class for Preview<long> anymore.
RTSL: ProjectItem .Get, .GetOrCreateFolder support for relative path added.
RTSL: ProjectItem .GetPreview, .SetPreview Preview replaced with byte[]
RTSL: IStorageAsync .GetPreviewsAsync, .GetPreviewsPerFolderAsync return value type changed to Task<Preview<TID>[]>
RTSL: IStorageAsync .GetID(Preview), .SetID(Preview) methods removed
RTSL: IStorageAsync .CreatePreview(ProjectItem, byte[]) method removed
RTSL: IProjectAsync .LoadImportItems, .Import, .GetAssetBundles, .GetStaticAssetLibraries renamed (Async prefix added)
RTSL: IProjectAsync .GetPreviewsAsync return value changed from Task<Preview[]> to Task<byte[][]>
RTSL: IProjectAsync .ToPersistentID(Preview), .SetPersistentID(Preview) methods removed
RTSL: callback parameters added to ProjectExtensions.Save, ProjectExtensions.Load methods

##[RTEditor] - November 6, 2020
RTSL: IProjectAsync interface, ProjectAsync added
RTSL: IStorageAsync<TID>, IStorageAdapter<TID> added
RTSL: Preview<TID>, AssetItem<TID> OnDeserilized methods added to copy data from deprecated fields if present.
RTSL: Preview<TID> with ID field added
RTSL: PrefabPart deprecated.
RTSL: ProjectItem .GetTypeGuid, .SetTypeGuid virtual methods added.
RTSL: ProjectItem .GetPreview, .SetPreview virtual methods added.
RTSL: ProjectItem .ItemID, .ItemGUID fields deprecated.
RTSL: ProjectItem ProjectItem(string name) constructor added.
RTSL: ProjectItem .GetOrCreateFolder(string).Get(string) methods added.
RTSL: ProjectItem .Get(string, bool) method deprecated.
RTSL: AssetItem .Preview property deprectated.
RTSL: AssetItem .Parts, .Dependencies, .DependenciesGuids fields are deprecated.
RTSL: AssetItem<TID> .ID, .DepenencyIDs, .EmbeddedIDs, .AssetLibraryID fields added.
RTSL: AssetItem<TID> added

##[RTEditor] - October 30, 2020
RuntimeUndo: restoring child RectTransforms after LayoutGroup add component undo.
RectTransformGizmo: refersh gizmo on LateUpdate, incorrect gizmo position fixed.
ComponentDescriptors: TextMeshProUGUI added.
RTSL: TMP_Text, TextMeshProUGUI persistent classes added.
PersistentClassesWindow: Fields and properties that are not UnityEngine.Object subclass and have no constructor without parameters was hidden.

##[RTEditor] - October 28, 2020
RuntimeUndo.AddComponent: check for DisallowMultipleComponents attribute.
RectTransform: reset button fixed.
RectTransform: locking properties driven by LayoutGroup
Transform Handles: XY2D, XZ2D, YZ2D and XYZ3D modes.
BaseHandle: SharedLockObject property added, LockObject property returns copy.

##[RTEditor] - October 27, 2020
PropertyEditors: BoolFloat editor added (used in LayoutEelement component editor)
PropertyDescriptor:  Range deprecated, PropertyMetadata added.
AddComponentControl: localization.
RectTransformEditor: disable properties editors for ScreenSpace canvas.
RectTransformEditor: disable anchor selector for root Canvas.
Vector2Editor: IsXInteractable, IsYInteractable properties added.
RTCommon/Billboard.shader: CONSTANT_SCALE shader feature added.

##[RTEditor] - October 26, 2020
ComponentDescriptors: UnityEngine.UI.LayoutElement component descriptor added.
ComponentDescriptors: UnityEngine.UI.HorizontalLayoutGroup component descriptor added.
ComponentDescriptors: UnityEngine.UI.VerticalLayoutGroup component descriptor added.
ComponentDescriptors: UnityEngine.UI.GridLayoutGroup component descriptor added.
DockPanel: Region resizer right and middle mouse button disabled.

##[RTEditor] - October 23, 2020

RTSL: PersistentClassMappingEditor, ability to reorder property mappings
RTSL: Toggle, ToggleGroup added to ClassMappingsTemplate
ComponentDescriptors: UnityEngine.UI.Image component descriptor added.
ComponentDescriptors: UnityEngine.UI.Canvas component descriptor added.
ComponentDescriptors: UnityEngine.UI.ToggleGroup component descriptor added.
ComponentDescriptors: UnityEngine.UI.Toggle component descriptor added.
ComponentDescriptors: UnityEngine.UI.Button component descriptor added.

##[RTEditor] - October 22, 2020

ComponentDescriptors: UnityEngine.UI.Text component descriptor added.
StringEditor: InputField RichText disabled.
RTSL: ExposeToEditor.Unselected property mapping disabled.
UnityEventBaseEditor: ability to select properties and methods with suitable parameters. (PersistentListenerMode.EventDefined)
ObjectEditor: ability to select component type using dropdown.
PropertyEditor: BeginEdit, EndEnd methods accessibility level changed from protected to public. BeginEdit method call removed from PropertyEditor.SetValue method.
Localization: LocaleChanged event added to ILocalization interface
RTEColors: DropdownColor.Pressed alpha changed to 255.

##[RTEditor] - October 20, 2020

Runtime Editor Configuration window: PersistentCall, UnityEventBase added to properties section.
IListEditor, CustomTypeEditor, BoundsEditor: using BeginRecord and EndRecord callbacks instead of OnValueChanging, OnValueChanged
PropertyEditor: m_currentValue field replaced with CurrentValue property.
EditorMap: mappings for UnityEventBase, PersistentCall and UnityEngine.UI.Button added.
ButtonComponentDescriptor added.
OptionsEditor: SetInputField and OnValueChanged methods moved to base class.  
Property Editors: UnityEventBaseEditor, PersistentCallEditor, OptionsEditorString classes and prefabs added.
BoxSelection: using RuntimeCameraWindow.ViewRoot RectTransform to find allowed screen area.
PersistentUnityEventBase updated to use UnityEventBaseExtensions.
Reflection: CleanAssemblyName method added.
UnityEventBaseExtensions, UnityEventBaseExtensionsTests, added.

##[RTEditor] - October 19, 2020

Battlehub/RTEditor/Content/Runtime/RTEditor/BuiltinMenu/GameObjects/UI/Canvas.prefab/GraphicRaycaster: Layers 16-24 excluded from m_blockingMask.
PersistentClassesWindow: Show all suitable clasess with System.SerializableAttribute.
IRuntimeEditor: ResetToDefaultLayout method added.
IRuntimeEditor:  CmdGameObjectValidate, CmdGameObject, CmdEditValidate, CmdEdit methods deprecated.
EditCmd, GameObjectCmd removed.
IEditCmd, IGameObjectCmd deprecated.
SetKnownGameObjectsExample added.
MenuGameObject: using GameObjectsAsset to populate MenuGameObject.
ISettingsComponent: KnownGameObjects property added.
GameObjectsAsset added

##[RTEditor] - October 18, 2020

Dedicated folder Content/Runtime/RTEditor/BuiltInMenu/GameObjects created for prefabs which can be created by Runtime Editor 
MenuFile, MenuEdit, MenuWindow, MenuHelp removed from RTEditor.prefab/UI/MenuPanel
BtnFile, BtnEdit, BtnGameObject, BtnWindow, BtnHelp removed from RuntimeEditor.prefab/UIBackground/Background/MainMenuBar/MenuPanel
MenuFile, MenuEdit, MenuGameObject, MenuWindow and MenuHelp classes added to Runtime/RTEditor/BuiltinMenu
MenuCommandAttribute.RequiresInstance and its constructors are deprecated and no longer needed or used.
Ability to add MenuCommand attribute to instance methods of class derived from MonoBehavior.
public static void RegisterAssembly() methods RuntimeInitializeLoadType.SubsystemRegistration parameter added to RuntimeInitializeOnLoadMethodAttribute

##[RTEditor] - October 16, 2020

PersistentRuntimePrefab: redundant code removed.
HierarchyViewImpl: redundant IRTE.Delete(gameObjects) call was removed from OnItemRemoving event handler.
GameObjectEditor: UpdatePreviews method fixed to update asset preview correctly.
RuntimeUndo: RecordValues and ApplyRecordedValue methods were removed.
ExposeToEditor: SetMarkAsDestroyed method changed to update hideFlags of children. It fixes exception occurred after exiting the play mode in unity editor.
	Steps to reproduce were following:
		- Add cube with child to Runtime Editor hierarchy.
		- Delete game object from Runtime Editor hierarchy.
		- Exit play mode.
		Expected:
		- no exceptions.
		Actual:
		- Unable to Destroy UnityEngine.Transform exception occured.


##[RTEditor] - October 15, 2020

KnownAssemblies static class added, readonly Assemblies array removed from BHROOT
PersistentRuntimeScene.WriteToImpl method changed: significant part of it's code was moved to PersistentRuntimePrefab.RestoreDataAndResolveDependencies
PersistentRuntimeScene.ReadFromImpl method removed: using base class implementation.
PersistentRuntimePrefab EmbeddedAssets and EmbeddedAssetIds fields added (replacement for Assets and AssetIds from PersistentRuntimeScene)
SetGetValueUsageExample updated.
IProject GetValue, GetValues, SetValue changed to accept any potentially serializable object.
Argument type of IStorage.GetValue, GetValues, SetValue methods changed from PersistentObject <TID> to System.Object.

##[RTEditor] - October 14, 2020

RuntimeSceneComponent: support for RectTransform added to Focus method.
RuntimeSelectionComponent: GetNextIndex returns next index only if pointer position remains the same.
SceneGizmo: fixed added base.Start call.
SceneView.prefab: Settings Panel added.
SceneSettingsPanel and SceneSettingsComponent added.
PropertyChangedEventHandler added to UIControls/Common
RectTool: lock m_currentAxis to XY if all active targets are RectTransforms
RectTransformGizmo added.
TransformChangesTracker added.
IRTEComponent interface added.
ComponentEditor.m_gizmos array replaced with Dictonary<RuntimeWindow,Component>
RTEBase.Delete, RTEBase.Duplicate: redundant !Undo.Enable handling block removed.
RTEBase.Duplicate: Setting duplcate parent using last param of Instantiate method. This ensures that scale of duplicate will be correct.
ISettingsComponent: SelectedTheme property added, Themes property setter added, SetThemeExample added.
RTEAppearance: Colors, Cursors, AssetIcons - are hidden from inspector

##[RTEditor] - October 12, 2020