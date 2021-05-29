using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileTracker
{

    public Tile tile;
    public TileTracker parentTracker;
    public float gScore;
    public float hScore;
    public float fScore;

    public TileTracker(Tile tile, TileTracker parentTracker = null)
    {
        this.tile = tile;
        this.parentTracker = parentTracker;
    }
}
