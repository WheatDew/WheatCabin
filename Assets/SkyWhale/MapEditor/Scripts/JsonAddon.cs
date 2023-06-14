using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LitJson
{
    public class JsonRegister
    {
        public static int RegFloatType;/** ���Ա��� ��ɾ��*/
        /// <summary>
        /// ע����չ���ͣ������л�֮ǰ���ⲿֱ�ӵ��ø÷���
        /// </summary>
        public static void RegisterExtendType()
        {
            /** ������ѡһ����*/
            RegisterFloat();
            //RegisterFloat2();
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
            RegFloatType = 1;
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
            RegFloatType = 2;
        }
    }
}
