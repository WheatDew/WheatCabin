using Battlehub.RTCommon;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTEditor.Models
{
    public class InspectorModel : MonoBehaviour, IInspectorModel
    {
        public event EventHandler<InspectorModelEventArgs> BeginEdit;
        public event EventHandler<InspectorModelEventArgs> EndEdit;
        public event EventHandler<InspectorModelEventArgs> PreviewsChanged;

        public bool IsEditing
        {
            get;
            private set;
        }

        public object[] m_targets = new object[0];
        public object[] Targets
        {
            get { return m_targets; }
            private set { m_targets = value; }
        }

        public IEnumerable<Component> TargetComponents
        {
            get
            {
                for (int i = 0; i < Targets.Length; ++i)
                {
                    object target = Targets[i];
                    if (target is Component)
                    {
                        yield return (Component)target;
                    }
                }
            }
        }

        protected virtual void Awake()
        {
            if (!IOC.IsFallbackRegistered<IInspectorModel>())
            {
                IOC.RegisterFallback<IInspectorModel>(this);
            }
        }
        protected virtual void OnDestroy()
        {
            IOC.UnregisterFallback<IInspectorModel>(this);
        }

        public void NotifyBeginEdit(object[] targets)
        {
            if (targets == null)
            {
                Targets = new object[0];
            }
            else
            {
                Targets = targets;
            }
            IsEditing = true;

            if (BeginEdit != null)
            {
                InspectorModelEventArgs args = new InspectorModelEventArgs(targets);
                BeginEdit(this, args);
            }
        }
        public void NotifyEndEdit(object[] targets)
        {
            if(EndEdit != null)
            {
                InspectorModelEventArgs args = new InspectorModelEventArgs(targets);
                EndEdit(this, args);
            }

            Targets = new object[0];
            IsEditing = false;
        }

        public void NotifyPreviewsChanged(object[] targets)
        {
            if(PreviewsChanged != null)
            {
                InspectorModelEventArgs args = new InspectorModelEventArgs(targets);
                PreviewsChanged(this, args);
            }
        }
    }
}

