using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityActivation : MonoBehaviour
{

    Person user;
    private void Start()
    {
        user = GetComponent<Person>();
    }
    private void Update()
    {

    }
    public void abilityButtonPressed(Ability ability)
    {//TODO make sure there isn't a bug if the player sets two keys to the same ability and then presses one while the other is held
        if (ability.maintainable)
        {
            if (!user.maintaining)
                ability.activate();
        }
        else if (ability.instant)
            ability.activate();
        else //not instant
            return;
    }

    public void abilityButtonHeld(Ability ability)
    {
        if (ability.instant)
            ability.activate();
    }

    public void abilityButtonReleased(Ability ability)
    {
        if (ability.maintainable)
        {
            if (ability.on)
                ability.activate();
            else //ability button was released, but ability is off
                return;
        }
        else if (ability.instant) //ability button was released, but ability is instant
            return;
        else //not instant
            ability.activate();
    }
}
