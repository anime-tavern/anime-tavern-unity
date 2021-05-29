using UnityEditor;
using UnityEngine;

public class VertexLocation3D
{
    public const int ROUND_PRECISION = 10;

    public float x;
    public float y;
    public float z;
    public int layer;

    public VertexLocation3D(int layer, float x, float y, float z)
    {
        this.layer = layer;
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(this.x, this.y, this.z);
    }

    /**
     * Returns an object-space Vector3
     */
    public Vector3 ToObjectSpaceVector3(Vector3 worldPosition)
    {
        return new Vector3(this.x - worldPosition.x, this.y - worldPosition.y, this.z - worldPosition.z);
    }

    public override bool Equals(object obj)
    {
        VertexLocation3D otherLocation = obj as VertexLocation3D;
        if (otherLocation == null)
        {
            return false;
        }

        // Round the X,Y, and Z values to a precision
        int x1_rounded = (int)(this.x * VertexLocation3D.ROUND_PRECISION);
        int y1_rounded = (int)(this.y * VertexLocation3D.ROUND_PRECISION);
        int z1_rounded = (int)(this.z * VertexLocation3D.ROUND_PRECISION);
        int x2_rounded = (int)(otherLocation.x * VertexLocation3D.ROUND_PRECISION);
        int y2_rounded = (int)(otherLocation.y * VertexLocation3D.ROUND_PRECISION);
        int z2_rounded = (int)(otherLocation.z * VertexLocation3D.ROUND_PRECISION);

        return x1_rounded == x2_rounded &&
            y1_rounded == y2_rounded &&
            z1_rounded == z2_rounded &&
            this.layer == otherLocation.layer;
    }

    public override int GetHashCode()
    {
        int rando = 100000000;
        return ((int)this.x * (rando + 1)^3 + (int)this.y * (rando+1)^2 + (int)this.z * (rando + 1)) + this.layer;
    }
}