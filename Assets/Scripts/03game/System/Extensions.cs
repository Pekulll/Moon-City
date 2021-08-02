using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static string SignedString(this float value, string format = "0.0")
    {
        return (value >= 0) ? $"+{value.ToString(format)}" : value.ToString(format);
    }
    
    public static string SignedString(this int value, string format = "0")
    {
        return (value >= 0) ? $"+{value.ToString(format)}" : value.ToString(format);
    }

    public static string Join<T>(this string value, List<T> iterable)
    {
        string result = "";

        foreach (T t in iterable)
        {
            result += t + value;
        }

        return result;
    }
    
    public static string Join<T>(this string value, T[] iterable)
    {
        string result = "";

        foreach (T t in iterable)
        {
            result += t + value;
        }

        return result;
    }
}
