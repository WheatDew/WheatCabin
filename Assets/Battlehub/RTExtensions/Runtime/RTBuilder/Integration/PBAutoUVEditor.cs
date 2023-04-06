using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Battlehub.ProBuilderIntegration
{
    public interface IAutoUVEditor
    {
        void ApplySettings(PBAutoUnwrapSettings settings, MeshSelection selection);
        PBAutoUnwrapSettings GetSettings(MeshSelection selection);

        bool HasAutoUV(MeshSelection selection, bool auto);
        void SetAutoUV(MeshSelection selection, bool auto);
        void ResetUV(MeshSelection selection);

        void GroupFaces(MeshSelection selection);
        void UngroupFaces(MeshSelection selection);
        MeshSelection SelectFaceGroup(MeshSelection currentSelection);
    }

    public class PBAutoUnwrapSettings
    {
        public event Action Changed;

        public enum Anchor
        {
            UpperLeft = 0,
            UpperCenter = 1,
            UpperRight = 2,
            MiddleLeft = 3,
            MiddleCenter = 4,
            MiddleRight = 5,
            LowerLeft = 6,
            LowerCenter = 7,
            LowerRight = 8,
            None = 9
        }
        public enum Fill
        {
            Fit = 0,
            Tile = 1,
            Stretch = 2
        }

        private AutoUnwrapSettings m_settings;

        public PBAutoUnwrapSettings()
        {
            m_settings = AutoUnwrapSettings.defaultAutoUnwrapSettings;
        }

        public PBAutoUnwrapSettings(AutoUnwrapSettings unwrapSettings)
        {
            m_settings = unwrapSettings;
        }
        public PBAutoUnwrapSettings(PBAutoUnwrapSettings unwrapSettings)
        {
            m_settings = new AutoUnwrapSettings(unwrapSettings.m_settings);
        }

        public static PBAutoUnwrapSettings stretch
        {
            get { return new PBAutoUnwrapSettings(AutoUnwrapSettings.stretch); }
        }

        public static PBAutoUnwrapSettings fit
        {
            get { return new PBAutoUnwrapSettings(AutoUnwrapSettings.fit); }
        }

        public static PBAutoUnwrapSettings defaultAutoUnwrapSettings
        {
            get { return new PBAutoUnwrapSettings(AutoUnwrapSettings.defaultAutoUnwrapSettings); }
        }

        public static PBAutoUnwrapSettings tile
        {
            get { return new PBAutoUnwrapSettings(AutoUnwrapSettings.tile); }
        }

        public Anchor anchor
        {
            get { return (Anchor)m_settings.anchor; }
            set
            {
                var newValue = (AutoUnwrapSettings.Anchor)value;
                if(newValue != m_settings.anchor)
                {
                    m_settings.anchor = newValue;
                    RaiseChanged();
                }
            }
        }


        public float rotation
        {
            get { return m_settings.rotation;  }
            set
            {
                if(m_settings.rotation != value)
                {
                    float clampedValue = value - Mathf.CeilToInt(value / 360f) * 360f;
                    if (clampedValue < 0)
                    {
                        clampedValue += 360f;
                    }

                    m_settings.rotation = clampedValue;
                    RaiseChanged();
                }
            }
        }

        public Vector2 offset
        {
            get { return m_settings.offset; }
            set
            {
                if(m_settings.offset != value)
                {
                    m_settings.offset = value;
                    RaiseChanged();
                }
            }
        }

        public Vector2 scale
        {
            get { return m_settings.scale; }
            set
            {
                if(m_settings.scale != value)
                {
                    m_settings.scale = value;
                    RaiseChanged();
                }
            }
        }

        public Fill fill
        {
            get { return (Fill)m_settings.fill; }
            set
            {
                var newValue = (AutoUnwrapSettings.Fill)value; 
                if (m_settings.fill != newValue)
                {
                    m_settings.fill = newValue;
                    RaiseChanged();
                }
            }
        }

        public bool swapUV
        {
            get { return m_settings.swapUV; }
            set
            {
                if(m_settings.swapUV != value)
                {
                    m_settings.swapUV = value;
                    RaiseChanged();
                }
            }
        }

        public bool flipV
        {
            get { return m_settings.flipV; }
            set
            {
                if(m_settings.flipV != value)
                {
                    m_settings.flipV = value;
                    RaiseChanged();
                }
            }
        }

        public bool flipU
        {
            get { return m_settings.flipU; }
            set
            {
                if(m_settings.flipU != value)
                {
                    m_settings.flipU = value;
                    RaiseChanged();
                }
            }
        }

        public bool useWorldSpace
        {
            get { return m_settings.useWorldSpace; }
            set
            {
                if(m_settings.useWorldSpace != value)
                {
                    m_settings.useWorldSpace = value;
                    RaiseChanged();
                }
            }
        }

        public void Reset()
        {
            m_settings.Reset();
        }

        private void RaiseChanged()
        {
            if(Changed != null)
            {
                Changed();
            }
        }

        public void CopyFrom(PBAutoUnwrapSettings settings)
        {
            m_settings = settings.m_settings;
        }

        public override string ToString()
        {
            return m_settings.ToString();
        }

        public static implicit operator AutoUnwrapSettings(PBAutoUnwrapSettings settings)
        {
            return settings.m_settings;
        }

        public static implicit operator PBAutoUnwrapSettings(AutoUnwrapSettings settings)
        {
            return new PBAutoUnwrapSettings(settings);
        }
    }

    public class PBAutoUVEditor : MonoBehaviour, IAutoUVEditor
    {
        public bool HasAutoUV(MeshSelection selection, bool auto)
        {
            if (selection == null)
            {
                return false;
            }

            selection = selection.ToFaces(false, false);
            IList<Face> faces = new List<Face>();
            foreach (KeyValuePair<GameObject, IList<int>> kvp in selection.SelectedFaces)
            {
                ProBuilderMesh mesh = kvp.Key.GetComponent<ProBuilderMesh>();

                faces.Clear();
                mesh.GetFaces(kvp.Value, faces);
                for (int i = 0; i < faces.Count; ++i)
                {
                    Face face = faces[i];
                    if(auto)
                    {
                        if (!face.manualUV)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (face.manualUV)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void SetAutoUV(MeshSelection selection, bool auto)
        {
            if (selection == null)
            {
                return;
            }

            selection = selection.ToFaces(false, false);

            List<Face> faces = new List<Face>();
            foreach(KeyValuePair<GameObject, IList<int>> kvp in selection.SelectedFaces)
            {
                ProBuilderMesh mesh = kvp.Key.GetComponent<ProBuilderMesh>();

                faces.Clear();
                mesh.GetFaces(kvp.Value, faces);

                PBAutoUVConversion.SetAutoUV(mesh, faces.ToArray(), auto);
                mesh.ToMesh();
                mesh.Refresh();
            }
        }

        public void ResetUV(MeshSelection selection)
        {
            if (selection == null)
            {
                return;
            }

            selection = selection.ToFaces(false, false);

            List<Face> faces = new List<Face>();
            foreach (KeyValuePair<GameObject, IList<int>> kvp in selection.SelectedFaces)
            {
                ProBuilderMesh mesh = kvp.Key.GetComponent<ProBuilderMesh>();

                faces.Clear();
                mesh.GetFaces(kvp.Value, faces);

                for(int i = 0; i < faces.Count; ++i)
                {
                    faces[i].uv = AutoUnwrapSettings.defaultAutoUnwrapSettings;
                }

                mesh.RefreshUV(faces);


                mesh.ToMesh();
                mesh.Refresh();
            }
        }

        public void ApplySettings(PBAutoUnwrapSettings settings, MeshSelection selection)
        {
            if (selection == null)
            {
                return;
            }

            selection = selection.ToFaces(false, false);

            AutoUnwrapSettings unwrapSettings = settings;
            IList<Face> faces = new List<Face>();
            foreach (KeyValuePair<GameObject, IList<int>> kvp in selection.SelectedFaces)
            {
                ProBuilderMesh mesh = kvp.Key.GetComponent<ProBuilderMesh>();

                faces.Clear();
                mesh.GetFaces(kvp.Value, faces);
                for (int i = 0; i < faces.Count; ++i)
                {
                    Face face = faces[i];
                    face.uv = unwrapSettings;
                }

                mesh.RefreshUV(faces);


                mesh.ToMesh();
                mesh.Refresh();
            }    
        }

        public PBAutoUnwrapSettings GetSettings(MeshSelection selection)
        {
            if (selection == null)
            {
                return PBAutoUnwrapSettings.defaultAutoUnwrapSettings;
            }

            selection = selection.ToFaces(false, false);

            PBAutoUnwrapSettings unwrapSettings = PBAutoUnwrapSettings.defaultAutoUnwrapSettings;
            IList<Face> faces = new List<Face>();
            foreach (KeyValuePair<GameObject, IList<int>> kvp in selection.SelectedFaces)
            {
                ProBuilderMesh mesh = kvp.Key.GetComponent<ProBuilderMesh>();
                faces.Clear();
                mesh.GetFaces(kvp.Value, faces);
                for (int i = 0; i < faces.Count;)
                {
                    Face face = faces[i];
                    unwrapSettings = face.uv;
                    break;
                }
            }

            return unwrapSettings;
        }

        private int GetUnusedTextureGroup(ProBuilderMesh mesh)
        {
            return mesh.faces.Max(f => f.textureGroup) + 2;
        }

        private void SetTextureGroup(MeshSelection selection, int textureGroup = 0)
        {
            selection = selection.ToFaces(false, false);

            List<Face> faces = new List<Face>();
            Dictionary<GameObject, IList<int>> selectedFaces = selection.SelectedFaces;
            foreach (KeyValuePair<GameObject, IList<int>> kvp in selectedFaces)
            {
                faces.Clear();

                ProBuilderMesh mesh = kvp.Key.GetComponent<ProBuilderMesh>();
                mesh.GetFaces(kvp.Value, faces);

                int texGroup = textureGroup >= 0 ? GetUnusedTextureGroup(mesh) : -1;
                for (int i = 0; i < faces.Count; ++i)
                {
                    Face face = faces[i];
                    face.textureGroup = texGroup;
                }

                mesh.RefreshUV(faces);

                mesh.ToMesh();
                mesh.Refresh();
            }
        }

        public void GroupFaces(MeshSelection selection)
        {
            SetTextureGroup(selection);
        }

        public void UngroupFaces(MeshSelection selection)
        {
            SetTextureGroup(selection, -1);
        }

        public MeshSelection SelectFaceGroup(MeshSelection currentSelection)
        {
            if(currentSelection == null)
            {
                return null;
            }

            currentSelection = currentSelection.ToFaces(false);
            if(!currentSelection.HasFaces)
            {
                return null;
            }

            
            ProBuilderMesh mesh = currentSelection.SelectedFaces.Last().Key.GetComponent<ProBuilderMesh>();
            IList<int> currentlySelectedFaces = currentSelection.SelectedFaces.Last().Value;
            if(currentlySelectedFaces.Count == 0)
            {
                return currentSelection;
            }
            //HashSet<int> facesHs = new HashSet<int>(currentlySelectedFaces);
            int faceIndex = currentlySelectedFaces.Last();
            int textureGroup = mesh.faces[faceIndex].textureGroup;
            if(textureGroup == -1)
            {
                return currentSelection;
            }

            MeshSelection selection = new MeshSelection();
            IList<Face> faces = mesh.faces;
            List<int> selectedFaces = new List<int>();
            for (int i = 0; i < faces.Count; ++i)
            {
                Face face = faces[i];
                if (face.textureGroup == textureGroup /*&& !facesHs.Contains(i)*/)
                {
                    selectedFaces.Add(i);
                }
            }

            selection.SelectedFaces.Add(mesh.gameObject, selectedFaces);
            return selection;
        }
    }
}
