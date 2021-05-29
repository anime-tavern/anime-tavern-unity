using UnityEditor;
using UnityEngine;
using System.Diagnostics;

public class Tile
{

    public static GameObject allTilesContainer;

    public Vector3 tileCenterPosition;
    public VertexLocation2D[] vertex2DLocations = new VertexLocation2D[4];
    public Material tileMaterial;
    public GameObject meshObject;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;
    public Mesh mesh;
    public int tileSize;
    public int tileLayer;
    public bool isRendered;
    public bool registerTileInWorldGrid = false;

    /**
     * Check if a game object is a world tile
     */
    public static bool isGameObjectATile(GameObject possibleTile)
    {
        if (possibleTile != null)
        {
            return possibleTile.tag == WorldGrid.GAME_TILE_TAG;
        }

        return false;
    }

    /**
     * Gets the world position of the center of the tile with a Y axis
     * value that is the median of the vertices heights
     */
    public Vector3 GetStandingPosition()
    {

        // Check if this has an override active for the Y position
        TileConfig config = this.meshObject.GetComponent<TileConfig>();
        if (config.overrideYStandPos == false)
        {
            float highestValue = 0;
            float? lowestValue = null;
            foreach (VertexLocation2D v2DLocation in this.vertex2DLocations)
            {
                VertexLocation3D v3DLocation = WorldGrid.getVertexAt2DLocation(v2DLocation);
                if (v3DLocation.y > highestValue)
                {
                    highestValue = v3DLocation.y;
                }

                if (lowestValue == null || v3DLocation.y < lowestValue)
                {
                    lowestValue = v3DLocation.y;
                }
            }

            // Return the tile's center position + the average between
            // the lowest vertex and the heighest vertex
            return this.tileCenterPosition + new Vector3(0, (highestValue + (float)lowestValue) / 2, 0);
        }
        else
        {
            Vector3 worldTileCenter = this.tileCenterPosition;
            worldTileCenter.y = config.worldStandYOverride;
            return worldTileCenter;
        }
    }

    public Tile(Vector3 tileCenterPosition, int tileSize, int tileLayer, bool registerTileInWorldGrid = true)
    {
        this.registerTileInWorldGrid = registerTileInWorldGrid;
        this.tileCenterPosition = tileCenterPosition;

        // Get the 2D vertex locations
        Vector2 vertex0AsWorldPosition = new Vector2(
            tileCenterPosition.x + (-tileSize / 2),
            tileCenterPosition.z + (-tileSize / 2)
        );
        Vector2 vertex1AsWorldPosition = new Vector2(
            tileCenterPosition.x + (-tileSize / 2),
            tileCenterPosition.z + (tileSize / 2)
        );
        Vector2 vertex2AsWorldPosition = new Vector2(
            tileCenterPosition.x + (tileSize / 2),
            tileCenterPosition.z + (tileSize / 2)
        );
        Vector2 vertex3AsWorldPosition = new Vector2(
            tileCenterPosition.x + (tileSize / 2),
            tileCenterPosition.z + (-tileSize / 2)
        );
        this.vertex2DLocations[0] = new VertexLocation2D(tileLayer, vertex0AsWorldPosition.x, vertex0AsWorldPosition.y);
        this.vertex2DLocations[1] = new VertexLocation2D(tileLayer, vertex1AsWorldPosition.x, vertex1AsWorldPosition.y);
        this.vertex2DLocations[2] = new VertexLocation2D(tileLayer, vertex2AsWorldPosition.x, vertex2AsWorldPosition.y);
        this.vertex2DLocations[3] = new VertexLocation2D(tileLayer, vertex3AsWorldPosition.x, vertex3AsWorldPosition.y);
        this.tileSize = tileSize;
        this.tileLayer = tileLayer;

        if (registerTileInWorldGrid)
        {
            // Add this tile to the world map array
            WorldGrid.registerTileInWorldMap(this, tileCenterPosition, tileLayer);
        }
    }

    public Tile(GameObject tileObject)
    {
        TileConfig config = tileObject.GetComponent<TileConfig>();
        this.meshObject = tileObject;
        this.meshRenderer = tileObject.GetComponent<MeshRenderer>();
        this.mesh = this.meshObject.GetComponent<MeshFilter>().sharedMesh;
        this.meshCollider = this.meshObject.GetComponent<MeshCollider>();
        //this.mesh = this.meshObject.GetComponent<MeshFilter>().mesh;
        this.tileLayer = config.layer;

        Vector3 vertex0AsWorldPosition = tileObject.transform.position + mesh.vertices[0];
        Vector3 vertex1AsWorldPosition = tileObject.transform.position + mesh.vertices[1];
        Vector3 vertex2AsWorldPosition = tileObject.transform.position + mesh.vertices[2];
        Vector3 vertex3AsWorldPosition = tileObject.transform.position + mesh.vertices[3];
        this.vertex2DLocations[0] = new VertexLocation2D(this.tileLayer, vertex0AsWorldPosition.x, vertex0AsWorldPosition.z);
        this.vertex2DLocations[1] = new VertexLocation2D(this.tileLayer, vertex1AsWorldPosition.x, vertex1AsWorldPosition.z);
        this.vertex2DLocations[2] = new VertexLocation2D(this.tileLayer, vertex2AsWorldPosition.x, vertex2AsWorldPosition.z);
        this.vertex2DLocations[3] = new VertexLocation2D(this.tileLayer, vertex3AsWorldPosition.x, vertex3AsWorldPosition.z);
        this.tileCenterPosition = tileObject.transform.position;
        
        this.tileSize = config.size;
        this.isRendered = true;
    }

    public override bool Equals(object obj)
    {
        Tile otherTile = (Tile)obj;
        if (otherTile == null)
        {
            return false;
        }
        Vector3 otherCenter = otherTile.tileCenterPosition;
        Vector3 thisCenter = this.tileCenterPosition;
        int thisCenterXRounded = (int)thisCenter.x * 10;
        int otherCenterXRounded = (int)otherCenter.x * 10;
        int thisCenterZRounded = (int)thisCenter.z * 10;
        int otherCenterZRounded = (int)otherCenter.z * 10;
        bool equality = (int)(otherCenter.x * 10) == (int)(thisCenter.x * 10) &&
            (int)(otherCenter.z * 10) == (int)(thisCenter.z * 10) &&
            this.tileLayer == otherTile.tileLayer;
        /*UnityEngine.Debug.Log("[" + equality + "]Equals check | " + thisCenterXRounded + "="
            + otherCenterXRounded + " AND " + thisCenterZRounded + "=" + otherCenterZRounded
            + " AND " + this.tileLayer + "=" + otherTile.tileLayer);*/

        return equality;
    }

    public static bool operator ==(Tile left, Tile right)
    {
        if (left is null)
        {
            if (right is null)
            {
                return true;
            }

            return false;
        }
        return left.Equals(right);
    }

    public static bool operator !=(Tile left, Tile right) => !(left == right);

    public override int GetHashCode()
    {
        int rando = 100000000;
        return ((int)this.tileCenterPosition.x * (rando + 1)^3) + 
            ((int)this.tileCenterPosition.y * (rando + 1)^2) + 
            ((int)this.tileCenterPosition.z * (rando + 1)) + 
            this.tileLayer;
    }

    /**
     * Gets the VertexLocation3D data for this tile's vertices
     */
    public VertexLocation3D[] getVertexLocation3DData()
    {
        return new VertexLocation3D[4] {
            WorldGrid.getVertexAt2DLocation(this.vertex2DLocations[0]),
            WorldGrid.getVertexAt2DLocation(this.vertex2DLocations[1]),
            WorldGrid.getVertexAt2DLocation(this.vertex2DLocations[2]),
            WorldGrid.getVertexAt2DLocation(this.vertex2DLocations[3]),
        };
    }

    /**
     * Checks for the map tile container. If one doesn't exist, creates it.
     */
    public GameObject getTileContainer()
    {
        if (Tile.allTilesContainer == null)
        {
            GameObject tileContainer = GameObject.Find("/" + WorldGrid.GAME_TILES_MAIN_CONTAINER_NAME);
            if (tileContainer == null)
            {
                tileContainer = new GameObject();
                tileContainer.name = WorldGrid.GAME_TILES_MAIN_CONTAINER_NAME;
            }

            Tile.allTilesContainer = tileContainer;
        }
        
        return Tile.allTilesContainer;
    }

    /**
     * Updates an already-rendered mesh's vertices by fetching them again
     * from the world vertex map.
     */
    public void updateMeshVertices()
    {
        Vector3 worldPositionOfMeshObject = this.meshObject.transform.position;
        this.meshCollider.sharedMesh = null;
        mesh.vertices = new Vector3[4]
        {
            WorldGrid.getVertexAt2DLocation(this.vertex2DLocations[0]).ToObjectSpaceVector3(worldPositionOfMeshObject),
            WorldGrid.getVertexAt2DLocation(this.vertex2DLocations[1]).ToObjectSpaceVector3(worldPositionOfMeshObject),
            WorldGrid.getVertexAt2DLocation(this.vertex2DLocations[2]).ToObjectSpaceVector3(worldPositionOfMeshObject),
            WorldGrid.getVertexAt2DLocation(this.vertex2DLocations[3]).ToObjectSpaceVector3(worldPositionOfMeshObject),
        };
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        try
        {
            this.meshCollider.sharedMesh = this.mesh;
        }
        catch (System.Exception) { }
        
    }

    public void remove()
    {
        TileLocation tLocation = new TileLocation(this.tileLayer, (int)this.tileCenterPosition.x, (int)this.tileCenterPosition.z);
        WorldGrid.map.Remove(tLocation);
        MonoBehaviour.DestroyImmediate(this.meshObject);
    }

    public void rotateUVs90DegreesClockwise()
    {
        mesh.uv = new Vector2[4]
        {
            mesh.uv[3],
            mesh.uv[0],
            mesh.uv[1],
            mesh.uv[2]
        };
    }

    public void rotateUVs90DegreesCounterclockwise()
    {
        mesh.uv = new Vector2[4]
        {
            mesh.uv[1],
            mesh.uv[2],
            mesh.uv[3],
            mesh.uv[0]
        };
    }

    public void render()
    {
        GameObject tileContainer = this.getTileContainer();

        // Mesh object
        GameObject meshObject = new GameObject();
        meshObject.tag = WorldGrid.GAME_TILE_TAG;
        meshObject.transform.position = this.tileCenterPosition;
        meshObject.name = string.Format("[{0}][{1},{2}]", this.tileLayer, this.tileCenterPosition.x, this.tileCenterPosition.z);
        meshObject.transform.SetParent(tileContainer.transform);

        if (this.registerTileInWorldGrid)
        {
            // TileConfig custom functionality for the mesh GameObject
            TileConfig tileConfig = meshObject.AddComponent<TileConfig>();
            tileConfig.layer = this.tileLayer;
            tileConfig.size = this.tileSize;
        }
        
        // Mesh itself
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[4]
        {
            WorldGrid.getVertexAt2DLocation(this.vertex2DLocations[0]).ToObjectSpaceVector3(this.tileCenterPosition),
            WorldGrid.getVertexAt2DLocation(this.vertex2DLocations[1]).ToObjectSpaceVector3(this.tileCenterPosition),
            WorldGrid.getVertexAt2DLocation(this.vertex2DLocations[2]).ToObjectSpaceVector3(this.tileCenterPosition),
            WorldGrid.getVertexAt2DLocation(this.vertex2DLocations[3]).ToObjectSpaceVector3(this.tileCenterPosition),
        };
        mesh.triangles = new int[6]
        {
            0,1,2,
            2,3,0
        };
        mesh.uv = new Vector2[4]
        {
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(0, 0)
        };
        mesh.normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };

        // Mesh renderer
        MeshRenderer renderer = meshObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = tileMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        // Mesh collider
        MeshCollider collider = meshObject.AddComponent<MeshCollider>();
        //collider.convex = true;
        collider.enabled = true;
        collider.sharedMesh = mesh;

        // Mesh filter
        MeshFilter filter = meshObject.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        // Recalcs
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        // Set properties
        this.mesh = mesh;
        this.meshCollider = collider;
        this.meshObject = meshObject;
        this.meshObject.transform.position = this.tileCenterPosition;
        this.isRendered = true;
        // this.rotateUVs90DegreesCounterclockwise();
    }

    public override string ToString()
    {
        return "Tile[" + this.tileLayer + "](" + this.tileCenterPosition.x + "," + this.tileCenterPosition.z + ")";
    }
}