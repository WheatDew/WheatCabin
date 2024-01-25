/// <summary>
/// Project : Easy Build System
/// Class : NativeFunctionOptions.cs
/// Namespace : EasyBuildSystem.Features.Editor.Extensions.ReorderableList
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;

namespace EasyBuildSystem.Features.Editor.Extensions.ReorderableList
{
	public struct NativeFunctionOptions : IEquatable<NativeFunctionOptions>
	{
		public bool Draggable { get; }

		public bool DisplayHeader { get; }

		public bool DisplayAddButton { get; }

		public bool DisplayRemoveButton { get; }

		public NativeFunctionOptions(
			bool draggable,
			bool displayHeader = true,
			bool displayAddButton = true,
			bool displayRemoveButton = true)
		{
			Draggable = draggable;
			DisplayHeader = displayHeader;
			DisplayAddButton = displayAddButton;
			DisplayRemoveButton = displayRemoveButton;
		}

		public static NativeFunctionOptions Default
			=> new NativeFunctionOptions(
				draggable: true,
				displayHeader: true,
				displayAddButton: true,
				displayRemoveButton: true
			);


		bool IEquatable<NativeFunctionOptions>.Equals(NativeFunctionOptions other)
		{
			if (Draggable != other.Draggable)
            {
                return false;
            }

            if (DisplayHeader != other.DisplayHeader)
            {
                return false;
            }

            if (DisplayAddButton != other.DisplayAddButton)
            {
                return false;
            }

            if (DisplayRemoveButton != other.DisplayRemoveButton)
            {
                return false;
            }

            return true;
		}

		public override int GetHashCode()
		{
            int hashCode = 1643622589;

			hashCode = hashCode * -1521134295 + Draggable.GetHashCode();
			hashCode = hashCode * -1521134295 + DisplayHeader.GetHashCode();
			hashCode = hashCode * -1521134295 + DisplayAddButton.GetHashCode();
			hashCode = hashCode * -1521134295 + DisplayRemoveButton.GetHashCode();

			return hashCode;
		}
	}
}

