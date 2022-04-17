using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InterpolationUtils
{
    /// <summary>
    /// Linearly map val from the range [fromStart, fromEnd] to the range [toStart, toEnd]
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    /// <param name="fromStart"></param>
    /// <param name="fromEnd"></param>
    /// <param name="toStart"></param>
    /// <param name="toEnd"></param>
    /// <returns></returns>
    //public static T Lmap<T>(T val, T fromStart, T fromEnd, T toStart, T toEnd)
    //{
    //    float tVal = (val - fromStart) / (fromEnd - fromStart);
    //    return ((1.0f - tVal) * toStart) + (toEnd * tVal);
    //}

    /// <summary>
    /// Linearly map val from the range [fromStart, fromEnd] to the range [toStart, toEnd]
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    /// <param name="fromStart"></param>
    /// <param name="fromEnd"></param>
    /// <param name="toStart"></param>
    /// <param name="toEnd"></param>
    /// <returns></returns>
    public static float Lmap(float val, float fromStart, float fromEnd, float toStart, float toEnd)
    {
        var tVal = (val - fromStart) / (fromEnd - fromStart);
        return ((1.0f - tVal) * toStart) + (toEnd * tVal);
    }

    /// <summary>
    /// Linearly map val from the range [fromStart, fromEnd] to the range [toStart, toEnd]
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    /// <param name="fromStart"></param>
    /// <param name="fromEnd"></param>
    /// <param name="toStart"></param>
    /// <param name="toEnd"></param>
    /// <returns></returns>
    public static double Lmap(double val, double fromStart, double fromEnd, double toStart, double toEnd)
    {
        var tVal = (val - fromStart) / (fromEnd - fromStart);
        return ((1.0 - tVal) * toStart) + (toEnd * tVal);
    }

    /// <summary>
    /// Linearly map val from the range [fromStart, fromEnd] to the range [toStart, toEnd]
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    /// <param name="fromStart"></param>
    /// <param name="fromEnd"></param>
    /// <param name="toStart"></param>
    /// <param name="toEnd"></param>
    /// <returns></returns>
    public static int Lmap(int val, int fromStart, int fromEnd, int toStart, int toEnd)
    {
        float tVal = (float)(val - fromStart) / (float)(fromEnd - fromStart);
        return (int)(((1.0f - tVal) * (float)toStart) + ((float)toEnd * tVal));
    }

    /// <summary>
    /// Easing function
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="tValue"></param>
    /// <returns></returns>
    //public static T EaseSinusoidal<T>(T a, T b, float tValue)
    //{
    //    float equation = (Mathf.Sin(tValue * 1.57079f));
    //    T ret = a + ((b - a) * equation);
    //    return ret;
    //}
}
