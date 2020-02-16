using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorExtensions
{
	/// <summary>
	/// Generates a random fully saturated color
	/// </summary>
	/// <returns>A fully saturated color</returns>
	public static Color RandomSaturatedColor()
	{
		float[] rgb = new float[3];
		rgb[0] = UnityEngine.Random.Range(0.0f, 1.0f);  // red
		rgb[1] = UnityEngine.Random.Range(0.0f, 1.0f);  // green
		rgb[2] = UnityEngine.Random.Range(0.0f, 1.0f);  // blue

		// find max and min indexes.
		int max, min;

		if (rgb[0] > rgb[1])
		{
			max = (rgb[0] > rgb[2]) ? 0 : 2;
			min = (rgb[1] < rgb[2]) ? 1 : 2;
		}
		else
		{
			max = (rgb[1] > rgb[2]) ? 1 : 2;
			int notmax = 1 + max % 2;
			min = (rgb[0] < rgb[notmax]) ? 0 : notmax;
		}
		rgb[max] = 1;
		rgb[min] = 0;

		return new Color(rgb[0], rgb[1], rgb[2]);
	}
}
