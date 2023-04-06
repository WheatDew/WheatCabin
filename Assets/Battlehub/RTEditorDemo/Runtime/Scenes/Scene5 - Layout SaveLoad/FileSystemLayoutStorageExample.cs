using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.UIControls.DockPanels;
using Battlehub.Utils;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene5
{
    /// <summary>
    /// This is an example of file system storage for layouts. By default, the Runtime Editor uses PlayerPrefs to store layouts.
    /// </summary>
    public class FileSystemLayoutStorageExample : BaseLayoutStorageModel
    {
        private void Awake()
        {
            string layoutsFolder = $"{Application.persistentDataPath}/ExampleLayouts";
            Directory.CreateDirectory(layoutsFolder);

            //Take a look at the console to see where the layout files are actually stored
            Debug.Log($"Layouts are saved in {layoutsFolder}");

            //Override default layout storage
            IOC.Register<ILayoutStorageModel>(this);
        }

        private void OnDestroy()
        {
            //Cleanup
            IOC.Unregister<ILayoutStorageModel>(this);
        }

        private string ToAbsolutePath(string path)
        {
            return $"{Application.persistentDataPath}/ExampleLayouts/{path}.layout";
        }


        #region ILayoutStorageModel interface implementations

        public override string DefaultLayoutName => "Layout";

        public override string[] GetLayouts()
        {
            string[] files = Directory.GetFiles($"{Application.persistentDataPath}/ExampleLayouts", "*.layout");
            return files.Select(fileName => Path.GetFileNameWithoutExtension(fileName)).ToArray();
        }

        public override bool LayoutExists(string path)
        {
            return File.Exists(ToAbsolutePath(path));
        }

        public override void SaveLayout(string path, LayoutInfo layout)
        {
            string xml = XmlUtility.ToXml(ToPersistentLayout(layout), Formatting.Indented);
            File.WriteAllText(ToAbsolutePath(path), xml);
        }

        public override LayoutInfo LoadLayout(string path)
        {
            string json = File.ReadAllText(ToAbsolutePath(path));
            return ToLayout(XmlUtility.FromXml<PersistentLayoutInfo>(json));
        }

        public override void DeleteLayout(string path)
        {
            File.Delete(ToAbsolutePath(path));
        }

        #endregion
    }
}