/// <summary>
/// Project : Easy Build System
/// Class : ReorderableList.cs
/// Namespace : EasyBuildSystem.Features.Editor.Extensions.ReorderableList
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;

namespace EasyBuildSystem.Features.Editor.Extensions.ReorderableList
{
	public class ReorderableList
	{
		public UnityEditorInternal.ReorderableList Native { get; private set; }

		protected bool UseFoldout { get; set; } = true;

		protected SerializedProperty SourceProperty { get; set; }

		protected SerializedProperty ElementPropertyAt(int index)
			=> SourceProperty.GetArrayElementAtIndex(index);

		protected string DisplayName
			=> SourceProperty.displayName;

		protected bool IsFoldedOut
		{
			get
			{
				return SourceProperty.isExpanded;
			}
			set
			{
				SourceProperty.isExpanded = value;
			}
		}

		public ReorderableList(
			SerializedProperty sourceProperty,
			bool useFoldout = true)
		{
            SourceProperty = sourceProperty ?? throw new System.ArgumentNullException("listProperty");
			UseFoldout = useFoldout;

			InitializeNative(sourceProperty);

			InitializeReadyMadeDrawers();
		}

		public ReorderableList(
			SerializedProperty sourceProperty,
			NativeFunctionOptions nativeOptions,
			ReadyMadeDrawerOptions drawerOptions,
			bool useFoldout = true)
		{
            SourceProperty = sourceProperty ?? throw new System.ArgumentNullException("listProperty");
			UseFoldout = useFoldout;

			InitializeNative(sourceProperty, nativeOptions);

			InitializeReadyMadeDrawers(drawerOptions);
		}

		public ReorderableList(
			SerializedProperty sourceProperty,
			NativeFunctionOptions nativeOptions,
			bool useFoldout = true)
		{
            SourceProperty = sourceProperty ?? throw new System.ArgumentNullException("listProperty");
			UseFoldout = useFoldout;

			InitializeNative(sourceProperty, nativeOptions);

			InitializeReadyMadeDrawers();
		}

		public ReorderableList(
			SerializedProperty sourceProperty,
			ReadyMadeDrawerOptions drawerOptions,
			bool useFoldout = true)
		{
            SourceProperty = sourceProperty ?? throw new System.ArgumentNullException("listProperty");
			UseFoldout = useFoldout;

			InitializeNative(sourceProperty);

			InitializeReadyMadeDrawers(drawerOptions);
		}

		protected virtual void InitializeNative(SerializedProperty listProperty)
		{
			InitializeNative(listProperty, NativeFunctionOptions.Default);
		}

		protected virtual void InitializeNative(SerializedProperty listProperty, NativeFunctionOptions options)
		{
			Native = new UnityEditorInternal.ReorderableList(
				listProperty.serializedObject,
				listProperty,
				options.Draggable,
				options.DisplayHeader,
				options.DisplayAddButton,
				options.DisplayRemoveButton
			);
		}

		public virtual void InitializeReadyMadeDrawers()
			=> InitializeReadyMadeDrawers(ReadyMadeDrawerOptions.Default);

		public virtual void InitializeReadyMadeDrawers(ReadyMadeDrawerOptions options)
		{
			if (options.UseReadyMadeHeader)
            {
                AddDrawHeaderCallback();
            }

            if (options.UseReadyMadeElement)
            {
                AddDrawElementPropertyCallback();
            }

            if (options.UseReadyMadeBackground)
            {
                AddDrawElementBackgroundCallback();
            }
        }

		public virtual void Layout()
		{
			if (Native == null)
            {
                return;
            }

            if (!UseFoldout)
            {
                Native.DoLayoutList();
            }
            else
            {
                LayoutWithFoldOut();
            }
        }

		protected virtual void LayoutWithFoldOut()
		{
			IsFoldedOut = EditorGUILayout.Foldout(IsFoldedOut, DisplayName);

			if (!IsFoldedOut)
            {
                return;
            }

            Native.DoLayoutList();
		}

		public virtual void AddDrawHeaderCallback()
		{
			if (Native == null)
            {
                return;
            }

            Native.drawHeaderCallback += DrawHeader;
		}

		public virtual void AddDrawHeaderCallback(string label)
		{
			if (Native == null)
            {
                return;
            }

            Native.drawHeaderCallback += (rect) => DrawHeader(rect, label);
		}

		protected virtual void DrawHeader(Rect rect)
		{
			EditorGUI.LabelField(rect, DisplayName);
		}

		protected virtual void DrawHeader(Rect rect, string label)
		{
			EditorGUI.LabelField(rect, label);
		}

		public void AddDrawElementPropertyCallback()
		{
			if (Native == null)
            {
                return;
            }

            Native.drawElementCallback += DrawProperty;
			Native.elementHeightCallback += ElementHeight;
		}

		protected virtual void DrawProperty(Rect rect, int index, bool isActive, bool isFocused)
			=> DrawProperty(rect, ElementPropertyAt(index));

		protected virtual void DrawProperty(Rect rect, SerializedProperty property)
		{
			if (property == null)
            {
                return;
            }

            EditorGUI.PropertyField(
				LayoutUtility.AdjustedRect(rect, property),
				property,
				true
			);
		}

		protected virtual float ElementHeight(int index)
			=> LayoutUtility.ElementHeight(ElementPropertyAt(index));

		public void AddDrawElementBackgroundCallback()
		{
			if (Native == null)
            {
                return;
            }

            Native.drawElementBackgroundCallback +=
				(Rect rect, int index, bool isActive, bool isFocused)
					=> DrawElementBackgroundAlternatively(rect, index, isActive, isFocused);
		}

		protected virtual void DrawElementBackgroundAlternatively(Rect rect, int index, bool isActive, bool isFocused)
		{
			if (isFocused)
			{
				DrawActiveColor(rect);
				return;
			}

			if (index % 2 != 0)
			{
				return;
			}

			DrawDifferentBackgroundColor(rect);
		}

		protected virtual void DrawActiveColor(Rect rect)
		{
			BackgroundUtility.DrawElementBackgroundColorActive(rect);
		}

		protected virtual void DrawDifferentBackgroundColor(Rect rect)
		{
			BackgroundUtility.DrawElementBackgroundColorDifferent(rect);
		}

		public virtual void AddDrawDropDownCallback(string[] canditateNames, System.Action<string> OnSelected)
		{
			if (Native == null)
				return;

			Native.onAddDropdownCallback += (rect, list)
				=> DrawDropDown(rect, canditateNames, OnSelected);
		}

		protected virtual void DrawDropDown(Rect rect, string[] canditateNames, System.Action<string> OnSelected)
		{
            GenericMenu menu = new GenericMenu();

			foreach (string name in canditateNames)
            {
                menu.AddItem(new GUIContent(name), false, () => OnSelected?.Invoke(name));
            }

            menu.DropDown(rect);
		}
	}
}
