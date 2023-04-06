using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTEditor.Models
{
    public class InspectorModelEventArgs : EventArgs
    {
        public object[] m_targets;
        public object[] Targets
        {
            get { return m_targets; }
        }

        public IEnumerable<Component> TargetComponents
        {
            get
            {
                for (int i = 0; i < m_targets.Length; ++i)
                {
                    object target = m_targets[i];
                    if (target is Component)
                    {
                        yield return (Component)target;
                    }
                }
            }
        }

        public InspectorModelEventArgs(object[] targets)
        {
            m_targets = targets;
        }
    }

    public interface IInspectorModel
    {
        event EventHandler<InspectorModelEventArgs> BeginEdit;
        event EventHandler<InspectorModelEventArgs> EndEdit;
        event EventHandler<InspectorModelEventArgs> PreviewsChanged;

        bool IsEditing
        {
            get;
        }

        object[] Targets
        {
            get;
        }

        IEnumerable<Component> TargetComponents
        {
            get;
        }

        void NotifyBeginEdit(object[] targets);
        void NotifyEndEdit(object[] targets);
        void NotifyPreviewsChanged(object[] targets);
    }
}

