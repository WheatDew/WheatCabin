//using System;
using UnityEngine;

namespace Battlehub.RTEditor
{

    [AddComponentMenu(""), /*Obsolete*/]
    public class AnimationViewImpl : MonoBehaviour
    {
        private AnimationView m_animationView;
        protected AnimationView AnimationView
        {
            get { return m_animationView; }
        }
        
        protected virtual void Start()
        {
            m_animationView = GetComponent<AnimationView>();
            m_animationView.Editor.Selection.SelectionChanged += OnSelectionChanged;
            OnSelectionChanged(null);
        }

        protected virtual void OnDestroy()
        {
            if(m_animationView != null && m_animationView.Editor != null)
            {
                m_animationView.Editor.Selection.SelectionChanged -= OnSelectionChanged;
            }            
        }

        protected virtual void OnSelectionChanged(Object[] unselectedObjects)
        {
            AnimationView.TargetGameObject = m_animationView.Editor.Selection.activeGameObject;
        }
    }

}
