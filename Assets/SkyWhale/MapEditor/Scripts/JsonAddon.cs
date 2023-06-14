using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LitJson
{
    public class JsonRegister
    {
        public static int RegFloatType;/** 测试变量 可删除*/
        /// <summary>
        /// 注册扩展类型，在序列化之前，外部直接调用该方法
        /// </summary>
        public static void RegisterExtendType()
        {
            /** 方法二选一即可*/
            RegisterFloat();
            //RegisterFloat2();
        }

        /// <summary>
        /// Float 扩展方法1 --> float转string
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
        /// Float 扩展方法2 --> float转double
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
