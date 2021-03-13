using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static string SignedString(this float value, string format = "0")
    {
        return (value >= 0) ? $"+{value.ToString(format)}" : value.ToString(format);
    }
    
    public static string SignedString(this int value, string format = "0")
    {
        return (value >= 0) ? $"+{value.ToString(format)}" : value.ToString(format);
    }
}
