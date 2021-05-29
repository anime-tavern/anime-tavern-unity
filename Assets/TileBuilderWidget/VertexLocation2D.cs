using UnityEditor;
using UnityEngine;

public class VertexLocation2D
{
    public const int ROUND_PRECISION = 10;

    public float x;
    public float z;
    public int layer;

    public VertexLocation2D(int layer, float x, float z)
    {
        this.layer = layer;
        this.x = x;
        this.z = z;
    }

    public override bool Equals(object obj)
    {
        VertexLocation2D otherLocation = obj as VertexLocation2D;
        if (otherLocation == null)
        {
            return false;
        }

        // Round the X,Y, and Z values to a precision
        int x1_rounded = (int)(this.x * VertexLocation2D.ROUND_PRECISION);
        int z1_rounded = (int)(this.z * VertexLocation2D.ROUND_PRECISION);
        int x2_rounded = (int)(otherLocation.x * VertexLocation2D.ROUND_PRECISION);
        int z2_rounded = (int)(otherLocation.z * VertexLocation2D.ROUND_PRECISION);

        return x1_rounded == x2_rounded &&
            z1_rounded == z2_rounded &&
            this.layer == otherLocation.layer;
    }

    public override int GetHashCode()
    {
        int rando = 100000000;
        return ((int)this.x * (rando + 1)^2 + (int)this.z * (rando + 1)) + this.layer;
    }
}