using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public interface ITerrainAreaProjector
    {
        Texture2D Brush
        {
            get;
            set;
        }

        float BrushOpacity
        {
            get;
            set;
        }

        Vector3 Position
        {
            get;
            set;
        }

        Vector3 Scale
        {
            get;
            set;
        }

        void Destroy();
    }

    public class TerrainAreaProjector : MonoBehaviour, ITerrainAreaProjector
    {
        private Renderer m_decal;
        public virtual Texture2D Brush
        {
            get { return (Texture2D)m_decal.material.GetTexture("_MainTex"); }
            set { m_decal.material.SetTexture("_MainTex", value); }
        }

        public float BrushOpacity
        {
            get { return m_decal.material.GetColor("_Color").a; }
            set
            {
                Color color = m_decal.material.GetColor("_Color");
                color.a = value;
                m_decal.material.SetColor("_Color", color);
            }
        }

        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        public Vector3 Scale
        {
            get 
            {
                Vector3 localScale = transform.localScale;
                return new Vector3(localScale.x, transform.localScale.z, localScale.y);
            }
            set 
            {
                Vector3 localScale = transform.localScale;
                localScale.x = value.x;
                localScale.y = value.z;
                transform.localScale = localScale;
            }
        }

        private void Awake()
        {
            m_decal = GetComponent<Renderer>();
        }
 
        public void Destroy()
        {
            if(this != null)
            {
                Destroy(gameObject);
            }
        }
    }
}

