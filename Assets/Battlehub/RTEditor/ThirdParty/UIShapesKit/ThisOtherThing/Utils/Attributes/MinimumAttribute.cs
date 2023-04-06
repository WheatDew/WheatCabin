using UnityEngine;

namespace ThisOtherThing.Utils
{
	public class MinimumAttribute : PropertyAttribute
	{
		public readonly float minFloat;
		public readonly int minInt;

		public MinimumAttribute(float min)
		{
			this.minFloat = min;
		}

		public MinimumAttribute(int min)
		{
			this.minInt = min;
		}
	}
}