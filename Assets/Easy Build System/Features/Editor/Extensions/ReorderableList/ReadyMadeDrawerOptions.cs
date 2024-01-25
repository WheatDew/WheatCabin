/// <summary>
/// Project : Easy Build System
/// Class : ReadyMadeDrawerOptions.cs
/// Namespace : EasyBuildSystem.Features.Editor.Extensions.ReorderableList
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;

namespace EasyBuildSystem.Features.Editor.Extensions.ReorderableList
{
	public struct ReadyMadeDrawerOptions : IEquatable<ReadyMadeDrawerOptions>
	{
		public bool UseReadyMadeHeader { get; }

		public bool UseReadyMadeElement { get; }

		public bool UseReadyMadeBackground { get; }

		public ReadyMadeDrawerOptions(
			bool useReadyMadeHeader,
			bool useReadyMadeElement = true,
			bool useReadyMadeBackground = true)
		{
			UseReadyMadeHeader = useReadyMadeHeader;
			UseReadyMadeElement = useReadyMadeElement;
			UseReadyMadeBackground = useReadyMadeBackground;
		}

		public static ReadyMadeDrawerOptions Default
			=> new ReadyMadeDrawerOptions(
				useReadyMadeHeader: true,
				useReadyMadeElement: true,
				useReadyMadeBackground: true
			);

		bool IEquatable<ReadyMadeDrawerOptions>.Equals(ReadyMadeDrawerOptions other)
		{
			if (UseReadyMadeHeader != other.UseReadyMadeHeader)
            {
                return false;
            }

            if (UseReadyMadeElement != other.UseReadyMadeElement)
            {
                return false;
            }

            if (UseReadyMadeBackground != other.UseReadyMadeBackground)
            {
                return false;
            }

            return true;
		}

		public override int GetHashCode()
		{
			var hashCode = -360803941;

			hashCode = hashCode * -1521134295 + UseReadyMadeHeader.GetHashCode();
			hashCode = hashCode * -1521134295 + UseReadyMadeElement.GetHashCode();
			hashCode = hashCode * -1521134295 + UseReadyMadeBackground.GetHashCode();

			return hashCode;
		}
	}
}
