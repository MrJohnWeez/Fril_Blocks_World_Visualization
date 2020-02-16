using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayExtensions
{
	/// <summary>
	/// Prints out any given array
	/// </summary>
	/// <typeparam name="T">type</typeparam>
	/// <param name="value">Array to print out</param>
	public static string ArrayToString<T>(this T[] value)
	{
		// TODO: Use string builder for this
		string longString = "";
		if (value != null && value.Length > 0)
		{
			foreach (T typeValue in value)
			{
				longString += typeValue.ToString();
			}
		}
		return longString;
	}
}
