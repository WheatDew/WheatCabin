using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTBuilder
{
    public interface IMaterialPaletteManager
    {
        event Action<Material> MaterialCreated;
        event Action<Material> MaterialAdded;
        event Action<Material> MaterialRemoved;
        event Action<MaterialPalette> PaletteChanged;
        event Action<Material, Material> MaterialReplaced;

        bool IsReady
        {
            get;
        }

        MaterialPalette Palette
        {
            get;
        }
        
        void CreateMaterial();
        void AddMaterial(Material material, bool setUniqueName = false);
        void ReplaceMaterial(Material oldMaterial, Material newMaterial);
        void RemoveMaterial(Material material);
        void ApplyTexture(Material material, Texture texture);
    }

    [DefaultExecutionOrder(-50)]
    public class MaterialPaletteManager : MonoBehaviour, IMaterialPaletteManager
    {
        public event Action<Material> MaterialCreated;
        public event Action<Material> MaterialAdded;
        public event Action<Material> MaterialRemoved;
        public event Action<MaterialPalette> PaletteChanged;
        public event Action<Material, Material> MaterialReplaced;

        public bool IsReady
        {
            get;
            private set;
        }

        private MaterialPalette m_palette;
        public MaterialPalette Palette
        {
            get { return m_palette; }
        }

        private IRTE m_rte;
        private IRuntimeEditor m_editor;
        private IProBuilderTool m_proBuilderTool;

        private void Awake()
        {
            IOC.RegisterFallback<IMaterialPaletteManager>(this);

            m_rte = IOC.Resolve<IRTE>();
            m_editor = IOC.Resolve<IRuntimeEditor>();
            if(m_editor != null)
            {
                m_editor.SceneLoaded += OnSceneLoaded;
            }
            
            m_proBuilderTool = IOC.Resolve<IProBuilderTool>();

            InitPalette();
            IsReady = true;
        }

        private void OnDestroy()
        {
            if(m_editor != null)
            {
                m_editor.SceneLoaded -= OnSceneLoaded;
            }
            IOC.UnregisterFallback<IMaterialPaletteManager>(this);
        }

        private void OnSceneLoaded()
        {
            InitPalette();
            if(PaletteChanged != null)
            {
                PaletteChanged(m_palette);
            }
        }

        private void InitPalette()
        {
            m_palette = FindObjectOfType<MaterialPalette>();
            if (m_palette == null)
            {
                GameObject go = new GameObject("MaterialPalette");
                m_palette = go.AddComponent<MaterialPalette>();

                Material material = Instantiate(PBBuiltinMaterials.DefaultMaterial);
                material.name = "Default";
                m_palette.Materials.Add(material);
            }
            CleanPalette();
        }

        public void CreateMaterial()
        {
            Material material = Instantiate(PBBuiltinMaterials.DefaultMaterial);
            material.name = PathHelper.GetUniqueName("Material", m_palette.Materials.Select(m => m.name).ToList());
            m_palette.Materials.Add(material);

            if(MaterialCreated != null)
            {
                MaterialCreated(material);
            }
        }

        public void AddMaterial(Material material, bool setUniqueName)
        {
            if(setUniqueName)
            {
                material.name = PathHelper.GetUniqueName("Material", m_palette.Materials.Select(m => m.name).ToList());
            }

            m_palette.Materials.Add(material);
            if(MaterialAdded != null)
            {
                MaterialAdded(material);
            }
        }

        public void ReplaceMaterial(Material oldMaterial, Material newMaterial)
        {
            int index = m_palette.Materials.IndexOf(oldMaterial);
            m_palette.Materials.RemoveAt(index);
            if(!m_palette.Materials.Contains(newMaterial))
            {
                m_palette.Materials.Insert(index, newMaterial);
            }
            if(MaterialReplaced != null)
            {
                MaterialReplaced(oldMaterial, newMaterial);
            }
        }

        public void RemoveMaterial(Material material)
        {
            m_palette.Materials.Remove(material);
            if(MaterialRemoved != null)
            {
                MaterialRemoved(material);
            }
        }

        public void ApplyTexture(Material material, Texture texture)
        {
            Texture oldTexture = material.MainTexture();
            Texture newTexture = texture;
            material.MainTexture(newTexture);

            m_rte.Undo.CreateRecord(record =>
            {
                material.MainTexture(newTexture);
                return true;
            },
            record =>
            {
                material.MainTexture(oldTexture);
                return true;
            });
        }

        private void CleanPalette()
        {
            if (m_palette.Materials == null)
            {
                m_palette.Materials = new List<Material>();
                return;
            }

            m_palette.Materials = m_palette.Materials.Where(m => m != null).ToList();
        }


        protected virtual void Update()
        {
            if (m_rte.ActiveWindow == null || m_rte.ActiveWindow != this && m_rte.ActiveWindow.WindowType != RuntimeWindowType.Scene)
            {
                return;
            }

            IInput input = m_rte.Input;
            bool select = input.GetKey(KeyCode.S);
            bool unselect = input.GetKey(KeyCode.U);

            if (!select && !unselect)
            {
                if (!m_proBuilderTool.HasSelection)
                {
                    return;
                }
            }
            
            if (!input.GetKey(KeyCode.LeftAlt) && !input.GetKey(KeyCode.RightAlt) && !input.GetKey(KeyCode.AltGr))
            {
                return;
            }

            for (int keyCode = (int)KeyCode.Alpha0; keyCode <= (int)KeyCode.Alpha0 + 9; ++keyCode)
            {
                if (!input.GetKeyDown((KeyCode)keyCode))
                {
                    continue;
                }

                int index = keyCode - (int)KeyCode.Alpha1;
                if (index == -1)
                {
                    index = 9;
                }

                if (0 <= index && index < Palette.Materials.Count)
                {
                    Material material = Palette.Materials[index];
                    if (material == null)
                    {
                        break;
                    }

                    if(select)
                    {
                        m_proBuilderTool.SelectFaces(material);
                    }
                    else if(unselect)
                    {
                        m_proBuilderTool.UnselectFaces(material);
                    }
                    else
                    {
                        m_proBuilderTool.ApplyMaterial(material);
                    }
                }
            }
        }
    }
}
