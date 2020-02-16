using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringExtensions
{
    public static bool ContainsInfo(this string value)
	{
		return !(string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value));
	}
}
