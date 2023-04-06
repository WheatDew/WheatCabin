using ProtoBuf;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainToolState : MonoBehaviour
    {
        [SerializeField]
        public TerrainGridTool.Interpolation Interpolation = TerrainGridTool.Interpolation.Bicubic;

        [SerializeField]
        public float Height = 32;
        [SerializeField]
        public float ZSize = 200;
        [SerializeField]
        public float XSize = 200;
        [System.Obsolete]
        public int Spacing;
        [SerializeField]
        public float ZSpacing = 20;
        [SerializeField]
        public float XSpacing = 20;
        [System.Obsolete]
        public int Size;

        public float[] Grid;
        public float[] HeightMap;
        public Texture2D CutoutTexture;

        public class Record
        {
            public float Height;
            public float ZSize;
            public float XSize;
            public float ZSpacing;
            public float XSpacing;
            public float[] Grid;
            public float[] HeightMap;
            public byte[] CutoutTexture;
        }

        public void Load(Record record)
        {
            Height = record.Height;
            ZSize = record.ZSize;
            XSize = record.XSize;
            ZSpacing = record.ZSpacing;
            XSpacing = record.XSpacing;
            if(record.Grid != null)
            {
                Grid = record.Grid.ToArray();
            }
            else
            {
                Grid = null;
            }
            if(record.HeightMap != null)
            {
                HeightMap = record.HeightMap.ToArray();
            }
            if (CutoutTexture != null)
            {
                Destroy(CutoutTexture);
                CutoutTexture = null;
            }
            if (record.CutoutTexture != null)
            {
                CutoutTexture = new Texture2D(1, 1, TextureFormat.ARGB32, true);
                CutoutTexture.LoadImage(record.CutoutTexture);
            }
        }

        public Record Save()
        {
            Record record = new Record();
            record.Height = Height;
            record.ZSize = ZSize;
            record.XSize = XSize;
            record.ZSpacing = ZSpacing;
            record.XSpacing = XSpacing;
            if(Grid != null)
            {
                record.Grid = Grid.ToArray();
            }
            if(HeightMap != null)
            {
                record.HeightMap = HeightMap.ToArray();
            }
            if(CutoutTexture != null)
            {
                record.CutoutTexture = CutoutTexture.EncodeToPNG();
            }

            return record;
        }
    }
}

