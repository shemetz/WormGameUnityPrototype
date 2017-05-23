using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Methods;

public class Ability : MonoBehaviour
{

    /// <summary>
    /// List of all abilities that have been programmed into the game. No abilities should be created if they don't belong to this list!
    /// </summary>
    public static List<string> implementedAbilities = new List<string>(new string[] { });
    protected static List<string> descriptions = new List<string>();
    protected static bool[,] elementalAttacksPossible = new bool[12, 7]; // [element][ability]
    protected static int[,] elementalAttackNumbers = new int[12, 3];
    protected static string[] elementalAttacks = new string[]
    { "Ball", "Beam", "Shield", "Wall", "Spray", "Strike", "Pool" };
    public static List<string> elementalPowers = new List<string>(new string[] {"Elemental Combat I", "Elemental Combat II", "Ball", "Beam", "Shield", "Wall", "Spray", "Strike", "Pool", "Sense Element",
           "Elemental Resistance", "Elemental Fists", "Trail", "Elemental Armor I", "Elemental Armor II", "Elemental Armor III" });
    public static string[] elementalPowersWithTheirOwnImages = new string[]
   { "Elemental Combat I", "Elemental Combat II", "Trail" };

    public enum CostType
    {
        NONE, MANA, STAMINA, CHARGE, LIFE
    }; // Abilities that use multiple don't exist, I think.

    public enum RangeType
    {
        CREATE_IN_GRID, EXACT_RANGE, CIRCLE_AREA, CONE, NONE, EXPLOSION
    };

    // permanent variables of the ability:

    /// <summary>
    /// Name of the ABILITY. not PERSON. Unity thinks "name" belongs to the object( the person), so use abilityName to refer to the ability's name.
    /// </summary>
    public string abilityName;
    /// <summary>
    /// Measures how powerful the ability is.
    /// <para> 0-10, 0 only for special abilities.</para>
    /// </summary>
    public int level;
    /// <summary>
    /// Instant abilities activate when the button is pressed.
    /// </summary>
    public bool instant;
    /// <summary>
    /// True if the ability can be toggled on/off (an "on-off" ability). Toggleable abilities are always treated as instant.
    /// </summary>
    public bool toggleable;
    /// <summary>
    /// True: The ability is maintainable, which means it continuously functions from the moment you press the button till the moment you release it. 
    /// Usually, while a user is "maintaining" an ability, they can't activate any other abilities. Also, maintainable abilities usually have a costPerSecond variable.
    /// </summary>
    public bool maintainable;
    /// <summary>
    /// True: If the power is maintainable, the user is unable to move while maintaining it.
    /// </summary>
    public bool stopsMovement;
    /// <summary>
    /// Determines what resource (mana, stamina, charge, life, or none) is drained when using the ability.
    /// </summary>
    public CostType costType;
    /// <summary>
    /// True if the ability is natural. Natural abilities can't be disabled (they include stuff like Punch, Sprint...)
    /// </summary>
    public bool natural;

    //NOTE: Any perks/abilities that change the stats of an ability, always MULTIPLY them. This makes order-of-operations meaningless.
    //Exception: the "level" stat is never multiplied, only increased/decreased by finite amounts.
    // Stats: Stuff that affects the ability's effectiveness and is shown in the Abilities menu

    /// <summary>
    /// Distance, in meters, from the user in which the ability works. This can represent several things. 
    /// For projectile abilities, it is the distance the projectiles go before disappearing.
    /// For aura abilities, it is the maximum distance in which things are affected.
    /// For targeted abilities, it is the maximum distance to a possible target (either an object or a place).
    /// For abilities without any concept of range, range == -1.
    /// </summary>
    public float range;
    /// <summary>
    /// Duration, in seconds, in which the ability is "on cooldown", i.e. can't be activated and doesn't do anything.
    /// </summary>
    public float cooldown;
    /// <summary>
    /// Cost of the ability per activation (the type of units is determined by costType).
    /// </summary>
    public float cost;
    /// <summary>
    /// Cost per second of the ability. Applies to maintained abilities, and to some toggleable abilities.
    /// </summary>
    public float costPerSecond;
    /// <summary>
    /// Radius of the area of effect, in meters. Note that radius usually exists only if the range variable means something else for that ability.
    /// So, for example, passive aura abilities use raneg and not radius.
    /// </summary>
    public float radius;
    /// <summary>
    /// Damage of the ability. If the ability deals damage continuously, this is the damage per second.
    /// </summary>
    public float damage;
    /// <summary>
    /// Pushback of the ability. If the ability deals pushback continuously, this is the damage per second.
    /// 
    /// <para>The units of "pushback" are very mysterious and prefer not to be bothered with.</para>
    /// </summary>
    public float pushback;
    /// <summary>
    /// Amount of "steal" that the ability has. TODO figure this out
    /// <para>Between 0 and 1.</para>
    /// </summary>
    public float steal;
    /// <summary>
    /// Duration of the effect of the ability, in seconds.
    /// </summary>
    public float duration;
    /// <summary>
    /// Chance that something will happen. For example, chance of freezing the target for Ice Strike.
    /// <para>Between 0 and 1.</para>
    /// </summary>
    public float chance;
    /// <summary>
    /// Amount of....something. This is the popular value to determine strength of float-changing abilities.
    /// </summary>
    public float amount;
    /// <summary>
    /// Amount of charge that is gained per second while the user appeases the ability's needs.
    /// 
    /// <para>Usually equal to 5.</para>
    /// </summary>
    public float chargeRate;
    /// <summary>
    /// Some abilities have an arc length (for example, the Spray ability).
    /// </summary>
    public float arc;
    /// <summary>
    /// ...Some abilities have a vertical range, I guess. TODO look if this is needed
    /// </summary>
    public float verticalRange;

    // Changing variables of the ability

    /// <summary>
    /// How much time is left, for some function of the ability.
    /// </summary>
    public float timeLeft;
    /// <summary>
    /// Time left until the ability is off cooldown.
    /// <para>If the ability is passive, a cooldownLeft value of 0 means that it has yet to be initialized.</para>
    /// </summary>
    public float cooldownLeft;
    /// <summary>
    /// True/False whether or not the ability is on/off. Relevant only for maintained and toggleable abilities.
    /// </summary>
    public bool on;
    /// <summary>
    /// Element (Fire, Water, Ice...) that is related to the ability.
    /// </summary>
    public Elements.Element element;
    /// <summary>
    /// Disabled abilities can't be used until they are enabled again. Abilities are usually only disabled temporarily and by other abilities.
    /// <para>Don't change this value directly - use disable() instead, since that lets the ability undo its changes on the user/targets/world before it stops working.</para>
    /// </summary>
    public bool disabled;
    /// <summary>
    /// Set this to true when you want to disable the ability in the next frame.
    /// </summary>
    public bool prepareToDisable;
    /// <summary>
    /// Set this to true when you want to enable (un-disable) the ability in the next frame.
    /// </summary>
    public bool prepareToEnable;

    /// <summary>
    /// Not the Unity tag!
    /// <para>This is a constant array of tags that describe the ability. For example: offensive, projectile, damaging...</para>
    /// </summary>
    protected string[] tags;


    /// <summary>
    /// Used for special effects.
    /// </summary>
    protected RangeType rangeType;
    /// <summary>
    /// Used for weird drawings. TODO make sure it's necessary and can't belong only to child classes.
    /// </summary>
    protected int frameNum = 0;

    /// <summary>
    /// The user of the ability, the one who has it.
    /// </summary>
    public Person user;




    private void Update()
    {
        if (on)
            maintain();
    }

    /// <summary>
    /// Feel free to override this! This function is called ONLY the ability is initialized. Sets the ability's variables to be what they need to be.
    /// <para>Use this to update variables such as range, cooldown, damage, etc. but not maintainable, costType, etc.</para> 
    /// </summary>
    public virtual void initStats()
    {

    }

    /// <summary>
    /// Feel free to override this! This function should not be called by you - instead, set prepareToDisable to true.
    /// If you want to disable something immediately, use disableImmediately().
    /// <para>This method should undo whatever the ability did or is doing: stop maintenance, turn off, remove buffs/nerfs of a passive ability, etc. </para>
    /// </summary>
    public virtual void disable()
    {

    }

    /// <summary>
    /// Feel free to override this! This method is called when a user activates an ability, which means when pressing the button for instant abilities (including maintained and toggleable ones),
    /// when releasing a button (for targeted/aimed abilities), or simply when the ability starts existing (for passive abilities).
    /// </summary>
    public virtual void activate()
    {

    }

    /// <summary>
    /// Feel free to override this! This method is called once every frame if the ability is on (which is relevant only for maintained and toggleable abilities).
    /// Note that the button isn't necessarily held - some toggleable abilities have a maintain while they're on/off, regardless.
    /// </summary>
    public virtual void maintain()
    {

    }

    /// <summary>
    /// Feel free to override this! This method is called for the player only, and is for visualizing stuff.
    /// </summary>
    /// <param name="player"></param>
    public virtual void updatePlayerTargeting(Player player)
    {

    }

    /// <summary>
    /// Disables the ability immediately. Calls disable() and then sets disabled=true.
    /// </summary>
    public virtual void disableImmediately()
    {
        //Hopefully no bugs happen here
        disable();
        disabled = true;
    }

    /// <summary>
    /// Always call base.Awake() when overriding this! 
    /// <para>This calls updatefloats() at its end. Remember that.</para>
    /// </summary>
    public virtual void Awake()
    {
        user = gameObject.GetComponent<Person>();
        abilityName = this.GetType().ToString();
        element = (Elements.Element)System.Enum.Parse(typeof(Elements.Element), getElementName());

        // default values.
        range = -1;
        cooldown = -1;
        cost = -1;
        costPerSecond = -1;
        radius = -1;
        damage = -1;
        pushback = -1;
        steal = -1;
        duration = -1;
        chance = -1;
        amount = -1;
        chargeRate = -1;

        verticalRange = 4; // default value.

        cooldownLeft = 0;
        instant = false;
        maintainable = false;
        stopsMovement = false;
        toggleable = false;
        on = false;
        costType = CostType.NONE;
        rangeType = RangeType.NONE;
        timeLeft = 0;
        disabled = false;
        prepareToDisable = false;
        prepareToEnable = false;
        natural = false;

        updateTags();
        initStats();
    }

    /// <summary>
    /// Is called when the ability initializes, and updates the tags variable according to the descriptions variable.
    /// </summary>
    private void updateTags()
    {
        string tagList = getDescription(abilityName);
        if (tagList.IndexOf('\n') == -1)
            Debug.LogError("ability class go to this line and solve this. abilityName was " + abilityName + " and text was: " + tagList);
        // skip first line, delete fluff and description
        // tagList is a string like "dangerous fire symbolic" which is parsed and will create a tag array of {"dangerous", "fire", "symbolic"}
        // make sure not to include double spaces!
        tagList = tagList.substring(tagList.IndexOf("\n") + 1, tagList.IndexOf("\n", tagList.IndexOf("\n") + 1));
        if (tagList.Length < 1)
        {
            tags = new string[0];
            return;
        }
        List<string> tags2 = new List<string>();
        string currTag = "";
        for (int i = 0; i < tagList.Length; i++)
        {
            if (tagList[i] != ' ')
                currTag += tagList[i];
            else
            {
                tags2.Add(currTag);
                currTag = "";
            }
        }
        tags2.Add(currTag);
        tags = new string[tags2.Count];
        for (int i = 0; i < tags2.Count; i++)
            tags[i] = tags2[i];
        return;
    }

    /// <summary>
    /// Returns the ability's name without any element (e.g. "Ball" instead of "Ball &lt;Fire&gt;")
    /// </summary>
    public string justName()
    {
        if (abilityName.Contains("<"))
            return abilityName.substring(0, abilityName.IndexOf('<') - 1);
        return abilityName;
    }

    /// <summary>
    /// Returns name of element. "None" if there's none.
    /// </summary>
    /// <returns></returns>
    public string getElementName()
    {
        if (abilityName.Contains("<"))
            return abilityName.substring(abilityName.IndexOf("<") + 1, abilityName.IndexOf(">"));
        return "None";
    }

    /// <summary>
    /// Returns number of element. -1 if there's none.
    /// </summary>
    /// <returns></returns>
    private int getElementNum()
    {
        if (abilityName.Contains("<"))
            return Elements.toInt(abilityName.substring(abilityName.IndexOf("<") + 1, abilityName.IndexOf(">") - abilityName.IndexOf("<") + 1));
        return -1;
    }

    /// <summary>
    /// Gives the description of an ability that is named in the following format:
    /// <para>Punch</para>
    /// or:
    /// <para>Ball &lt;Fire&gt;</para>
    public static string getDescription(string abilityName)
    {
        // name must not contain any numbers or elements
        // this method's returned string contains <E>
        if (abilityName.Contains("<"))
        {
            for (int i = 0; i < descriptions.Count; i++)
                if (descriptions[i].substring(0, descriptions[i].IndexOf('(') - 1).Equals(abilityName.substring(0, abilityName.IndexOf("<") - 1)))
                    return descriptions[i];
        }
        else
            for (int i = 0; i < descriptions.Count; i++)
                if (descriptions[i].substring(0, descriptions[i].IndexOf('(') - 1).Equals(abilityName))
                    return descriptions[i];
        return "String not found in abilities: \"" + abilityName + "\"";
    }

    public string getDescription()
    {
        return Ability.getDescription(abilityName);
    }

    /// <summary>
    /// Returns true of any of the tags match the given string.
    /// </summary>
    /// <param name="tag">lowercase string, e.g. "projectile".</param>
    /// <returns></returns>
    public bool hasTag(string tag)
    {
        for (int i = 0; i < tags.Length; i++)
            if (tags[i].Equals(tag))
                return true;
        return false;
    }

    /// <summary>
    /// Returns a single string with all tags, separated by spaces.
    public string getTags()
    {
        string s = "";
        for (int i = 0; i < tags.Length; i++)
            s += " " + tags[i];
        return s.substring(1);
    }

    /// <summary>
    /// Initializes the <see cref="descriptions"/> list.
    /// </summary>
    public static void initializeDescriptions()
    {
        TextAsset abilitiesTXT = Resources.Load("Texts/abilities") as TextAsset;
        string[] text = abilitiesTXT.text.Split('\n');
        int i = 0;
        // While there's lines left in the text file, do this:
        do
        {
            string s = "";
            string line = text[i];
            while (line != null && !line.Equals("") && !line.Equals("\r") && !line.Equals("\n"))
            {
                s += line + "\n";
                if (i + 1 >= text.Length)
                    break;
                i++;
                line = text[i];
            }
            s = s.substring(0, s.Length - 1); // remove final paragraph break
            descriptions.Add(s);
            while (text[i].Length <= 1 && i < text.Length)
                i++;
        }
        while (i < text.Length);
        Debug.Assert(descriptions.Count > 17);

        TextAsset elementalCombatCSV = Resources.Load("Texts/elementalCombatPossibilities") as TextAsset;
        text = elementalCombatCSV.text.Split('\n');
        i = 0;
        // While there's lines left in the text file, do this:
        do
        {
            string line = text[i];
            if (line != null)
            {
                int j = 0;
                for (int k = 0; k < line.Length; k++)
                    switch (line[k])
                    {
                        case 'X':
                            elementalAttacksPossible[i, j] = true;
                            j++;
                            break;
                        case 'O':
                            elementalAttacksPossible[i, j] = false;
                            j++;
                            break;
                        case ',':
                            break;
                        default: // Damage and stuff
                            int result;
                            bool canParse = int.TryParse("" + line[k], out result);
                            if (canParse)
                                elementalAttackNumbers[i, j - 7] = result;
                            else
                                break; //End-line character, probably
                            j++;
                            break;
                    }
            }
            i++;
        }
        while (i < text.Length);

    }
}
