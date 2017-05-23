using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Bars : MonoBehaviour
{
    [HideInInspector]
    public Player player;
    public GM gm;
    public RectTransform staminaImage, lifeImage, manaImage;

    private Vector3 staminaStartPosition, lifeStartPosition, manaStartPosition;
    private float staminaHeight, lifeHeight, manaHeight;
    // Use this for initialization
    void Start()
    {
        player = gm.player.GetComponent<Player>();
        staminaHeight = staminaImage.rect.height - 17; //that's the height, in pixels, of the diagonal line at the top of the bar. :/
        lifeHeight = lifeImage.rect.height;
        manaHeight = manaImage.rect.height - 17; //.... see above. sorry.
        staminaStartPosition = new Vector3(staminaImage.localPosition.x, staminaImage.localPosition.y + staminaHeight, staminaImage.localPosition.z);
        lifeStartPosition = new Vector3(lifeImage.localPosition.x, lifeImage.localPosition.y + lifeHeight, lifeImage.localPosition.z);
        manaStartPosition = new Vector3(manaImage.localPosition.x, manaImage.localPosition.y + manaHeight, manaImage.localPosition.z);
    }

    // Update is called once per frame
    void Update()
    {
        //Update stamina, life, mana bars
        staminaImage.localPosition = new Vector3(staminaStartPosition.x, staminaStartPosition.y - (int)(player.stamina / player.maxStamina * staminaHeight), staminaStartPosition.z);
        lifeImage.localPosition = new Vector3(lifeStartPosition.x, lifeStartPosition.y - (int)(player.life / player.maxLife * lifeHeight), lifeStartPosition.z);
        manaImage.localPosition = new Vector3(manaStartPosition.x, manaStartPosition.y - (int)(player.mana / player.maxMana * manaHeight), manaStartPosition.z);
    }
}
