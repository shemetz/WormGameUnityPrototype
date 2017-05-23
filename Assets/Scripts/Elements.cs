using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains static methods for elements and damage types, plus the list of all elements.
/// <para>Remember: the first 12 elements (fire, water, ..., plant) are the "elemental" elements. They aren't the only elements!</para>
/// </summary>
public class Elements : MonoBehaviour
{
    public enum Element
    {
        None,
        Fire, Water, Wind, Electricity, Metal, Ice, Energy, Acid, Lava, Flesh, Earth, Plant, Sense, Strong, Regenerate, Flight, Dexterity, Armor, Movement,
        Teleport, Ghost, Force_Field, Time, Loop, Power, Steal, Audiovisual, Summon, Explosion, Control, Buff, Charge
    };

    public enum DamageType
    {
        Blunt, Sharp, Burn, Acid, Shock, Phantom, Error
    }


    /// <summary>
    /// 
    /// Returns the damage type of the element.
    /// <para>Water, Wind, Metal, Earth, Plant:   Blunt (Impact)</para>
    /// <para>Ice, Flesh:   Sharp (Stab)</para>
    /// <para>Fire, Lava:   Burn (Fire)</para>
    /// <para>Acid:   Acid</para>
    /// <para>Electricity, Energy, Force Field:   Shock</para>
    /// </summary>
    public static DamageType damageType(Element element)
    {
        switch (element)
        {
            case Element.Water:
            case Element.Wind:
            case Element.Metal:
            case Element.Earth:
            case Element.Plant:
                return DamageType.Blunt;
            case Element.Ice:
            case Element.Flesh:
                return DamageType.Sharp;
            case Element.Fire:
            case Element.Lava:
                return DamageType.Burn; // fire
            case Element.Acid:
                return DamageType.Acid; // acid
            case Element.Electricity:
            case Element.Energy:
            case Element.Force_Field:
                return DamageType.Shock; // shock
            default:
                Debug.LogError("5555: Unknown element damage type! " + element);
                return DamageType.Error;
        }
    }

    /// <summary>
    /// Turns an element name into the matching index, starting from 0.
    /// </summary>
    /// <param name="elementName">Element name, capitalized (e.g. "Water")</param>
    /// <returns></returns>
    public static int toInt(string elementName)
    {
        for (int i = 0; i < elementList.Length; i++)
            if (elementList[i].Equals(elementName))
                return i;
        return -1;
    }

    /// <summary>
    /// Enum to index.
    /// </summary>
    public static int toInt(Element element)
    {
        for (int i = 0; i < elementList.Length; i++)
            if (elementList[i].Equals(element.ToString().Replace('_', ' ')))
                return i;
        return -1;
    }

    /// <summary>
    /// Turns a hex representation of a color, e.g. "#4F8AEE" or "4F8AEE", into a Color object.
    /// <para>Will have an alpha channel if the string is long enough (e.g. "#4F8AEE29")</para>
    /// </summary>
    private static Color hexColor(string hex)
    {
        hex = hex.Replace("#", "");
        byte a = 255;//assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        //Only use alpha if the string has enough characters
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color(r, g, b, a);
    }

    /// <summary>
    /// List of all of the 32 elements. Note that "element" does not only include the first 12 elements (fire, water....plant), which are the "elemental elements".
    /// </summary>
    public static string[] elementList =
    { "Fire", "Water", "Wind", "Electricity", "Metal", "Ice", "Energy", "Acid", "Lava", "Flesh", "Earth", "Plant", "Sense", "Strong", "Regenerate", "Flight", "Dexterity", "Armor", "Movement",
            "Teleport", "Ghost", "Force Field", "Time", "Loop", "Power", "Steal", "Audiovisual", "Summon", "Explosion", "Control", "Buff", "Charge" };
    /// <summary>
    /// Colors that identify each element. Note that the Ghost element has some transparency to it.
    /// </summary>
    public static Color[] elementColors = new Color[]
    {
        hexColor("#FF6A00"), hexColor("#0094FF"), hexColor("#CDE8FF"), hexColor("#FFD800"), hexColor("#999999"), hexColor("#84FFFF"), hexColor("#E751FF"),
            hexColor("#A8A30D"), hexColor("#D32B00"), hexColor("#FF75AE"), hexColor("#8C2F14"), hexColor("#5DAE00"), hexColor("#91C6FF"), hexColor("#4F2472"),
            hexColor("#156B08"), hexColor("#D1CDFF"), hexColor("#00E493"), hexColor("#0800FF"), hexColor("#FFF9A8"), hexColor("#1ECAFF"), new Color(224, 224, 224, 120),
            hexColor("#C6FF7C"), hexColor("#A7C841"), hexColor("#6D6B08"), hexColor("#693F59"), hexColor("#404E74"), hexColor("#FFE2EC"), hexColor("#8131C6"),
            hexColor("#E57600"), hexColor("#FFC97F"), hexColor("#8FFFC2"), hexColor("#FF9F00") };
}
