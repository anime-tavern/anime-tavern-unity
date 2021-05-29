using UnityEditor;
using UnityEngine;

public class TileLocation
{
    public int x;
    public int z;
    public int layer;

    public TileLocation(int layer, int x, int z)
    {
        this.layer = layer;
        this.x = x;
        this.z = z;
    }

    public override bool Equals(object obj)
    {
        TileLocation otherLocation = obj as TileLocation;
        if (otherLocation == null)
        {
            return false;
        }

        return this.x == otherLocation.x && this.z == otherLocation.z && this.layer == otherLocation.layer;
    }

    public override int GetHashCode()
    {
        int rando = 1000000000;
        return this.x * (rando + 1) ^ 2 + this.z * (rando + 1) + this.layer;
    }
}