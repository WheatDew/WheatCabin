using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor.UI
{
    public enum LayoutType
    {
        Vertical,
        Horizontal,
    }

    
    public abstract class LayoutGroupAttribute : Attribute
    {
        public string IsInteractableProperty { get; private set; }
        public string IsActiveProperty { get; private set; }
        public LayoutType LayoutType { get; private set; }
        public LayoutGroupAttribute(LayoutType layout, string isInteractablePropery, string isActiveProperty)
        {
            LayoutType = layout;
        }
    }

    public abstract class HorizontalOrVerticalLayoutGroupAttribute : LayoutGroupAttribute
    {
        public float Spacing { get; private set; }
        public int PaddingLeft { get; private set; }
        public int PaddingRight { get; private set; }
        public int PaddingTop { get; private set; }
        public int PaddingBottom { get; private set; }

        public bool ChildForceExpandWidth { get; private set; }
        public bool ChildForceExpandHeight { get; private set; }
        public bool ChildControlWidth { get; private set; }
        public bool ChildControlHeight { get; private set; }
        public TextAnchor ChildAlignment { get; private set; }

        public HorizontalOrVerticalLayoutGroupAttribute(LayoutType layout, float spacing = 0, int padding = 0, bool childForceExpandWidth = true, bool childForceExpandHeight = true, bool childControlWidth = true, bool childControlHeight = true, TextAnchor childAlignment = TextAnchor.UpperLeft, string isInteractableProperty = null, string isActiveProperty = null) 
            : this(layout, spacing, padding, padding, padding, padding, childForceExpandWidth, childForceExpandHeight, childControlWidth, childControlHeight, childAlignment, isInteractableProperty, isActiveProperty)
        {
        }

        public HorizontalOrVerticalLayoutGroupAttribute(LayoutType layout, float spacing = 0, int paddingLeft = 0, int paddingRight = 0, int paddingTop =  0, int paddingBottom = 0, bool childForceExpandWidth = true, bool childForceExpandHeight = true, bool childControlWidth = true, bool childControlHeight = true, TextAnchor childAlignment = TextAnchor.UpperLeft, string isInteractableProperty = null, string isActiveProperty = null) : base(layout, isInteractableProperty, isActiveProperty)
        {
            Spacing = spacing;
            PaddingLeft = paddingLeft;
            PaddingRight = paddingRight;
            PaddingTop = paddingTop;
            PaddingBottom = paddingBottom;
            ChildForceExpandWidth = childForceExpandWidth;
            ChildForceExpandHeight = childForceExpandHeight;
            ChildControlWidth = childControlWidth;
            ChildControlHeight = childControlHeight;
            ChildAlignment = childAlignment;
        }

        public void CopyTo(HorizontalOrVerticalLayoutGroup layoutGroup)
        {
            layoutGroup.spacing = Spacing;
            layoutGroup.childForceExpandWidth = ChildForceExpandWidth;
            layoutGroup.childForceExpandHeight = ChildForceExpandHeight;
            layoutGroup.childControlWidth = ChildControlWidth;
            layoutGroup.childControlHeight = ChildControlHeight;
            layoutGroup.padding = new RectOffset(PaddingLeft, PaddingRight, PaddingTop, PaddingRight);
            layoutGroup.childAlignment = ChildAlignment;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class HorizontalLayoutGroupAttribute : HorizontalOrVerticalLayoutGroupAttribute
    {
        public HorizontalLayoutGroupAttribute(float spacing = 0, int padding = 0, bool childForceExpandWidth = true, bool childForceExpandHeight = true, bool childControlWidth = true, bool childControlHeight = true, TextAnchor childAlignment = TextAnchor.UpperLeft, string isInteractableProperty = null, string isActiveProperty = null)
            : base(LayoutType.Horizontal, spacing, padding,  childForceExpandWidth, childForceExpandHeight, childControlWidth, childControlHeight, childAlignment, isInteractableProperty, isActiveProperty)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class VerticalLayoutGroupAttribute : HorizontalOrVerticalLayoutGroupAttribute
    {
        public VerticalLayoutGroupAttribute(float spacing = 0, int padding = 0, bool childForceExpandWidth = true, bool childForceExpandHeight = true, bool childControlWidth = true, bool childControlHeight = true, TextAnchor childAlignment = TextAnchor.UpperLeft, string isInteractableProperty = null, string isActiveProperty = null)
            : base(LayoutType.Vertical, spacing, padding, childForceExpandWidth, childForceExpandHeight, childControlWidth, childControlHeight, childAlignment, isInteractableProperty, isActiveProperty)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public class LayoutAttribute : Attribute
    {
        public bool IgnoreLayout { get; private set; }
        public int LayoutPriority { get; private set; }
        public float FlexibleWidth { get; private set; }
        public float FlexibleHeight { get; private set; }
        public float PreferredHeight { get; private set; }
        public float PreferredWidth { get; private set; }
        public float MinHeight { get; private set; }
        public float MinWidth { get; private set; }        

        public LayoutAttribute(bool ignoreLayout = false, int layoutPriority = 0, float flexibleWidth = -1, float flexibleHeight = -1,  float preferredWidth = -1, float preferredHeight = -1,  float minWidth = -1, float minHeight = -1)
        {
            IgnoreLayout = ignoreLayout;
            LayoutPriority = layoutPriority;
            FlexibleWidth = flexibleWidth;
            FlexibleHeight = flexibleHeight;
            PreferredHeight = preferredHeight;
            PreferredWidth = preferredWidth;
            MinHeight = minHeight;
            MinWidth = minWidth;
        }

        public void CopyTo(LayoutElement layoutElement)
        {
            layoutElement.layoutPriority = LayoutPriority;
            layoutElement.flexibleWidth = FlexibleWidth;
            layoutElement.flexibleHeight = FlexibleHeight;
            layoutElement.preferredHeight = PreferredHeight;
            layoutElement.preferredWidth = PreferredWidth;
            layoutElement.minHeight = MinHeight;
            layoutElement.minWidth = MinWidth;
            layoutElement.ignoreLayout = IgnoreLayout;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ItemHorizontalLayoutGroupAttribute : HorizontalLayoutGroupAttribute
    {
        public ItemHorizontalLayoutGroupAttribute(float spacing = 0, int padding = 0, bool childForceExpandWidth = true, bool childForceExpandHeight = true, bool childControlWidth = true, bool childControlHeight = true)
           : base(spacing, padding, childForceExpandWidth, childForceExpandHeight, childControlWidth, childControlHeight)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ItemVerticalLayoutGroupAttribute : VerticalLayoutGroupAttribute
    {
        public ItemVerticalLayoutGroupAttribute(float spacing = 0, int padding = 0,  bool childForceExpandWidth = true, bool childForceExpandHeight = true, bool childControlWidth = true, bool childControlHeight = true, TextAnchor childAlignment = TextAnchor.UpperLeft, string isInteractableProperty = null, string isActiveProperty = null)
            : base(spacing, padding, childForceExpandWidth, childForceExpandHeight, childControlWidth, childControlHeight, childAlignment, isInteractableProperty, isActiveProperty)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ItemLayoutAttribute : LayoutAttribute
    {
        public ItemLayoutAttribute(bool ignoreLayout = false, int layoutPriority = 0, float flexibleWidth = -1, float flexibleHeight = -1,  float preferredWidth = -1, float preferredHeight = -1, float minWidth = -1, float minHeight = -1)
            : base(ignoreLayout, layoutPriority, flexibleWidth, flexibleHeight, preferredWidth, preferredHeight, minWidth, minHeight)
        {
        }
    }

    public class StyleAttribute : Attribute
    {
        public string ClassName { get; private set; }
        public string Color { get; private set; }
        public TextAlignmentOptions TextAlignment { get; private set; }

        public StyleAttribute(string className = null, string color = null, TextAlignmentOptions textAlignment = TextAlignmentOptions.MidlineLeft)
        {
            ClassName = className;
            Color = color;
            TextAlignment = textAlignment;
        }
    }

    public abstract class MemberAttribute : Attribute
    {
        public string Caption { get; private set; }
        public string IsActiveProperty { get; private set; }
        public bool InvertIsActiveProperty { get; private set; }

        public MemberAttribute(string caption = null, string isActiveProperty = null, bool invertIsActiveProperty = false)
        {
            Caption = caption;
            IsActiveProperty = isActiveProperty;
            InvertIsActiveProperty = invertIsActiveProperty;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ActionAttribute : MemberAttribute
    {
        public string IsInteractableProperty { get; private set; }

        public bool InvertIsInteractableProperty { get; private set; }
        
        public ActionAttribute(string caption = null, string isInteractableProperty = null, bool invertIsInteractableProperty = false, string isActiveProperty = null, bool invertIsActiveProperty = false) : base(caption, isActiveProperty, invertIsActiveProperty)
        {
            IsInteractableProperty = isInteractableProperty;
            InvertIsInteractableProperty = invertIsInteractableProperty;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyAttribute : MemberAttribute
    {
        public string IsInteractableProperty { get; private set; }
        public bool InvertIsInteractableProperty { get; private set; }
        public string IsReadonlyProperty { get; private set; }
        public bool InvertIsReadonlyProperty { get; private set; }

        public PropertyAttribute(string caption = null, string isInteractableProperty = null, bool invertIsInteractableProperty = false, string isActiveProperty = null, bool invertIsActiveProperty = false, string isReadonlyProperty = null, bool invertIsReadonlyProperty = false) : base(caption, isActiveProperty, invertIsActiveProperty)
        {
            IsInteractableProperty = isInteractableProperty;
            InvertIsInteractableProperty = invertIsInteractableProperty;
            IsReadonlyProperty = isReadonlyProperty;
            InvertIsReadonlyProperty = invertIsReadonlyProperty;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CollectionPropertyAttribute : PropertyAttribute
    {
        public string SelectedIndexProperty { get; private set; }

        public CollectionPropertyAttribute(string caption = null, string selectedIndexProperty = null, string isInteractableProperty = null, bool invertIsInteractableProperty = false, string isActiveProperty = null, bool invertIsActiveProperty = false, string isReadonlyProperty = null, bool invertIsReadonlyProperty = false)
            : base(caption, isInteractableProperty, invertIsInteractableProperty, isActiveProperty, invertIsActiveProperty, isReadonlyProperty, invertIsReadonlyProperty)
        {
            SelectedIndexProperty = selectedIndexProperty;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ProceduralAttribute : Attribute
    {
        public string BindingMethodName
        {
            get;
            private set;
        }
        public ProceduralAttribute(string bindingMethodName = null)
        {
            BindingMethodName = bindingMethodName;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class InjectAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PropertyChangedEventHandlerAttribute : Attribute
    {
        public Type Sender { get; private set; }
        public PropertyChangedEventHandlerAttribute(Type sender)
        {
            Sender = sender;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CollectionChangedEventHandlerAttribute : Attribute
    {
        public Type Sender { get; private set; }

        public CollectionChangedEventHandlerAttribute(Type sender)
        {
            Sender = sender;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DialogOkActionAttribute : Attribute
    {
        public string Caption
        {
            get;
            private set;
        }

        public DialogOkActionAttribute(string caption = null)
        {
            Caption = caption;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DialogAltActionAttribute : Attribute
    {
        public string Caption
        {
            get;
            private set;
        }

        public DialogAltActionAttribute(string caption = null)
        {
            Caption = caption;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DialogCancelActionAttribute : Attribute
    {
        public string Caption
        {
            get;
            private set;
        }

        public DialogCancelActionAttribute(string caption = null)
        {
            Caption = caption;
        }
    }

}
