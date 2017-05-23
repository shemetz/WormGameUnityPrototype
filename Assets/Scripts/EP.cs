using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// an EP is an element-points pair. For example: "Fire 4" or "Audiovisual 9". 
/// 
/// The elementNum is the index of the element, and the points are the amount of
/// points (between 1 and 10, inclusive).
/// </summary>
public class EP : MonoBehaviour
{
    /// <summary>
    /// index of the element of this EP
    public int elementNum;
    /// <summary>
    /// number of points in this EP
    /// </summary>
    public int points;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e">element index, e.g. 10 for Earth</param>
    /// <param name="p">number of points / level</param>
    public EP(int e, int p)
    {
        elementNum = e;
        points = p;
    }

    /// <summary>
    /// </summary>
    /// <param name="e">element name, e.g. "Strong"</param>
    /// <param name="p">number of points / level</param>
    public EP(string e, int p)
    {
        elementNum = Elements.toInt(e);
        points = p;
    }

    /// <summary>
    /// This other version of toString is in uppercase if it's a main element, and lowercase if it's minor. Easier to read that way, for me.
    /// </summary>
    /// <returns>string describing this EP: NAME points or name points</returns>
    public string toString2()
    {
        if (points < 4)
            return "" + Elements.elementList[elementNum].ToLower() + " " + points;
        return "" + Elements.elementList[elementNum].ToUpper() + " " + points;
    }

    /// <summary>
    /// Returns a string describing this EP, in the format of "Fire 7" for example.
    public string toString()
    {
        return "" + Elements.elementList[elementNum] + " " + points;
    }
}
