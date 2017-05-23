using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHotkeys : MonoBehaviour
{
    Player player;
    Ability[] abilities;
    AbilityActivation abilityActivator;
    public static float lookRotationSpeed = 10f;
    // Use this for initialization
    void Start()
    {
        player = GetComponent<Player>();
        abilityActivator = GetComponent<AbilityActivation>();
    }

    // Update is called once per frame
    void Update()
    {
        //Set target
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.transform.position.z;
        player.target = Camera.main.ScreenToWorldPoint(mousePosition); //implicitly removes Z component
        if (player.dead)
            return;

        //If left click is pressed and no ability is pressed
        bool abilityButtonsHeld = false;

        abilities = GetComponents<Ability>();

        for (int i = 0; i < player.hotkeys.Length; i++)
            if (player.hotkeys[i] != -1)
            {
                if (Input.GetButtonDown("Hotkey " + i))
                    abilityActivator.abilityButtonPressed(abilities[player.hotkeys[i]]);
                if (Input.GetButton("Hotkey " + i))
                {
                    abilityActivator.abilityButtonHeld(abilities[player.hotkeys[i]]);
                    abilityButtonsHeld = true;
                }
                if (Input.GetButtonUp("Hotkey " + i))
                    abilityActivator.abilityButtonReleased(abilities[player.hotkeys[i]]);
            }

        if (!abilityButtonsHeld)
            if (Input.GetMouseButton(0))
            {
                //Rotate towards cursor while holding left click

                Quaternion toTarget = Quaternion.LookRotation(new Vector3(player.target.x - player.transform.position.x, player.target.y - player.transform.position.y, 0), new Vector3(0, 0, 1));
                player.transform.rotation = Quaternion.Lerp(player.transform.rotation, toTarget, lookRotationSpeed * Time.deltaTime);

            }
    }


}
