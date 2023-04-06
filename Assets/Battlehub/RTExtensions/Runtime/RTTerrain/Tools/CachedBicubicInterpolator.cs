using UnityEngine;
using Unity.Mathematics;

namespace Battlehub.RTTerrain
{
	public class CachedBicubicInterpolator 
	{
		public float4x4 m;

		public void UpdateCoefficients(float[,] p) // 4x4
		{
			m = new float4x4(
				// 1
				p[1, 1],
				.5f * (-p[1, 0] + p[1, 2]),
				p[1, 0] - 2.5f * p[1, 1] + 2f * p[1, 2] - .5f * p[1, 3],
				-.5f * p[1, 0] + 1.5f * p[1, 1] - 1.5f * p[1, 2] + .5f * p[1, 3],

				// 2
				.5f * (-p[0, 1] + p[2, 1]),

				.25f * (p[0, 0] - p[0, 2] - p[2, 0] + p[2, 2]),

				-.5f * p[0, 0] + 1.25f * p[0, 1] - p[0, 2] + .25f * p[0, 3]
				+ .5f * p[2, 0] - 1.25f * p[2, 1] + p[2, 2] - .25f * p[2, 3],

				.25f * p[0, 0] - .75f * p[0, 1] + .75f * p[0, 2] - .25f * p[0, 3]
				- .25f * p[2, 0] + .75f * p[2, 1] - .75f * p[2, 2] + .25f * p[2, 3],

				// 3
				p[0, 1] - 2.5f * p[1, 1] + 2 * p[2, 1] - .5f * p[3, 1],

				-.5f * p[0, 0] + .5f * p[0, 2]
				+ 1.25f * p[1, 0] - 1.25f * p[1, 2]
				- p[2, 0] + p[2, 2]
				+ .25f * p[3, 0] - .25f * p[3, 2],

				p[0, 0] - 2.5f * p[0, 1] + 2 * p[0, 2] - .5f * p[0, 3]
				- 2.5f * p[1, 0] + 6.25f * p[1, 1] - 5 * p[1, 2] + 1.25f * p[1, 3]
				+ 2 * p[2, 0] - 5 * p[2, 1] + 4 * p[2, 2] - p[2, 3]
				- .5f * p[3, 0] + 1.25f * p[3, 1] - p[3, 2] + .25f * p[3, 3],

				-.5f * p[0, 0] + 1.5f * p[0, 1] - 1.5f * p[0, 2] + .5f * p[0, 3]
				+ 1.25f * p[1, 0] - 3.75f * p[1, 1] + 3.75f * p[1, 2] - 1.25f * p[1, 3]
				- p[2, 0] + 3 * p[2, 1] - 3 * p[2, 2] + p[2, 3]
				+ .25f * p[3, 0] - .75f * p[3, 1] + .75f * p[3, 2] - .25f * p[3, 3],

				// 4
				-.5f * p[0, 1] + 1.5f * p[1, 1]
				- 1.5f * p[2, 1] + .5f * p[3, 1],

				.25f * p[0, 0] - .25f * p[0, 2]
				- .75f * p[1, 0] + .75f * p[1, 2]
				+ .75f * p[2, 0] - .75f * p[2, 2]
				- .25f * p[3, 0] + .25f * p[3, 2],

				-.5f * p[0, 0] + 1.25f * p[0, 1] - p[0, 2] + .25f * p[0, 3]
				+ 1.5f * p[1, 0] - 3.75f * p[1, 1] + 3 * p[1, 2] - .75f * p[1, 3]
				- 1.5f * p[2, 0] + 3.75f * p[2, 1] - 3 * p[2, 2] + .75f * p[2, 3]
				+ .5f * p[3, 0] - 1.25f * p[3, 1] + p[3, 2] - .25f * p[3, 3],

				.25f * p[0, 0] - .75f * p[0, 1] + .75f * p[0, 2] - .25f * p[0, 3]
				- .75f * p[1, 0] + 2.25f * p[1, 1] - 2.25f * p[1, 2] + .75f * p[1, 3]
				+ .75f * p[2, 0] - 2.25f * p[2, 1] + 2.25f * p[2, 2] - .75f * p[2, 3]
				- .25f * p[3, 0] + .75f * p[3, 1] - .75f * p[3, 2] + .25f * p[3, 3]);
		}

		public void UpdateCoefficients(float4x4 p) // 4x4
		{
			m = new float4x4(
				p.c1.y,
				0.5f * (p.c1.z - p.c1.x),
				math.dot(p.c1, new float4(1.0f, -2.5f, 2.0f, -0.5f)),
				math.dot(p.c1, new float4(-0.5f, 1.5f, -1.5f, 0.5f)),

				// 2
				0.5f * (p.c2.y - p.c0.y),
				0.25f * (p.c0.x - p.c0.z - p.c2.x + p.c2.z),
				math.dot(p.c2 - p.c0, new float4(0.5f, -1.25f, 1.0f, -0.25f)),
				math.dot(p.c0 - p.c2, new float4(0.25f, -0.75f, 0.75f, -0.25f)),

				// 3
				p.c0.y - 2.5f * p.c1.y + 2.0f * p.c2.y - 0.5f * p.c3.y,

				0.5f * (p.c0.z - p.c0.x) + 
				-1.25f * (p.c1.z - p.c1.x) +
				1.0f * (p.c2.z - p.c2.x) + 
				-0.25f * (p.c3.z - p.c3.x),

				math.dot(p.c0, new float4(1.0f, -2.5f, 2.0f, -0.5f)) +
				math.dot(p.c1, new float4(-2.5f, 6.25f, -5.0f, 1.25f)) +
				math.dot(p.c2, new float4(2.0f, -5.0f, 4.0f, -1.0f)) +
				math.dot(p.c3, new float4(-0.5f, 1.25f, -1.0f, 0.25f)),

				math.dot(p.c0, new float4(-0.5f, 1.5f, -1.5f, 0.5f)) +
				math.dot(p.c1, new float4(1.25f, -3.75f, 3.75f, -1.25f)) +
				math.dot(p.c2, new float4(-1.0f, 3.0f, -3.0f, 1.0f)) +
				math.dot(p.c3, new float4(0.25f, -0.75f, 0.75f, -0.25f)),

				// 4
				0.5f * (p.c3.y - p.c0.y) + 1.5f * (p.c1.y - p.c2.y),

				0.25f * (p.c0.x - p.c0.z - p.c3.x + p.c3.z) -
				0.75f * (p.c1.x - p.c1.z - p.c2.x + p.c2.z),

				math.dot(p.c1 - p.c2, new float4(1.5f, -3.75f, 3.0f, -0.75f)) +
				math.dot(p.c3 - p.c0, new float4(0.5f, -1.25f, 1.0f, -0.25f)),

				math.dot(p.c0 - p.c3, new float4(0.25f, -0.75f, 0.75f, -0.25f)) +
				math.dot(p.c2 - p.c1, new float4(0.75f, -2.25f, 2.25f, -0.75f)));
		}

		public float GetValue(float x, float y)
		{
			float x2 = x * x;
			float y2 = y * y;

			float4 vx = new float4(1.0f, x, x2, x2 * x);
			float4 vy = new float4(1.0f, y, y2, y2 * y);

			return math.dot(math.mul(m, vy), vx);
		}
	}
}
