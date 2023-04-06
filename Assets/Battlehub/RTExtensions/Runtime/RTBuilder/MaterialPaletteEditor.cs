using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.UIControls;
using Battlehub.Utils;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTBuilder
{
    public class MaterialPaletteEditor : MonoBehaviour
    {
        private const string DataFolder = "RTBuilderData/";
        private const string PaletteFile = "DefaultMaterialPalette";
        private const string MaterialFile = "Material";

        [SerializeField]
        private Button m_createMaterialButton = null;
        public Button CreateMaterialButton
        {
            get { return m_createMaterialButton; }
        }

        [SerializeField]
        private VirtualizingTreeView m_treeView = null;
        public VirtualizingTreeView TreeView
        {
            get { return m_treeView; }
        }

        private Texture m_texture = null;
        public Texture Texture
        {
            get { return m_texture; }
            set
            {
                m_texture = value;
                m_texturePicker.Reload();
                Material material = (Material)TreeView.SelectedItem;
                if (material != null)
                {
                    m_paletteManager.ApplyTexture(material, m_texture);
                }
            }
        }

        public Material SelectedMaterial
        {
            get { return (Material)TreeView.SelectedItem; }
        }

        [SerializeField]
        private TexturePicker m_texturePicker = null;
        [SerializeField]
        private GameObject m_texturePickerPlaceholder = null;

        private IProBuilderTool m_proBuilderTool;
        private IMaterialPaletteManager m_paletteManager;
        private ILocalization m_localization;

        private IRTE m_editor;
        public IRTE Editor
        {
            get { return m_editor; }
        }

        private RuntimeWindow m_probuilderWindow;

        protected virtual void Awake()
        {
            m_texturePicker.gameObject.SetActive(false);
            m_texturePicker.Init(this, Strong.PropertyInfo((MaterialPaletteEditor x) => x.Texture), false);
        }

        protected virtual IEnumerator Start()
        {
            m_probuilderWindow = GetComponentInParent<RuntimeWindow>();

            m_editor = IOC.Resolve<IRTE>();
            
            m_proBuilderTool = IOC.Resolve<IProBuilderTool>();
            m_paletteManager = IOC.Resolve<IMaterialPaletteManager>();
            m_localization = IOC.Resolve<ILocalization>();

            TreeView.ItemDataBinding += OnItemDataBinding;
            TreeView.ItemDrop += OnItemDrop;
            TreeView.SelectionChanged += OnSelectionChanged;

            TreeView.CanEdit = false;
            TreeView.CanReorder = true;
            TreeView.CanReparent = false;
            TreeView.CanSelectAll = false;
            TreeView.CanUnselectAll = true;
            TreeView.CanRemove = false;
            TreeView.CanMultiSelect = false;

            if (m_paletteManager != null)
            {
                m_paletteManager.PaletteChanged += OnPaletteChanged;
                m_paletteManager.MaterialAdded += OnMaterialAdded;
                m_paletteManager.MaterialCreated += OnMaterialCreated;
                m_paletteManager.MaterialReplaced += OnMaterialReplaced;
                m_paletteManager.MaterialRemoved += OnMaterialRemoved;
                
                yield return new WaitUntil(() => m_paletteManager.IsReady);
                yield return new WaitWhile(() => m_editor.IsBusy);

                if (CreateMaterialButton != null)
                {
                    CreateMaterialButton.onClick.AddListener(CreateMaterial);
                }

                TreeView.Items = m_paletteManager.Palette.Materials;
            }
        }

        protected virtual void OnDestroy()
        {
            if (TreeView != null)
            {
                TreeView.ItemDataBinding -= OnItemDataBinding;
                TreeView.ItemDrop -= OnItemDrop;
                TreeView.SelectionChanged -= OnSelectionChanged;
            }

            if (CreateMaterialButton != null)
            {
                CreateMaterialButton.onClick.RemoveListener(CreateMaterial);
            }

            if (m_paletteManager != null)
            {
                m_paletteManager.MaterialAdded -= OnMaterialAdded;
                m_paletteManager.MaterialCreated -= OnMaterialCreated;
                m_paletteManager.MaterialReplaced -= OnMaterialReplaced;
                m_paletteManager.MaterialRemoved -= OnMaterialRemoved;
                m_paletteManager.PaletteChanged -= OnPaletteChanged;
            }
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            Material material = (Material)e.Item;

            MaterialPaletteItem paletteItem = e.ItemPresenter.GetComponent<MaterialPaletteItem>();
            paletteItem.Material = material;

            int index = m_paletteManager.Palette.Materials.IndexOf(material);
            if (index > 10)
            {
                paletteItem.Text = m_localization.GetString("ID_RTBuilder_MaterialPalette_Apply", "Apply");
            }
            else
            {
                paletteItem.Text = m_localization.GetString("ID_RTBuilder_MaterialPalette_Alt", "Alt + ") + (m_paletteManager.Palette.Materials.IndexOf(material) + 1) % 10;
            }
        }

        private void OnItemDrop(object sender, ItemDropArgs args)
        {
            TreeView.ItemDropStdHandler<Material>(args,
                (item) => null,
                (item, parent) => { },
                (item, parent) => m_paletteManager.Palette.Materials.IndexOf(item),
                (item, parent) => m_paletteManager.Palette.Materials.Remove(item),
                (item, parent, i) => m_paletteManager.Palette.Materials.Insert(i, item),
                (item, parent) => m_paletteManager.Palette.Materials.Add(item));

            for (int i = 0; i < m_paletteManager.Palette.Materials.Count; ++i)
            {
                TreeView.DataBindItem(m_paletteManager.Palette.Materials[i]);
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedArgs e)
        {
            Material material = (Material)e.NewItem;
            if (material != null)
            {
                Texture = material.MainTexture();
                m_texturePickerPlaceholder.gameObject.SetActive(true);
                m_texturePicker.gameObject.SetActive(true);
                TreeView.ScrollIntoView(material);
            }
            else
            {
                m_texturePickerPlaceholder.gameObject.SetActive(false);
                m_texturePicker.gameObject.SetActive(false);
                Texture = null;
            }
        }

        public virtual bool CanDrop()
        {
            return false;
        }

        public virtual void CompleteDragDrop()
        {

        }

        public void ApplyMaterial(Material material)
        {
            if (m_proBuilderTool != null)
            {
                m_proBuilderTool.ApplyMaterial(material);
            }
        }

        public void CreateMaterial()
        {
            m_paletteManager.CreateMaterial();
        }

        public void ReplaceMaterial(Material oldMaterial, Material newMaterial)
        {
            m_paletteManager.ReplaceMaterial(oldMaterial, newMaterial);
        }

        public void RemoveMaterial(Material material)
        {
            TreeView.RemoveChild(null, material);
            m_paletteManager.RemoveMaterial(material);
        }

        public void SelectMaterial(Material material)
        {
            TreeView.SelectedItem = material;
        }

        public void SelectFacesByMaterial(Material material)
        {
            if (m_proBuilderTool != null)
            {
                m_proBuilderTool.SelectFaces(material);
            }
        }

        public void UnselectFacesByMaterial(Material material)
        {
            if (m_proBuilderTool != null)
            {
                m_proBuilderTool.UnselectFaces(material);
            }
        }

        private void OnPaletteChanged(MaterialPalette palette)
        {
            TreeView.Items = palette.Materials;
            TreeView.SelectedItem = palette.Materials.FirstOrDefault();
        }


        private void OnMaterialCreated(Material material)
        {
            TreeView.Add(material);
            TreeView.SelectedItem = material;
            TreeView.ScrollIntoView(material);
        }

        private void OnMaterialAdded(Material material)
        {
            TreeView.Add(material);
        }

        private void OnMaterialReplaced(Material oldMaterial, Material newMaterial)
        {
            bool wasSelected = TreeView.IsItemSelected(oldMaterial);
            int index = TreeView.IndexOf(oldMaterial);
            TreeView.RemoveChild(null, oldMaterial);
            TreeView.AddChild(null, newMaterial);
            TreeView.SetIndex(newMaterial, index);
            if (wasSelected)
            {
                TreeView.SelectedItem = newMaterial;
            }
        }

        private void OnMaterialRemoved(Material material)
        {
            for (int i = 0; i < m_paletteManager.Palette.Materials.Count; ++i)
            {
                TreeView.DataBindItem(m_paletteManager.Palette.Materials[i]);
            }
        }

        protected virtual void Update()
        {
            if (m_editor.ActiveWindow == null || m_editor.ActiveWindow != m_probuilderWindow)
            {
                return;
            }

            if (m_proBuilderTool == null || !m_proBuilderTool.HasSelection)
            {
                return;
            }

            IInput input = m_editor.Input;
            if (input.GetKeyDown(KeyCode.Delete))
            {
                if (TreeView.SelectedItem != null)
                {
                    foreach (Material material in TreeView.SelectedItems)
                    {
                        m_paletteManager.RemoveMaterial(material);
                    }
                }

                TreeView.RemoveSelectedItems();
            }
        }
    }
}

