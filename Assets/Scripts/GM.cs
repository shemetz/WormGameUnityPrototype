using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM : MonoBehaviour
{
    [HideInInspector]
    public GameObject player;
    public static float gravity = 9.8f;
    [HideInInspector]
    public Environment env;
    void Awake()
    {
        Ability.initializeDescriptions();
        Block.initializeStrings();
        
        //Create player
        player = new GameObject();
        player.name = "The Player";
        player.AddComponent<Player>();
        player.AddComponent<PlayerHotkeys>();
        Camera.main.gameObject.AddComponent<CameraFollow>().target = player.transform;
        player.transform.position = new Vector3(3, 3);
        
        //Create environment
        env = new GameObject().AddComponent<Environment>();
        env.name = "Environment";
        env.init();
        
        Cursor.SetCursor(Resources.Load("temp/Cursor") as Texture2D, new Vector2(1, 1), CursorMode.ForceSoftware);
    }
}
