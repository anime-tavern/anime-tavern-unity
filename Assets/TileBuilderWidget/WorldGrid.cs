using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WorldGrid
{

    public const string GRID_OBJECT_NAME = "WorldGrid";
    public const string GAME_TILES_MAIN_CONTAINER_NAME = "GameTiles";
    public const string GAME_TILE_TAG = "GameTile";

    // The settings GameObject in the main hierarchy
    public static GameObject settingsObject;

    // The settings component that is attached to the settingsObject
    public static WorldGridSettings settingsComponent;

    // The grid size of this world's/scene's grid
    public static int gridSize;

    // TODO: Possible remove this property and give TileBuilderWindow a static that sets to the current
    // instance of the actie tile builder window
    public TileBuilderWindow tileBuilderWindow;

    public static Dictionary<TileLocation, Tile> map = new Dictionary<TileLocation, Tile>();
    public static Dictionary<VertexLocation2D, VertexLocation3D> vertexMap = new Dictionary<VertexLocation2D, VertexLocation3D>();

    public WorldGrid(int gridSize, TileBuilderWindow tileBuilderWindow)
    {
        this.tileBuilderWindow = tileBuilderWindow;
        WorldGrid.gridSize = gridSize;
        this.createDataObjects();
        this.syncPropertiesToSettings();
    }

    public WorldGrid(GameObject worldGrid, TileBuilderWindow tileBuilderWindow)
    {
        this.tileBuilderWindow = tileBuilderWindow;
        this.loadDataObjects();
        this.syncPropertiesToSettings();
    }

    public void createDataObjects()
    {
        WorldGrid.settingsObject = new GameObject();
        WorldGrid.settingsObject.name = WorldGrid.GRID_OBJECT_NAME;
        WorldGrid.settingsObject.transform.position = new Vector3();
        WorldGrid.settingsComponent = settingsObject.AddComponent<WorldGridSettings>();
        WorldGrid.settingsComponent.tileBuilderWindow = this.tileBuilderWindow; 
    }

    public void loadDataObjects()
    {
        WorldGrid.settingsObject = GameObject.Find("/" + WorldGrid.GRID_OBJECT_NAME);
        WorldGrid.settingsObject.transform.position = new Vector3();
        WorldGrid.settingsComponent = settingsObject.GetComponent<WorldGridSettings>();
        WorldGrid.gridSize = settingsComponent.GridSize;
        WorldGrid.settingsComponent.tileBuilderWindow = this.tileBuilderWindow;

        GameObject worldGridTilesContainer = GameObject.Find("/" + WorldGrid.GAME_TILES_MAIN_CONTAINER_NAME);
        if (worldGridTilesContainer == null)
        {
            worldGridTilesContainer = new GameObject(WorldGrid.GAME_TILES_MAIN_CONTAINER_NAME);
        }
    }

    /**
     * Disables picking/selecting of the grid tiles in the Scene
     */
    public static void disablePickingOfSceneTiles()
    {
        GameObject worldGridTilesContainer = GameObject.Find("/" + WorldGrid.GAME_TILES_MAIN_CONTAINER_NAME);
        SceneVisibilityManager.instance.DisablePicking(worldGridTilesContainer, true);
    }

    /**
     * Syncs the known properties to the settingsComponent GameObject
     */
    public void syncPropertiesToSettings()
    {
        TagHelper.AddTagIfNotExists(GridGuidelines.GRID_GUIDELINE_TAG_NAME);
        WorldGrid.settingsComponent.GridSize = WorldGrid.gridSize;
        WorldGrid.disablePickingOfSceneTiles();
    }

    /**
     * Fetches an array of a tile's vertices based on the world grid size.
     * The return is in local space and not world space. This will satisfy
     * a line renderer's vertices (because it has a duplicate element at the end)
     */
    public static Vector3[] getLineRenderTileVertices()
    {
        float halfGridSize = WorldGrid.gridSize / 2;
        return new Vector3[5] {
            new Vector3(-halfGridSize,0,-halfGridSize),
            new Vector3(-halfGridSize,0,halfGridSize),
            new Vector3(halfGridSize,0,halfGridSize),
            new Vector3(halfGridSize,0,-halfGridSize),
            new Vector3(-halfGridSize,0,-halfGridSize)
        };
    }

    /**
     * Converts a world position into the nearest center of a tile
     */
    public static Vector3 worldPositionToNearestTileCenter(Vector3 worldPosition)
    {
        int xTileLocation = (int)(Mathf.Round(worldPosition.x / WorldGrid.gridSize) * WorldGrid.gridSize);
        int zTileLocation = (int)(Mathf.Round(worldPosition.z / WorldGrid.gridSize) * WorldGrid.gridSize);
        return new Vector3(xTileLocation, worldPosition.y, zTileLocation);
    }

    /**
     * Registers a tile into the world map
     */
    public static void registerTileInWorldMap(Tile tile, Vector3 centerPosition, int tileLayer)
    {
        TileLocation tLocation = new TileLocation(tileLayer, (int)centerPosition.x, (int)centerPosition.z);
        WorldGrid.map.Add(tLocation, tile);
    }

    /**
     * Registers a VertexLocation3D at a VertexLocation2D
     */
    public static void registerVertexInVertexMap(VertexLocation3D location3D, VertexLocation2D location2D)
    {
        WorldGrid.vertexMap.Add(location2D, location3D);
    }

    /**
     * Will fetch the 3D vertex data at a vertex location. If there is no data at that location,
     * then this function will automatically add a vertex at that location with a height of 0
     * unless the height parameter is provided.
     */
    public static VertexLocation3D getVertexAt2DLocation(VertexLocation2D location2D, int createAtWorldHeightIfNotExists = 0)
    {
        if (WorldGrid.vertexMap.ContainsKey(location2D))
        {
            return WorldGrid.vertexMap[location2D];
        }
        else
        {
            // Create it
            VertexLocation3D location3D = new VertexLocation3D(location2D.layer, location2D.x, createAtWorldHeightIfNotExists, location2D.z);
            WorldGrid.registerVertexInVertexMap(location3D, location2D);
            return location3D;
        }
    }

    public static Tile getTileAtWorldPositionOnLayer(Vector3 position, int tileLayer)
    {
        TileLocation tLocation = new TileLocation(tileLayer, (int)position.x, (int)position.z);
        Tile val;
        if (WorldGrid.map.TryGetValue(tLocation, out val))
        {
            return val;
        }

        return null;
    }

    /**
     * Fetches all adjacent tiles. A single tile can have up to 8 adjacent tiles
     */
    public static List<Tile> getAdjacentTiles(Tile tile)
    {
        Vector3 tilePosition = tile.tileCenterPosition;
        List<Tile> tilesFound = new List<Tile>();
        TileLocation[] locationsToCheck = new TileLocation[8]
        {
            new TileLocation(tile.tileLayer, (int)tilePosition.x - WorldGrid.gridSize, (int)tilePosition.z - WorldGrid.gridSize),
            new TileLocation(tile.tileLayer, (int)tilePosition.x - WorldGrid.gridSize, (int)tilePosition.z + WorldGrid.gridSize),
            new TileLocation(tile.tileLayer, (int)tilePosition.x - WorldGrid.gridSize, (int)tilePosition.z),
            new TileLocation(tile.tileLayer, (int)tilePosition.x + WorldGrid.gridSize, (int)tilePosition.z + WorldGrid.gridSize),
            new TileLocation(tile.tileLayer, (int)tilePosition.x + WorldGrid.gridSize, (int)tilePosition.z - WorldGrid.gridSize),
            new TileLocation(tile.tileLayer, (int)tilePosition.x + WorldGrid.gridSize, (int)tilePosition.z),
            new TileLocation(tile.tileLayer, (int)tilePosition.x, (int)tilePosition.z + WorldGrid.gridSize),
            new TileLocation(tile.tileLayer, (int)tilePosition.x, (int)tilePosition.z - WorldGrid.gridSize),
        };

        foreach(TileLocation location in locationsToCheck)
        {
            if (WorldGrid.map.ContainsKey(location))
            {
                tilesFound.Add(WorldGrid.map[location]);
            }
        }

        return tilesFound;
    }


    /**
     * Fetches all vertices adjacent to the provided one. Will _NOT_ 
     * zero-fill vertices that don't exist (such as off the edge of the map).
     */
    public static List<VertexLocation2D> getAdjacentVertices(VertexLocation2D v2DLocation)
    {
        List<VertexLocation2D> foundLocations = new List<VertexLocation2D>();
        VertexLocation2D[] vertexLocationsToCheck = new VertexLocation2D[4]
        {
            new VertexLocation2D(v2DLocation.layer, v2DLocation.x, v2DLocation.z),
            new VertexLocation2D(v2DLocation.layer, v2DLocation.x + WorldGrid.gridSize, v2DLocation.z),
            new VertexLocation2D(v2DLocation.layer, v2DLocation.x, v2DLocation.z + WorldGrid.gridSize),
            new VertexLocation2D(v2DLocation.layer, v2DLocation.x + WorldGrid.gridSize, v2DLocation.z + WorldGrid.gridSize),
        };

        foreach (VertexLocation2D vertexLocation in vertexLocationsToCheck)
        {
            if (WorldGrid.vertexMap.ContainsKey(vertexLocation))
            {
                foundLocations.Add(vertexLocation);
            }
        }

        return foundLocations;
    }

    /**
     * CPU heavy function. Will iterate over all of the game tiles and refresh the map data.
     * Usually called when the editor is first opened.
     */
    public static void LoadMapDataFromPhysicalTiles()
    {
        // Reset the map, if necessary
        WorldGrid.map = new Dictionary<TileLocation, Tile>();
        WorldGrid.vertexMap = new Dictionary<VertexLocation2D, VertexLocation3D>();
        GameObject tileContainer = GameObject.Find("/" + WorldGrid.GAME_TILES_MAIN_CONTAINER_NAME);
        if (tileContainer)
        {
            // Iterate over all children
            int childCount = tileContainer.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                GameObject tileObject = tileContainer.transform.GetChild(i).gameObject;
                Tile tile = new Tile(tileObject);
                try
                {
                    WorldGrid.registerTileInWorldMap(tile, tile.tileCenterPosition, tile.tileLayer);
                    int iterator = 0;
                    foreach(VertexLocation2D tileVertex2D in tile.vertex2DLocations) 
                    {
                        try {
                            VertexLocation3D vertex3DLocation = new VertexLocation3D(tile.tileLayer, tileVertex2D.x, tile.mesh.vertices[iterator].y, tileVertex2D.z);
                            WorldGrid.registerVertexInVertexMap(vertex3DLocation, tileVertex2D);
                        }
                        catch (System.ArgumentException) { }
                        ++iterator;
                    }
                }
                catch (System.ArgumentException) 
                {
                    Debug.LogWarning("An issue happened while syncing tiles with internal map data. Most likely a tile is sitting at the same location of another tile. This issue did not prevent the rest of the map from being synchronized.");
                }
                
            }
        }
    }

}