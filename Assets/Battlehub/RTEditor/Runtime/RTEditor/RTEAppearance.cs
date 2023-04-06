
using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public interface IRTEAppearance
    {
        [Obsolete]
        AssetIcon[] AssetIcons
        {
            get;
            set;
        }

        [Obsolete("Use ISettingsComponent.SelectedTheme.GetIcon(\"RTEAsset_{type}\") instead")]
        Sprite GetAssetIcon(string type);
        
        RTECursor[] CursorSettings
        {
            get;
            set;
        }

        RTEColors Colors
        {
            get;
            set;
        }

        CanvasScaler UIBackgroundScaler
        {
            get;
        }

        CanvasScaler UIForegroundScaler
        {
            get;
        }

        [Obsolete("Use UIScale property instead")]
        CanvasScaler UIScaler
        {
            get;
        }

        float UIScale
        {
            get;
            set;
        }

        bool IsMainMenuActive
        {
            get;
            set;
        }

        bool IsFooterActive
        {
            get;
            set;
        }

        bool IsBackgroundImageEnabled
        {
            get;
            set;
        }

        bool IsUIBackgroundActive
        {
            get;
            set;
        }


        void ApplyColors(GameObject root);
        void RegisterPrefab(GameObject prefab);
    }

    [Serializable]
    public class AssetIcon
    {
        public string AssetTypeName;
        public Sprite Icon;
    }


    [Serializable]
    public struct RTECursor
    {
        public string Name;
        public KnownCursor Type;
        public Texture2D Texture;
    }

    [Serializable]
    public struct RTESelectableColors
    {
        public Color Normal;
        public Color Highlight;
        public Color Pressed;
        public Color Disabled;
        public Color Selected;

        public RTESelectableColors(Color normal, Color highlight, Color pressed, Color disabled)
        {
            Normal = normal;
            Highlight = highlight;
            Pressed = pressed;
            Disabled = disabled;
            Selected = new Color32(0, 0x97, 0xFF, 0xFF);
        }

        public RTESelectableColors(Color normal, Color highlight, Color pressed, Color disabled, Color32 selected)
        {
            Normal = normal;
            Highlight = highlight;
            Pressed = pressed;
            Disabled = disabled;
            Selected = selected;
        }


        public bool EqualTo(RTESelectableColors c)
        {
            return
                c.Normal == Normal &&
                c.Highlight == Highlight &&
                c.Pressed == Pressed &&
                c.Disabled == Disabled &&
                c.Selected == Selected;
        }
    }

    [Serializable]
    public struct RTEHierarchyColors
    {
        public Color NormalItem;
        public Color DisabledItem;
        
        public RTEHierarchyColors(Color normal, Color disabled)
        {
            NormalItem = normal;
            DisabledItem = disabled;
        }

        public bool EqualTo(RTEHierarchyColors c)
        {
            return
                c.NormalItem == NormalItem &&
                c.DisabledItem == DisabledItem;
        }
    }

    [Serializable]
    public struct RTEMenuItemColors
    {
        public Color SelectionColor;
        public Color TextColor;
        public Color DisabledSelectionColor;
        public Color DisabledTextColor;

        public RTEMenuItemColors(Color selectionColor, Color textColor, Color disabledSelectionColor, Color disabledTextColor)
        {
            SelectionColor = selectionColor;
            TextColor = textColor;
            DisabledSelectionColor = disabledSelectionColor;
            DisabledTextColor = disabledTextColor;
        }

        public bool EqualTo(RTEMenuItemColors c)
        {
            return
                c.SelectionColor == SelectionColor &&
                c.TextColor == TextColor &&
                c.DisabledSelectionColor == DisabledSelectionColor &&
                c.DisabledTextColor == DisabledTextColor;
        }
    }

    [Serializable]
    public class RTEColors
    {
        public static readonly Color DefaultPrimary = new Color32(0x38, 0x38, 0x38, 0xFF);
        public static readonly Color DefaultSecondary = new Color32(0x27, 0x27, 0x27, 0xFF);
        public static readonly Color DefaultBorder = new Color32(0x20, 0x20, 0x20, 0xFF);
        public static readonly Color DefaultBorder2 = new Color32(0x1A, 0x1A, 0x1A, 0xFF);
        public static readonly Color DefaultBorder3 = new Color32(0x52, 0x4C, 0x4C, 0xFF);
        public static readonly Color DefaultBorder4 = new Color(0x87, 0x87, 0x87, 0xFF);
        public static readonly Color DefaultAccent = new Color32(0x0, 0x97, 0xFF, 0xC0);
        public static readonly Color DefaultText = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
        public static readonly Color DefaultText2 = new Color32(0xFF, 0xFF, 0xFF, 0x7F);
        public static readonly Color DefaultModalOverlay = new Color32(0x00, 0x00, 0x00, 0x40);
        public static readonly Color DefaultMainMenuBar = new Color32(0x38, 0x38, 0x38, 0xFF);
        public static readonly Color DefaultToolsToggle = new Color32(0xC8, 0xC8, 0xC8, 0x7F);
        public static readonly Color DefaultFooter = new Color32(0x38, 0x38, 0x38, 0xFF);
        public static readonly RTESelectableColors DefaultMainMenuButton = new RTESelectableColors(new Color32(0xff, 0xff, 0xff, 0x00), new Color32(0x0, 0x97, 0xFF, 0x7F), new Color32(0x0, 0x97, 0xFF, 0xFF), new Color32(0, 0, 0, 0));
        public static readonly RTEMenuItemColors DefaultMenuItem = new RTEMenuItemColors(new Color32(0x00, 0x97, 0xFF, 0xFF), new Color32(0xFF, 0xFF, 0xFF, 0xFF), new Color32(0x95, 0x93, 0x93, 0x7F), new Color32(0x71, 0x71, 0x71, 0xFF));
        public static readonly RTEHierarchyColors DefaultHierarchy = new RTEHierarchyColors(Color.white, new Color32(0x93, 0x92, 0x92, 0xFF));
        public static readonly Color DefaultProjectFolder = new Color32(0x48, 0x48, 0x48, 0xFF);
        public static readonly RTESelectableColors DefaultConsoleButton = new RTESelectableColors(DefaultPrimary, DefaultPrimary, DefaultSecondary, new Color32(0xC8, 0xC8, 0xC8, 0x7F));
        public static readonly RTESelectableColors DefaultScrollBar = new RTESelectableColors(DefaultPrimary, DefaultPrimary, DefaultSecondary, new Color32(0xC8, 0xC8, 0xC8, 0x7F));
        public static readonly Color DefaultScrollBarBackground = new Color32(0xB2, 0xB2, 0xB2, 0xFF);
        public static readonly RTESelectableColors DefaultToggle = new RTESelectableColors(DefaultSecondary, DefaultSecondary, new Color32(0x20, 0x20, 0x20, 0xFF), new Color32(0x38, 0x38, 0x38, 0x7F));
        public static readonly RTESelectableColors DefaultToggle2 = new RTESelectableColors(DefaultSecondary, DefaultSecondary, new Color32(0x20, 0x20, 0x20, 0xFF), new Color32(0x38, 0x38, 0x38, 0x7F));
        public static readonly RTESelectableColors DefaultToggleButton = new RTESelectableColors(new Color32(0x44, 0x44, 0x44, 0xFF), new Color32(0x38, 0x38, 0x38, 0xFF), new Color32(0x27, 0x27, 0x27, 0xFF), new Color32(0xC8, 0xC8, 0xC8, 0x7F));
        public static readonly RTESelectableColors DefaultInputField = new RTESelectableColors(DefaultSecondary, DefaultSecondary, new Color32(0x20, 0x20, 0x20, 0xFF), new Color32(0x5C, 0x5C, 0x5C, 0x7F), new Color32(0, 0x97, 0xFF, 0xFF));
        public static readonly RTESelectableColors DefaultInputField2 = new RTESelectableColors(DefaultSecondary, DefaultSecondary, new Color32(0x20, 0x20, 0x20, 0xFF), new Color32(0x5C, 0x5C, 0x5C, 0x7F), new Color32(0x20, 0x20, 0x20, 0xFF));
        public static readonly RTESelectableColors DefaultButton = new RTESelectableColors(new Color32(0x44, 0x44, 0x44, 0xFF), new Color32(0x55, 0x55, 0x55, 0xFF), new Color32(0x38, 0x38, 0x38, 0xFF), new Color32(0x5C, 0x5C, 0x5C, 0x66));
        public static readonly RTESelectableColors DefaultButton2 = new RTESelectableColors(new Color(0xFF, 0xFF, 0xFF, 0xFF), new Color32(0xF5, 0xF5, 0xF5, 0xFF), new Color32(0xC8, 0xC8, 0xC8, 0xFF), new Color32(0xC8, 0xC8, 0xC8, 0x7F));
        public static readonly RTESelectableColors DefaultSlider = new RTESelectableColors(new Color32(0x44, 0x44, 0x44, 0xFF), new Color32(0x55, 0x55, 0x55, 0xFF), new Color32(0x38, 0x38, 0x38, 0xFF), new Color32(0x5C, 0x5C, 0x5C, 0x66));
        public static readonly RTESelectableColors DefaultDropdown = new RTESelectableColors(new Color32(0x44, 0x44, 0x44, 0xFF), new Color32(0x55, 0x55, 0x55, 0xFF), new Color32(0x38, 0x38, 0x38, 0xFF), new Color32(0x5C, 0x5C, 0x5C, 0x66));
        public static readonly RTESelectableColors DefaultToolCmdItem = new RTESelectableColors(new Color32(0x00, 0x97, 0xFF, 0xC0), new Color32(0x00, 0x97, 0xFF, 0xFF), new Color32(0x00, 0xB0, 0xFF, 0xFF), new Color32());
        public static readonly RTESelectableColors DefaultHeaderToggle = new RTESelectableColors(DefaultPrimary, DefaultPrimary, DefaultSecondary, new Color32(0x38, 0x38, 0x38, 0x00), DefaultPrimary);
        public static readonly RTESelectableColors DefaultHeaderButton = new RTESelectableColors(DefaultPrimary, DefaultPrimary, DefaultSecondary, new Color32(0x38, 0x38, 0x38, 0x00), DefaultPrimary);

        public Color Primary;
        public Color Secondary;
        public Color Border;
        public Color Border2;
        public Color Border3;
        public Color Border4;
        public Color Accent;
        public Color Text;
        public Color Text2;
        public Color ModalOverlay;
        public Color MainMenuBar;
        public Color Footer;
        public RTESelectableColors MainMenuButton;
        public Color ToolsToggle;
        public RTEMenuItemColors MenuItem;
        public RTEHierarchyColors Hierarchy;
        public Color ProjectFolder;
        public RTESelectableColors ConsoleButton;
        public RTESelectableColors ScrollBar;
        public Color ScrollBarBackground;
        public RTESelectableColors Toggle;
        public RTESelectableColors Toggle2;
        public RTESelectableColors ToggleButton;
        public RTESelectableColors InputField;
        public RTESelectableColors InputField2;
        public RTESelectableColors Button;
        public RTESelectableColors Button2;
        public RTESelectableColors Slider;
        public RTESelectableColors Dropdown;
        public RTESelectableColors ToolCmdItem;
        public RTESelectableColors MobileHeaderToggle;
        public RTESelectableColors MobileHeaderButton;
        
        public RTEColors()
        {
            Primary = DefaultPrimary;
            Secondary = DefaultSecondary;
            Border = DefaultBorder;
            Border2 = DefaultBorder2;
            Border3 = DefaultBorder3;
            Border4 = DefaultBorder4;
            Accent = DefaultAccent;
            Text = DefaultText;
            Text2 = DefaultText2;
            ModalOverlay = DefaultModalOverlay;
            MainMenuBar = DefaultMainMenuBar;
            ToolsToggle = DefaultToolsToggle;
            Footer = DefaultFooter;
            MainMenuButton = DefaultMainMenuButton;
            MenuItem = DefaultMenuItem;
            Hierarchy = DefaultHierarchy;
            ProjectFolder = DefaultProjectFolder;
            ConsoleButton = DefaultConsoleButton;
            ScrollBar = DefaultScrollBar;
            ScrollBarBackground = DefaultScrollBarBackground;
            Toggle = DefaultToggle;
            Toggle2 = DefaultToggle2;
            ToggleButton = DefaultToggleButton;
            InputField = DefaultInputField;
            InputField2 = DefaultInputField2;
            Button = DefaultButton;
            Button2 = DefaultButton2;
            Slider = DefaultSlider;
            Dropdown = DefaultDropdown;
            ToolCmdItem = DefaultToolCmdItem;
            MobileHeaderToggle = DefaultHeaderToggle;
            MobileHeaderButton = DefaultHeaderButton;
        }

        public bool IsDefault
        {
            get
            {
                return
                    Primary == DefaultPrimary &&
                    Secondary == DefaultSecondary &&
                    Border == DefaultBorder &&
                    Border2 == DefaultBorder2 &&
                    Border3 == DefaultBorder3 &&
                    Border4 == DefaultBorder4 &&
                    Accent == DefaultAccent &&
                    Text == DefaultText &&
                    Text2 == DefaultText2 &&
                    ModalOverlay == DefaultModalOverlay &&
                    MainMenuBar == DefaultMainMenuBar &&
                    ToolsToggle == DefaultToolsToggle &&
                    Footer == DefaultFooter &&
                    MainMenuButton.EqualTo(DefaultMainMenuButton) &&
                    MenuItem.EqualTo(DefaultMenuItem) &&
                    Hierarchy.EqualTo(DefaultHierarchy) &&
                    ProjectFolder == DefaultProjectFolder &&
                    ConsoleButton.EqualTo(DefaultConsoleButton) &&
                    ScrollBar.EqualTo(DefaultScrollBar) &&
                    ScrollBarBackground == DefaultScrollBarBackground &&
                    Toggle.EqualTo(DefaultToggle) &&
                    Toggle2.EqualTo(DefaultToggle2) &&
                    ToggleButton.EqualTo(DefaultToggleButton) &&
                    InputField.EqualTo(DefaultInputField) &&
                    InputField2.EqualTo(DefaultInputField2) &&
                    Button.EqualTo(DefaultButton) &&
                    Button2.EqualTo(DefaultButton2) &&
                    Slider.EqualTo(DefaultSlider) &&
                    Dropdown.EqualTo(DefaultDropdown) &&
                    ToolCmdItem.EqualTo(DefaultToolCmdItem) &&
                    MobileHeaderToggle.EqualTo(DefaultHeaderToggle) &&
                    MobileHeaderButton.EqualTo(DefaultHeaderButton);
            }
        }
    }

    [DefaultExecutionOrder(-90)]
    public class RTEAppearance : MonoBehaviour, IRTEAppearance
    {
        [SerializeField, HideInInspector]
        private RTECursor[] m_cursorSettings = null;
        public RTECursor[] CursorSettings
        {
            get { return m_cursorSettings; }
            set
            {
                m_cursorSettings = value;
                if (m_editor != null && m_editor.IsOpened)
                {
                    ApplyCursorSettings();
                }
            }
        }

        [SerializeField, HideInInspector]
        public AssetIcon[] m_assetIcons;
        public AssetIcon[] AssetIcons
        {
            get { return m_assetIcons; }
            set
            {
                m_assetIcons = value;
                UpdateAssetTypeToIconDictionary();
            }
        }

        private readonly Dictionary<string, Sprite> m_assetTypeToIcon = new Dictionary<string, Sprite>();
        public Sprite GetAssetIcon(string type)
        {
            Sprite sprite;
            if(m_assetTypeToIcon.TryGetValue(type, out sprite))
            {
                return sprite;
            }
            return null;
        }

        [SerializeField]
        private RectTransform m_mainMenu = null;

        [SerializeField]
        private RectTransform m_footer = null;

        [SerializeField]
        private Image m_backgroundImage = null;

        [SerializeField]
        private RectTransform m_uiBackground = null;

        [SerializeField]
        private RectTransform m_layoutRoot = null;

        [SerializeField]
        private RectTransform m_freeLayer = null;

        [SerializeField]
        private RectTransform m_modalLayer = null;

        [SerializeField]
        private RectTransform m_busyIndicatorLayer = null;

        private float MainMenuHeight
        {
            get 
            { 
                if(m_mainMenu == null)
                {
                    return 0;
                }

                if(m_mainMenu.rect.height == 0)
                {
                    LayoutElement layoutElement = m_mainMenu.GetComponent<LayoutElement>();
                    if(layoutElement != null)
                    {
                        return Mathf.Max(layoutElement.preferredHeight, layoutElement.minHeight);
                    }
                }

                return m_mainMenu.rect.height;
            }
        }

        private float FooterHeight
        {
            get
            {
                if (m_footer == null)
                {
                    return 0;
                }

                if (m_footer.rect.height == 0)
                {
                    LayoutElement layoutElement = m_footer.GetComponent<LayoutElement>();
                    if (layoutElement != null)
                    {
                        return Mathf.Max(layoutElement.preferredHeight, layoutElement.minHeight);
                    }
                }

                return m_footer.rect.height;
            }
        }

        public bool IsMainMenuActive
        {
            get { return m_mainMenu == null || m_mainMenu.gameObject.activeSelf; }
            set
            {
                if(IsMainMenuActive != value)
                {
                    if (m_mainMenu != null)
                    {
                        m_mainMenu.gameObject.SetActive(value);
                    }

                    UpdateLayerOffsets();
                }
            }
        }

        public bool IsBusyIndicatorLayerActive
        {
            get { return m_busyIndicatorLayer == null || m_busyIndicatorLayer.gameObject.activeSelf; }
            set
            {
                if (IsBusyIndicatorLayerActive != value)
                {
                    if (m_busyIndicatorLayer != null)
                    {
                        m_busyIndicatorLayer.gameObject.SetActive(value);
                    }
                }
            }
        }

        public bool IsFooterActive
        {
            get { return m_footer == null || m_footer.gameObject.activeSelf; }
            set
            {
                if (IsFooterActive != value)
                {
                    if (m_footer != null)
                    {
                        m_footer.gameObject.SetActive(value);
                    }

                    UpdateLayerOffsets();
                }
               
            }
        }
        public bool IsBackgroundImageEnabled
        {
            get { return m_backgroundImage == null || m_backgroundImage.enabled; }
            set
            {
                if (IsBackgroundImageEnabled != value)
                {
                    if(m_backgroundImage != null)
                    {
                        m_backgroundImage.enabled = false;
                    }
                }
            }
        }

        public bool IsUIBackgroundActive
        {
            get { return m_uiBackground == null || m_uiBackground.gameObject.activeSelf; }
            set
            {
                if (IsUIBackgroundActive != value)
                {
                    if (m_uiBackground != null)
                    {
                        m_uiBackground.gameObject.SetActive(value);
                    }

                    if(m_uiBackgroundScaler != null)
                    {
                        Canvas canvas = m_uiBackground.GetComponent<Canvas>();
                        if (canvas != null && canvas.worldCamera != null)
                        {
                            canvas.worldCamera.gameObject.SetActive(IsUIBackgroundActive);
                        }
                    }

                    UpdateLayerOffsets();
                }
            }
        }

        [SerializeField]
        private CanvasScaler m_uiBackgroundScaler = null;
        public CanvasScaler UIBackgroundScaler
        {
            get { return m_uiBackgroundScaler; }
        }

        [SerializeField]
        private CanvasScaler m_uiForegroundScaler = null;
        public CanvasScaler UIForegroundScaler
        {
            get { return m_uiForegroundScaler; }
        }

        [SerializeField, Obsolete]
        private CanvasScaler m_uiScaler = null;
        [Obsolete]
        public CanvasScaler UIScaler
        {
            get { return m_uiScaler; }
        }

        public float UIScale
        {
            get { return m_uiForegroundScaler.scaleFactor; }
            set
            {
                if(m_uiForegroundScaler != null)
                {
                    m_uiForegroundScaler.scaleFactor = value;
                }
                
                if(m_uiBackgroundScaler != null)
                {
                    m_uiBackgroundScaler.scaleFactor = value;
                }
            }
        }

        [SerializeField, HideInInspector]
        private RTEColors m_colors = null;
        public RTEColors Colors
        {
            get { return m_colors; }
            set
            {
                m_colors = value;
                ApplyColors(gameObject);
            }
        }


        [SerializeField]
        private List<GameObject> m_prefabs = null;

        private IRTE m_editor;
        private void Awake()
        {
            if(m_assetIcons == null || m_assetIcons.Length == 0)
            {
                AssetIcons =  new[]
                {
                    new AssetIcon { AssetTypeName = "Folder", Icon = Resources.Load<Sprite>("FolderLarge")  },
                    new AssetIcon { AssetTypeName = "Default", Icon = Resources.Load<Sprite>("RTE_Object")  },
                    new AssetIcon { AssetTypeName = "None", Icon = Resources.Load<Sprite>("None")  },
                    new AssetIcon { AssetTypeName = typeof(Scene).FullName, Icon = Resources.Load<Sprite>("FileLarge")  },
                    new AssetIcon { AssetTypeName = typeof(Mesh).FullName, Icon = Resources.Load<Sprite>("RTE_Mesh")  },
                    new AssetIcon { AssetTypeName = typeof(RuntimeAnimationClip).FullName, Icon = Resources.Load<Sprite>("RTE_AnimationClip")  },
                };
            }

            m_editor = IOC.Resolve<IRTE>();

            UpdateAssetTypeToIconDictionary();

            List<RTECursor> cursorSettings;
            if (m_cursorSettings == null)
            {
                cursorSettings = new List<RTECursor>();
            }
            else
            {
                cursorSettings = m_cursorSettings.ToList();
            }

            AddCursorIfRequired(cursorSettings, KnownCursor.DropAllowed, "Drag & Drop Allowed", "RTE_DropAllowed_Cursor");
            AddCursorIfRequired(cursorSettings, KnownCursor.DropNotAllowed, "Drag & Drop Not Allowed", "RTE_DropNotAllowed_Cursor");
            AddCursorIfRequired(cursorSettings, KnownCursor.HResize, "Horizontal Resize", "RTE_HResize_Cursor");
            AddCursorIfRequired(cursorSettings, KnownCursor.VResize, "Vertical Resize", "RTE_VResize_Cursor");
            m_cursorSettings = cursorSettings.ToArray();

            m_editor.IsOpenedChanged += OnIsOpenedChanged;
            OnIsOpenedChanged();
        }

        private void OnDestroy()
        {
            if (m_editor != null)
            {
                m_editor.IsOpenedChanged -= OnIsOpenedChanged;
            }
        }

        private void OnIsOpenedChanged()
        {
            if (m_editor.IsOpened)
            {
                ApplyCursorSettings();
                ApplyColors();
            }
        }

        private void UpdateLayerOffsets()
        {
            if (m_layoutRoot != null)
            {
                if (IsUIBackgroundActive)
                {
                    UpdateLayerOffsets(m_layoutRoot, -MainMenuHeight, FooterHeight); 
                }
                else
                {
                    m_layoutRoot.Stretch();
                }
            }

            if (m_freeLayer != null)
            {
                if (IsUIBackgroundActive)
                {
                    UpdateLayerOffsets(m_freeLayer, MainMenuHeight, -FooterHeight);
                }
                else
                {
                    m_freeLayer.Stretch();
                }
            }

            if (m_modalLayer != null)
            {
                if (IsUIBackgroundActive)
                {
                    m_modalLayer.SetPivot(Vector2.zero);
                    UpdateLayerOffsets(m_modalLayer, MainMenuHeight, -FooterHeight);
                    //m_modalLayer.SetPivot(new Vector2(0.5f, 0.5f));
                }
                else
                {
                    m_modalLayer.Stretch();
                }
            }

            if(m_busyIndicatorLayer != null)
            {
                if (IsUIBackgroundActive)
                {
                    UpdateLayerOffsets(m_busyIndicatorLayer, MainMenuHeight, -FooterHeight);
                }
                else
                {
                    m_busyIndicatorLayer.Stretch();
                }
            }
        }

        private void UpdateLayerOffsets(RectTransform rt, float max, float min)
        {
            if (IsMainMenuActive)
            {
                SetMax(rt, 0, max);
            }
            else
            {
                SetMax(rt, 0, 0);
            }

            if (IsFooterActive)
            {
                SetMin(rt, 0, min);
            }
            else
            {
                SetMin(rt, 0, 0);
            }
        }

        private static void SetMin(RectTransform rt, float minX = 0, float minY = 0)
        {
            if (rt != null)
            {
                Vector2 offsetMin = rt.offsetMin;
                offsetMin.x = minX;
                offsetMin.y = minY;
                rt.offsetMin = offsetMin;
            }
        }

        private static void SetMax(RectTransform rt, float maxX = 0, float maxY = 0)
        {
            if (rt != null)
            {
                Vector2 offsetMax = rt.offsetMax;
                offsetMax.x = maxX;
                offsetMax.y = maxY;
                rt.offsetMax = offsetMax;
            }
        }

        private void UpdateAssetTypeToIconDictionary()
        {
            m_assetTypeToIcon.Clear();
            if (m_assetIcons == null)
            {
                return;
            }

            for (int i = 0; i < m_assetIcons.Length; ++i)
            {
                AssetIcon icon = m_assetIcons[i];
                if (icon.AssetTypeName != null && icon.Icon != null)
                {
                    if (!m_assetTypeToIcon.ContainsKey(icon.AssetTypeName))
                    {
                        m_assetTypeToIcon.Add(icon.AssetTypeName, icon.Icon);
                    }
                }
            }
        }

        private static void AddCursorIfRequired(List<RTECursor> cursorSettings, KnownCursor cursorType, string name, string texture)
        {
            if (!cursorSettings.Any(c => c.Type == cursorType))
            {
                RTECursor cursor = new RTECursor
                {
                    Name = name,
                    Type = cursorType,
                    Texture = Resources.Load<Texture2D>(texture)
                };
                cursorSettings.Add(cursor);
            }
        }

        private void ApplyCursorSettings()
        {
            for (int i = 0; i < m_cursorSettings.Length; ++i)
            {
                RTECursor cursor = m_cursorSettings[i];
                m_editor.CursorHelper.SetCursorTexture(cursor.Type, cursor.Texture);
            }
        }

        private void ApplyColors()
        {
            if (Colors.IsDefault)
            {
                Debug.Log("Using default colors");
                return;
            }
            ApplyColors(gameObject);
        }

        public void ApplyColors(GameObject root)
        {
            UIStyle[] styles = root.GetComponentsInChildren<UIStyle>(true)
                .Union(m_prefabs.Where(p => p != null).SelectMany(p => p.GetComponentsInChildren<UIStyle>(true))).ToArray();
            for(int i = 0; i < styles.Length; ++i)
            {
                UIStyle style = styles[i];
                switch(style.Name)
                {
                    case "PrimaryColor":
                        style.ApplyImageColor(Colors.Primary);
                        break;
                    case "SecondaryColor":
                        style.ApplyImageColor(Colors.Secondary);
                        break;
                    case "BorderColor":
                        style.ApplyImageColor(Colors.Border);
                        break;
                    case "Border2Color":
                        style.ApplyImageColor(Colors.Border2);
                        break;
                    case "Border3Color":
                        style.ApplyImageColor(Colors.Border3);
                        style.ApplyOutlineColor(Colors.Border3);
                        break;
                    case "Border4Color":
                        style.ApplyImageColor(Colors.Border4);
                        style.ApplyOutlineColor(Colors.Border4);
                        break;
                    case "AccentColor":
                        style.ApplyImageColor(Colors.Accent);
                        break;
                    case "TextColor":
                        style.ApplyTextColor(Colors.Text);
                        style.ApplyImageColor(Colors.Text);
                        break;
                    case "Text2Color":
                        style.ApplyTextColor(Colors.Text2);
                        style.ApplyImageColor(Colors.Text2);
                        break;
                    case "ModalOverlayColor":
                        style.ApplyImageColor(Colors.ModalOverlay);
                        break;
                    case "MainMenuBarColor":
                        style.ApplyImageColor(Colors.MainMenuBar);
                        break;
                    case "MainMenuButtonColor":
                        UIMenuStyle.ApplyMainButtonColor(style, Colors.MainMenuButton.Normal, Colors.MainMenuButton.Highlight, Colors.MainMenuButton.Pressed);
                        break;
                    case "MenuItemColor":
                        UIMenuStyle.ApplyMenuItemColor(style, Colors.MenuItem.SelectionColor, Colors.MenuItem.TextColor, Colors.MenuItem.DisabledSelectionColor, Colors.MenuItem.DisabledTextColor);
                        break;
                    case "ToolsToggleColor":
                        style.ApplyImageColor(Colors.ToolsToggle);
                        break;
                    case "FooterColor":
                        style.ApplyImageColor(Colors.Footer);
                        break;
                    case "HierarchyColor":
                        UIEditorStyle.ApplyHierarchyColors(style, Colors.Hierarchy.NormalItem, Colors.Hierarchy.DisabledItem);
                        break;
                    case "ProjectFolderColor":
                        style.ApplyImageColor(Colors.ProjectFolder);
                        break;
                    case "ConsoleButtonColor":
                        style.ApplySelectableColor (Colors.ConsoleButton.Normal, Colors.ConsoleButton.Highlight, Colors.ConsoleButton.Pressed, Colors.ConsoleButton.Disabled, Colors.ConsoleButton.Selected);
                        break;
                    case "ScrollBarColor":
                        style.ApplySelectableColor(Colors.ScrollBar.Normal, Colors.ScrollBar.Highlight, Colors.ScrollBar.Pressed, Colors.ScrollBar.Disabled, Colors.ScrollBar.Selected);
                        break;
                    case "ScrollBarBackgroundColor":
                        Image image = style.GetComponent<Image>();
                        if (image != null)
                        {
                            if (Colors.ScrollBarBackground != RTEColors.DefaultScrollBarBackground)
                            {
                                image.sprite = null;
                            }
                            else
                            {
                                if (image.name.Contains("V"))
                                {
                                    image.sprite = Resources.Load<Sprite>("DarkBackV");
                                }
                                else
                                {
                                    image.sprite = Resources.Load<Sprite>("DarkBackH");
                                }
                            }
                        }
                        style.ApplyImageColor(Colors.ScrollBarBackground);
                        break;
                    case "ToggleColor":
                        style.ApplySelectableColor(Colors.Toggle.Normal, Colors.Toggle.Highlight, Colors.Toggle.Pressed, Colors.Toggle.Disabled, Colors.Toggle.Selected);
                        break;
                    case "Toggle2Color":
                        style.ApplySelectableColor(Colors.Toggle2.Normal, Colors.Toggle2.Highlight, Colors.Toggle2.Pressed, Colors.Toggle2.Disabled, Colors.Toggle2.Selected);
                        break;
                    case "ToggleButtonColor":
                        style.ApplySelectableColor(Colors.ToggleButton.Normal, Colors.ToggleButton.Highlight, Colors.ToggleButton.Pressed, Colors.ToggleButton.Disabled, Colors.ToggleButton.Selected);
                        break;
                    case "InputFieldColor":
                        style.ApplyInputFieldColor(Colors.InputField.Normal, Colors.InputField.Highlight, Colors.InputField.Pressed, Colors.InputField.Disabled, Colors.InputField.Selected);
                        break;
                    case "InputField2Color":
                        style.ApplyInputFieldColor(Colors.InputField2.Normal, Colors.InputField2.Highlight, Colors.InputField2.Pressed, Colors.InputField2.Disabled, Colors.InputField2.Selected);
                        break;
                    case "ButtonColor":
                        style.ApplySelectableColor(Colors.Button.Normal, Colors.Button.Highlight, Colors.Button.Pressed, Colors.Button.Disabled, Colors.Button.Selected);
                        break;
                    case "Button2Color":
                        style.ApplySelectableColor(Colors.Button2.Normal, Colors.Button2.Highlight, Colors.Button2.Pressed, Colors.Button2.Disabled, Colors.Button2.Selected);
                        break;
                    case "SliderColor":
                        style.ApplySelectableColor(Colors.Slider.Normal, Colors.Slider.Highlight, Colors.Slider.Pressed, Colors.Slider.Disabled, Colors.Slider.Selected);
                        break;
                    case "DropdownColor":
                        style.ApplySelectableColor(Colors.Dropdown.Normal, Colors.Dropdown.Highlight, Colors.Dropdown.Pressed, Colors.Dropdown.Disabled, Colors.Dropdown.Selected);
                        break;
                    case "ToolCmdItemColor":
                        UIEditorStyle.ApplyToolCmdItemColor(style, Colors.ToolCmdItem.Normal, Colors.ToolCmdItem.Highlight, Colors.ToolCmdItem.Pressed);
                        break;
                    case "TimlineControlBackgroundColor":
                        UIEditorStyle.ApplyTimelineControlBackgroundColor(style, Colors.Secondary);
                        break;
                    case "MobileHeaderToggleColor":
                        style.ApplySelectableColor(Colors.MobileHeaderToggle.Normal, Colors.MobileHeaderToggle.Highlight, Colors.MobileHeaderToggle.Pressed, Colors.MobileHeaderToggle.Disabled, Colors.MobileHeaderToggle.Selected);
                        break;
                    case "MobileHeaderButtonColor":
                        style.ApplySelectableColor(Colors.MobileHeaderButton.Normal, Colors.MobileHeaderButton.Highlight, Colors.MobileHeaderButton.Pressed, Colors.MobileHeaderButton.Disabled, Colors.MobileHeaderButton.Selected);
                        break;

                }
            }
        }

        public void RegisterPrefab(GameObject prefab)
        {
            m_prefabs.Add(prefab);
        }
    }

}

