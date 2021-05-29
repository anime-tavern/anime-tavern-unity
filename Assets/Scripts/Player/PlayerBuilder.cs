using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBuilder
{

    public static TileLocation TILE_DEFAULT_SPAWN = new TileLocation(0, 80, -154);

    public static GameObject getBasePlayerObject()
    {
        GameObject basePlayer = MonoBehaviour.Instantiate((GameObject)Resources.Load("Player/BasePlayer"));
        return basePlayer;
    }
}
