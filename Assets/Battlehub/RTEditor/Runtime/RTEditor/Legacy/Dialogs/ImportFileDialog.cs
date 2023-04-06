using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [AddComponentMenu(""), /*Obsolete*/]
    public class ImportFileDialog : RuntimeWindow
    {
        private Dialog m_parentDialog;
        private FileBrowser m_fileBrowser;
        private ILocalization m_localization;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.ImportFile;
            base.AwakeOverride();

            m_localization = IOC.Resolve<ILocalization>();
        }

        private void Start()
        {
            List<Assembly> assemblies = new List<Assembly>();
            foreach (string assemblyName in KnownAssemblies.Names)
            {
                var asName = new AssemblyName();
                asName.Name = assemblyName;

                try
                {
                    Assembly asm = Assembly.Load(asName);
                    assemblies.Add(asm);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.ToString());
                }
            }

            m_parentDialog = GetComponentInParent<Dialog>();
            m_parentDialog.Ok += OnOk;
            m_parentDialog.OkText = m_localization.GetString("ID_RTEditor_ImportFileDialog_Btn_Open", "Open");
            m_parentDialog.IsOkVisible = true;
            m_parentDialog.CancelText = m_localization.GetString("ID_RTEditor_ImportFileDialog_Btn_Cancel", "Cancel");
            m_parentDialog.IsCancelVisible = true;

            m_fileBrowser = GetComponent<FileBrowser>();
            m_fileBrowser.DoubleClick += OnFileBrowserDoubleClick;
            m_fileBrowser.SelectionChanged += OnFileBrowserSelectionChanged;

            IImporterModel importerModel = IOC.Resolve<IImporterModel>();
            m_fileBrowser.AllowedExt = importerModel.Extensions.ToList();
            m_fileBrowser.Icons = importerModel.Icons.Zip(importerModel.Extensions, (icon, ext) => new FileIcon { Icon = icon, Ext = ext }).ToList();
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
            }

            if(m_fileBrowser != null)
            {
                m_fileBrowser.DoubleClick -= OnFileBrowserDoubleClick;
                m_fileBrowser.SelectionChanged -= OnFileBrowserSelectionChanged;
            }
        }

        private void OnOk(Dialog sender, DialogCancelArgs args)
        {
            args.Cancel = true;

            string path = m_fileBrowser.Open();
            if(string.IsNullOrEmpty(path))
            {    
                return;
            }

            if (!File.Exists(path))
            {
                return;
            }

            TryImport(path);
        }

        private void OnFileBrowserSelectionChanged(string path)
        {
            if (File.Exists(path))
            {
                m_parentDialog.OkText = m_localization.GetString("ID_RTEditor_ImportFileDialog_Btn_Import", "Import");
            }
            else
            {
                m_parentDialog.OkText = m_localization.GetString("ID_RTEditor_ImportFileDialog_Btn_Open", "Open");
            }
        }

        private void OnFileBrowserDoubleClick(string path)
        {
            if(File.Exists(path))
            {
                TryImport(path);
            }
        }
        
        private async void TryImport(string path)
        {
            IImporterModel importerModel = IOC.Resolve<IImporterModel>();
            IRTE rte = IOC.Resolve<IRTE>();
            rte.IsBusy = true;
            await importerModel.ImportAsync(path, Path.GetExtension(path).ToLower());
            rte.IsBusy = false;
            m_parentDialog.Close();
        }
    }
}
