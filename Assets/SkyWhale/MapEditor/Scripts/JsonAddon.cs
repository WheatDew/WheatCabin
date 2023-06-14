using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LitJson
{
    public class JsonRegister
    {
        /// <summary>
        /// ע����չ���ͣ������л�֮ǰ���ⲿֱ�ӵ��ø÷���
        /// </summary>
        public static void RegisterExtendType()
        {
            RegisterSceneObjData();
        }

        /// <summary>
        /// Float ��չ����1 --> floatתstring
        /// </summary>
        private static void RegisterFloat()
        {
            void Exporter(float obj, JsonWriter writer)
            {
                writer.Write(obj.ToString());
            }
            JsonMapper.RegisterExporter((ExporterFunc<float>)Exporter);

            float Importer(string obj)
            {
                float fOut;
                if (float.TryParse(obj, out fOut))
                    return fOut;
                return 0;
            }
            JsonMapper.RegisterImporter((ImporterFunc<string, float>)Importer);
        }

        private static void RegisterSceneObjData()
        {
            void Exporter(SceneObjData obj, JsonWriter writer)
            {

                writer.WriteObjectStart();

                writer.WritePropertyName("name");//д��������
                writer.Write(obj.name);//д��ֵ
                writer.WritePropertyName("type");
                writer.Write(obj.type);
                writer.WritePropertyName("detailType");
                writer.Write(obj.detailType);
                writer.WritePropertyName("position");
                writer.Write(string.Format("{0},{1},{2}", obj.position.x, obj.position.y, obj.position.z));
                writer.WritePropertyName("rotation");
                writer.Write(string.Format("{0},{1},{2},{3}", obj.rotation.x, obj.rotation.y, obj.rotation.z, obj.rotation.w));

                writer.WriteObjectEnd();
            }
            JsonMapper.RegisterExporter((ExporterFunc<SceneObjData>)Exporter);

            SceneObjData Importer(string obj)
            {
                
                var sceneObjData = new SceneObjData("test","test","test",Vector3.zero,Quaternion.identity);
                return sceneObjData;
            }
            JsonMapper.RegisterImporter((ImporterFunc<string, SceneObjData>)Importer);
        }

        /// <summary>
        /// Float ��չ����2 --> floatתdouble
        /// </summary>
        private static void RegisterFloat2()
        {
            void Exporter(float obj, JsonWriter writer)
            {
                writer.Write(obj);
            }
            JsonMapper.RegisterExporter((ExporterFunc<float>)Exporter);

            float Importer(double obj)
            {
                return (float)obj;
            }
            JsonMapper.RegisterImporter((ImporterFunc<double, float>)Importer);
        }
    }
}
