using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class WorldGridLinesContainer : MonoBehaviour
{

    [System.NonSerialized]
    public bool isActive = false;

    [System.NonSerialized]
    public float lastHighestVertex = 0.0f;

    public void OnEnable()
    {
        SceneView.duringSceneGui += this.OnScene;
    }

    public void OnDisable()
    {
        SceneView.duringSceneGui -= this.OnScene;
    }

    public void OnDestroy()
    {
        SceneView.duringSceneGui -= this.OnScene;
    }

    public void CleanupEvents()
    {
        GridGuidelines.GridPrimaryTilePositionChanged -= this.OnGridGuidelinesPrimaryTilePositionChanged;
    }

    public void OnGridGuidelinesPrimaryTilePositionChanged(Vector3 oldPosition, Vector3 newPosition)
    {
        if (this.isActive)
        {
            if (TileBuilderWindow.instance.brushType == TileBuilderWindow.BrushType.Rectangular)
            {
                this.paintTilesInGrid();
            }
            else if (TileBuilderWindow.instance.brushType == TileBuilderWindow.BrushType.Flatten)
            {
                this.flatten();
            }
            else if (TileBuilderWindow.instance.brushType == TileBuilderWindow.BrushType.Remove)
            {
                this.destroyTilesInGrid();
            }
        }
    }

    public void OnScene(SceneView scene)
    {
        if (this.isActive == true)
        {

            // Get the Editor control ID of this ... UI? I guess.
            // It's used to disable the selection box UI that gets drawn
            // when dragging the mouse with left-mouse down
            int thisControlID = GUIUtility.GetControlID(FocusType.Passive);

            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseMove || currentEvent.type == EventType.MouseDrag)
            {
                // Allow non-left-mouse-button events to proceed
                if (currentEvent.type == EventType.MouseDrag && currentEvent.button != 0)
                {
                    return;
                }

                if (currentEvent.type == EventType.MouseDrag)
                {
                    if (TileBuilderWindow.instance.brushType == TileBuilderWindow.BrushType.Height)
                    {
                        // Increase the height of everything in the grid lines
                        int yDelta = (int)currentEvent.delta.y;
                        this.adjustHeightOfTiles(-yDelta);
                        return;
                    }
                }

                // Move the grid
                Vector2 mousePosition = currentEvent.mousePosition;
                float ppp = EditorGUIUtility.pixelsPerPoint;
                mousePosition.y = scene.camera.pixelHeight - mousePosition.y * ppp;
                mousePosition.x *= ppp;
                Vector3 worldPosition = MousePositionHelper.GetWorldPositionFromMousePosition(mousePosition);

                // Handle adjusting it if there is tile data
                worldPosition = MousePositionHelper.GetWorldPositionAdjustedForMedianTerrainHeight(worldPosition);
                GridGuidelines.renderGuidelinesAtPosition(worldPosition);

                // Consume the event and don't let others use it
                currentEvent.Use();

                // DO NOT CALL THIS
                // IT LAGS LIKE A MOTHER
                // SceneVisibilityManager.instance.DisableAllPicking();
            }
            else if (currentEvent.type == EventType.MouseDown)
            {
                if (currentEvent.button == 0)
                {
                    // Disable the selection box that happens when dragging
                    GUIUtility.hotControl = thisControlID;

                    if (TileBuilderWindow.instance.brushType == TileBuilderWindow.BrushType.Rectangular)
                    {
                        this.paintTilesInGrid();
                        // Possibly cleanup old event connection
                        GridGuidelines.GridPrimaryTilePositionChanged -= this.OnGridGuidelinesPrimaryTilePositionChanged;
                        GridGuidelines.GridPrimaryTilePositionChanged += this.OnGridGuidelinesPrimaryTilePositionChanged;
                    }
                    else if (TileBuilderWindow.instance.brushType == TileBuilderWindow.BrushType.Flatten)
                    {
                        this.lastHighestVertex = this.findHighestVertex();
                        this.flatten();
                        // Possibly cleanup old event connection
                        GridGuidelines.GridPrimaryTilePositionChanged -= this.OnGridGuidelinesPrimaryTilePositionChanged;
                        GridGuidelines.GridPrimaryTilePositionChanged += this.OnGridGuidelinesPrimaryTilePositionChanged;
                    }
                    else if (TileBuilderWindow.instance.brushType == TileBuilderWindow.BrushType.Smoothen)
                    {

                        // Try and remove it from the update delegation first
                        // in case the user let go of the mouse button outside of the scene view
                        UnityEditor.EditorApplication.update -= this.smoothen;

                        UnityEditor.EditorApplication.update += this.smoothen;
                    }
                    else if (TileBuilderWindow.instance.brushType == TileBuilderWindow.BrushType.Remove)
                    {
                        this.destroyTilesInGrid();
                        // Possibly cleanup old event connection
                        GridGuidelines.GridPrimaryTilePositionChanged -= this.OnGridGuidelinesPrimaryTilePositionChanged;
                        GridGuidelines.GridPrimaryTilePositionChanged += this.OnGridGuidelinesPrimaryTilePositionChanged;
                    }
                    else if (TileBuilderWindow.instance.brushType == TileBuilderWindow.BrushType.SetWalkable)
                    {
                        this.toggleTilesWalkable();
                    }
                    else if (TileBuilderWindow.instance.brushType == TileBuilderWindow.BrushType.Paint)
                    {
                        this.paintTiles();
                    }
                }
            }
            else if (currentEvent.type == EventType.MouseUp)
            {
                if (currentEvent.button == 0)
                {
                    this.CleanupEvents();
                    WorldGrid.disablePickingOfSceneTiles();
                    if (TileBuilderWindow.instance.brushType == TileBuilderWindow.BrushType.Smoothen)
                    {
                        UnityEditor.EditorApplication.update -= this.smoothen;
                    }
                }
            }
            else if (currentEvent.type == EventType.KeyDown)
            {
                if (currentEvent.keyCode == KeyCode.Escape)
                {
                    this.CleanupEvents();
                    this.isActive = false;

                    // Swapping to the same brush will disable it
                    TileBuilderWindow.instance.swapToTool(TileBuilderWindow.instance.brushType);
                    currentEvent.Use();
                }
                else if (currentEvent.keyCode == KeyCode.R)
                {
                    if (TileBuilderWindow.instance.brushType == TileBuilderWindow.BrushType.Paint)
                    {
                        this.rotateUVMapsOfTileMesh();
                    }
                }
            }
        }
    }

    /**
     * Finds the highest vertex point in the current grid guidelines
     */
    public float findHighestVertex()
    {
        float currentHighest = 0.0f;
        foreach (Vector3 tileCenterPositionInObjectSpace in GridGuidelines.gridGuidelineTileCenters.ToArray())
        {
            Vector3 tileCenterPosition = WorldGrid.worldPositionToNearestTileCenter(
                GridGuidelines.currentGuidelinesRenderFromPosition
            ) + tileCenterPositionInObjectSpace;

            int currentLayer = TileBuilderWindow.instance.gridLayer;

            Tile existingTile = WorldGrid.getTileAtWorldPositionOnLayer(tileCenterPosition, currentLayer);
            if (existingTile != null)
            {
                VertexLocation3D[] vertexLocationData = existingTile.getVertexLocation3DData();
                foreach(VertexLocation3D vertexData in vertexLocationData)
                {
                    if (vertexData.y > currentHighest)
                    {
                        currentHighest = vertexData.y;
                    }
                }
            }
        }

        return currentHighest;
    }

    /**
     * Applies a gaussian blur to the current selection of terrain
     */
    public void smoothen()
    {
        Vector3[] gridGuidelineTileCenters = GridGuidelines.gridGuidelineTileCenters.ToArray();
        Dictionary<VertexLocation2D, VertexLocation3D> vertexCollection = new Dictionary<VertexLocation2D, VertexLocation3D>();
        foreach (Vector3 tileCenterPositionInObjectSpace in gridGuidelineTileCenters)
        {
            Vector3 tileCenterPosition = WorldGrid.worldPositionToNearestTileCenter(
                GridGuidelines.currentGuidelinesRenderFromPosition
            ) + tileCenterPositionInObjectSpace;

            // The current tile layer where tiles are being adjusted
            int currentLayer = TileBuilderWindow.instance.gridLayer;

            Tile existingTile = WorldGrid.getTileAtWorldPositionOnLayer(tileCenterPosition, currentLayer);
            if (existingTile != null)
            {
                VertexLocation2D[] vertex2DLocations = existingTile.vertex2DLocations;
                foreach (VertexLocation2D vertexLocation2D in vertex2DLocations)
                {
                    // Get the 3D world space data at this location
                    VertexLocation3D v3DLocation = WorldGrid.getVertexAt2DLocation(vertexLocation2D);
                    if (!vertexCollection.ContainsKey(vertexLocation2D))
                    {
                        vertexCollection.Add(vertexLocation2D, v3DLocation);
                    }
                }
            }
        }

        // Blur them
        MathHelper.applyBoxFilterToVertexData(ref vertexCollection);

        // Apply them

        // Keep track of tiles already adjusted to be EFFICIENT and avoid PhysX errors
        Dictionary<Tile, bool> processedTiles = new Dictionary<Tile, bool>();
        foreach (Vector3 tileCenterPositionInObjectSpace in gridGuidelineTileCenters)
        {
            Vector3 tileCenterPosition = WorldGrid.worldPositionToNearestTileCenter(
                GridGuidelines.currentGuidelinesRenderFromPosition
            ) + tileCenterPositionInObjectSpace;

            // The current tile layer where tiles are being adjusted
            int currentLayer = TileBuilderWindow.instance.gridLayer;

            Tile existingTile = WorldGrid.getTileAtWorldPositionOnLayer(tileCenterPosition, currentLayer);
            if (existingTile != null)
            {
                if (!processedTiles.ContainsKey(existingTile))
                {
                    processedTiles.Add(existingTile, true);
                    existingTile.updateMeshVertices();
                }

                Tile[] tilesAdjacent = WorldGrid.getAdjacentTiles(existingTile).ToArray();
                foreach (Tile adjacentTile in tilesAdjacent)
                {
                    if (!processedTiles.ContainsKey(adjacentTile))
                    {
                        adjacentTile.updateMeshVertices();
                        processedTiles.Add(adjacentTile, true);
                    }
                }
            }
        }
    }

    /**
     * Toggles the isWalkable flag on tiles
     */
    public void toggleTilesWalkable()
    {
        bool isNorthImpassable = TileBuilderWindow.instance.isNorthFlagUnwalkable;
        bool isEastImpassable = TileBuilderWindow.instance.isEastFlagUnwalkable;
        bool isSouthImpassable = TileBuilderWindow.instance.isSouthFlagUnwalkable;
        bool isWestImpassable = TileBuilderWindow.instance.isWestFlagUnwalkable;

        Vector3[] gridGuidelineTileCenters = GridGuidelines.gridGuidelineTileCenters.ToArray();
        foreach (Vector3 tileCenterPositionInObjectSpace in gridGuidelineTileCenters)
        {
            Vector3 tileCenterPosition = WorldGrid.worldPositionToNearestTileCenter(
                GridGuidelines.currentGuidelinesRenderFromPosition
            ) + tileCenterPositionInObjectSpace;

            // The current tile layer where tiles are being adjusted
            int currentLayer = TileBuilderWindow.instance.gridLayer;

            Tile existingTile = WorldGrid.getTileAtWorldPositionOnLayer(tileCenterPosition, currentLayer);
            if (existingTile != null)
            {
                TileConfig tConfig = existingTile.meshObject.GetComponent<TileConfig>();
                tConfig.isNorthBorderImpassable = isNorthImpassable;
                tConfig.isEastBorderImpassable = isEastImpassable;
                tConfig.isSouthBorderImpassable = isSouthImpassable;
                tConfig.isWestBorderImpassable = isWestImpassable;
            }
        }
    }

    /**
     * Paints the tiles with the current texture in the tile editor
     */
    public void paintTiles()
    {

        Vector3[] gridGuidelineTileCenters = GridGuidelines.gridGuidelineTileCenters.ToArray();
        foreach (Vector3 tileCenterPositionInObjectSpace in gridGuidelineTileCenters)
        {
            Vector3 tileCenterPosition = WorldGrid.worldPositionToNearestTileCenter(
                GridGuidelines.currentGuidelinesRenderFromPosition
            ) + tileCenterPositionInObjectSpace;

            // The current tile layer where tiles are being adjusted
            int currentLayer = TileBuilderWindow.instance.gridLayer;

            Tile existingTile = WorldGrid.getTileAtWorldPositionOnLayer(tileCenterPosition, currentLayer);
            if (existingTile != null)
            {
                existingTile.meshRenderer.material = (Material) TileBuilderWindow.instance.materialField;
            }
        }
    }

    /**
     * Rotates the UV maps of a tile mesh by 90 degrees CW
     */
    public void rotateUVMapsOfTileMesh()
    {

        Vector3[] gridGuidelineTileCenters = GridGuidelines.gridGuidelineTileCenters.ToArray();
        foreach (Vector3 tileCenterPositionInObjectSpace in gridGuidelineTileCenters)
        {
            Vector3 tileCenterPosition = WorldGrid.worldPositionToNearestTileCenter(
                GridGuidelines.currentGuidelinesRenderFromPosition
            ) + tileCenterPositionInObjectSpace;

            // The current tile layer where tiles are being adjusted
            int currentLayer = TileBuilderWindow.instance.gridLayer;

            Tile existingTile = WorldGrid.getTileAtWorldPositionOnLayer(tileCenterPosition, currentLayer);
            if (existingTile != null)
            {
                existingTile.rotateUVs90DegreesClockwise();
            }
        }
    }

    /**
     * Flattens terrain inside the current grid guidelines
     * to the highest vertex when the mouse was down
     */
    public void flatten()
    {
        Vector3[] gridGuidelineTileCenters = GridGuidelines.gridGuidelineTileCenters.ToArray();
        foreach (Vector3 tileCenterPositionInObjectSpace in gridGuidelineTileCenters)
        {
            Vector3 tileCenterPosition = WorldGrid.worldPositionToNearestTileCenter(
                GridGuidelines.currentGuidelinesRenderFromPosition
            ) + tileCenterPositionInObjectSpace;

            // The current tile layer where tiles are being adjusted
            int currentLayer = TileBuilderWindow.instance.gridLayer;

            Tile existingTile = WorldGrid.getTileAtWorldPositionOnLayer(tileCenterPosition, currentLayer);
            if (existingTile != null)
            {
                // Adjust the height of the vertices that this tile uses
                VertexLocation2D[] vertex2DLocations = existingTile.vertex2DLocations;
                foreach (VertexLocation2D vertexLocation2D in vertex2DLocations)
                {
                    // Get the 3D world space data at this location
                    VertexLocation3D v3DLocation = WorldGrid.getVertexAt2DLocation(vertexLocation2D);
                    v3DLocation.y = this.lastHighestVertex;
                }
                existingTile.updateMeshVertices();

                Tile[] tilesAdjacent = WorldGrid.getAdjacentTiles(existingTile).ToArray();
                foreach (Tile adjacentTile in tilesAdjacent)
                {
                    adjacentTile.updateMeshVertices();
                }
            }
        }
    }

    /**
     * Renders tile in the grid lines if the location is empty
     */
    public void paintTilesInGrid()
    {
        foreach (Vector3 tileCenterPositionInObjectSpace in GridGuidelines.gridGuidelineTileCenters.ToArray())
        {
            Vector3 tileCenterPosition = WorldGrid.worldPositionToNearestTileCenter(
                GridGuidelines.currentGuidelinesRenderFromPosition
            ) + tileCenterPositionInObjectSpace;

            int currentLayer = TileBuilderWindow.instance.gridLayer;

            Tile existingTile = WorldGrid.getTileAtWorldPositionOnLayer(tileCenterPosition, currentLayer);
            if (existingTile == null)
            {
                Tile tile = new Tile(tileCenterPosition, WorldGrid.gridSize, currentLayer);
                if (TileBuilderWindow.instance.materialField)
                {
                    tile.tileMaterial = (Material)TileBuilderWindow.instance.materialField;
                }
                tile.render();
            }
            else
            {
                // Debug.Log("TILE EXISTS HERE");
            }
        }
    }

    /**
     * Destroys tiles in the grid guidelines
     */
    public void destroyTilesInGrid()
    {
        foreach (Vector3 tileCenterPositionInObjectSpace in GridGuidelines.gridGuidelineTileCenters.ToArray())
        {
            Vector3 tileCenterPosition = WorldGrid.worldPositionToNearestTileCenter(
                GridGuidelines.currentGuidelinesRenderFromPosition
            ) + tileCenterPositionInObjectSpace;

            int currentLayer = TileBuilderWindow.instance.gridLayer;

            Tile existingTile = WorldGrid.getTileAtWorldPositionOnLayer(tileCenterPosition, currentLayer);
            if (existingTile != null)
            {
                existingTile.remove();
            }
        }
    }

    /**
     * Adjusts the vertex height of tiles within the grid lines. 
     * The further away a vertex of a tile is from the center point, the less it is affected.
     */
    public void adjustHeightOfTiles(int mouseYDelta)
    {

        // Strength of the height adjustment
        float strengthRange = TileBuilderWindow.MAX_BRUSH_STRENGTH - TileBuilderWindow.MIN_BRUSH_STRENGTH;
        float sliderPercent = (float)TileBuilderWindow.instance.brushStrengthAsInteger / (float)TileBuilderWindow.MAX_BRUSH_STRENGTH_SLIDER_VALUE;
        float strength = TileBuilderWindow.MIN_BRUSH_STRENGTH + (sliderPercent) * strengthRange;

        // This is the maximum Y value change
        // The rest will multiply a percent against this as the vertex gets further
        float maxYVertexAdjustment = strength * mouseYDelta;

        Vector3[] gridGuidelineTileCenters = GridGuidelines.gridGuidelineTileCenters.ToArray();
        foreach (Vector3 tileCenterPositionInObjectSpace in gridGuidelineTileCenters)
        {
            Vector3 tileCenterPosition = WorldGrid.worldPositionToNearestTileCenter(
                GridGuidelines.currentGuidelinesRenderFromPosition
            ) + tileCenterPositionInObjectSpace;

            // The current tile layer where tiles are being adjusted
            int currentLayer = TileBuilderWindow.instance.gridLayer;

            Tile existingTile = WorldGrid.getTileAtWorldPositionOnLayer(tileCenterPosition, currentLayer);
            if (existingTile != null)
            {
                // Adjust the height of the vertices that this tile uses
                VertexLocation2D[] vertex2DLocations = existingTile.vertex2DLocations;
                foreach(VertexLocation2D vertexLocation2D in vertex2DLocations)
                {
                    // Get the 3D world space data at this location
                    VertexLocation3D v3DLocation = WorldGrid.getVertexAt2DLocation(vertexLocation2D);

                    // Adjust the Y axis
                    // Get the distance from the current render position of the grid guidelines
                    // Ignore the Y values
                    float distanceFromRenderCenter = Vector3.Distance(
                        new Vector3(GridGuidelines.currentGuidelinesRenderFromPosition.x, 0, GridGuidelines.currentGuidelinesRenderFromPosition.z),
                        new Vector3(v3DLocation.x, 0, v3DLocation.z)
                    );

                    // Make the minimum distance that is 100% effected be the distance to
                    // all vertices of a tile
                    float minimumDistanceEffectedAt100Percent = (float)(WorldGrid.gridSize / 2);

                    float maxDistance = minimumDistanceEffectedAt100Percent + (float)(WorldGrid.gridSize * GridGuidelines.brushSize) / 2;
                    float distancePercentage = distanceFromRenderCenter / maxDistance;
                    float falloffPercent = 1 - distancePercentage;

                    if (falloffPercent < 0)
                    {
                        falloffPercent = 0;
                    }

                    float vertexYAdjustment = falloffPercent * maxYVertexAdjustment;
                    v3DLocation.y += vertexYAdjustment;
                }

                existingTile.updateMeshVertices();

                Tile[] tilesAdjacent = WorldGrid.getAdjacentTiles(existingTile).ToArray();
                foreach(Tile adjacentTile in tilesAdjacent)
                {
                    adjacentTile.updateMeshVertices();
                }
            }
        }
    }

    public static bool areVectorsCloseEnoughToBeTheSame(Vector3 v1, Vector3 v2)
    {
        int roundingPrecision = 10; // Multiply them to this (100) then compare
        int x1 = (int)(v1.x * roundingPrecision);
        int x2 = (int)(v2.x * roundingPrecision);
        int y1 = (int)(v1.y * roundingPrecision);
        int y2 = (int)(v2.y * roundingPrecision);
        int z1 = (int)(v1.z * roundingPrecision);
        int z2 = (int)(v2.z * roundingPrecision);
        
        return x1 == x2 && y1 == y2 && z1 == z2;
    }
}