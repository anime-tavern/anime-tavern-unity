using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public delegate void Notify(Vector3 oldTilePosition, Vector3 newTilePosition);

public class GridGuidelines
{

    public const string GUIDELINES_CONTAINER_NAME = "GridGuidelinesContainer";
    public const string GUIDELINES_GUIDETILE_NAME = "GridTileGuideline";
    public const string GRID_GUIDELINE_MAIN_CONTAINER_TAG_NAME = "GridGuidelinesMasterContainer";
    public const string GRID_GUIDELINE_MAIN_CONTAINER_PENDING_REMOVAL_TAG_NAME = "GridGuidelinesMasterContainer_PendingRemoval";
    public const string GRID_GUIDELINE_TAG_NAME = "AnimeTavernGridGuidelinesObject";

    public static int? brushSize;
    public static Vector3 currentGuidelinesRenderFromPosition;
    public static Vector3 gridGuidelinesWorldCenter;
    public static Vector3 currentPrimaryTilePosition;
    public static List<Vector3> gridGuidelineTileCenters;
    public static GameObject gridGuidelinesContainer;
    public static Material currentGuidelinesMaterial;

    public static event Notify GridPrimaryTilePositionChanged;

    public static void createGridGuidelinesContainerIfNotExists()
    {
        // Check if there are existing grid guidelines
        if (GridGuidelines.gridGuidelinesContainer == null)
        {
            // Try to find it
            GridGuidelines.gridGuidelinesContainer = GameObject.FindGameObjectWithTag(GridGuidelines.GRID_GUIDELINE_MAIN_CONTAINER_TAG_NAME);
            if (gridGuidelinesContainer == null)
            {

                if (GridGuidelines.currentGuidelinesMaterial == null)
                {
                    GridGuidelines.currentGuidelinesMaterial = (Material)Resources.Load("Materials/LineRenderMaterial");
                }

                GridGuidelines.gridGuidelinesContainer = new GameObject();
                GridGuidelines.gridGuidelinesContainer.AddComponent<WorldGridLinesContainer>();
                GridGuidelines.gridGuidelinesContainer.name = GridGuidelines.GUIDELINES_CONTAINER_NAME;
                GridGuidelines.gridGuidelinesContainer.tag = GridGuidelines.GRID_GUIDELINE_MAIN_CONTAINER_TAG_NAME;
                GridGuidelines.gridGuidelinesContainer.transform.parent = WorldGrid.settingsObject.transform;
            }
        }
    }

    public static void clearAllGuidelinesContainers()
    {
        GameObject[] containers = GameObject.FindGameObjectsWithTag(GridGuidelines.GRID_GUIDELINE_MAIN_CONTAINER_PENDING_REMOVAL_TAG_NAME);
        foreach (GameObject container in containers)
        {
            WorldGrid.settingsComponent.destroyOtherObject(container);
        }
    }

    public static void enableRendering()
    {
        GridGuidelines.createGridGuidelinesContainerIfNotExists();
        WorldGridLinesContainer guidelinesComponent = GridGuidelines.gridGuidelinesContainer.GetComponent<WorldGridLinesContainer>();
        guidelinesComponent.isActive = true;
    }

    public static void disableRendering()
    {
        if (GridGuidelines.gridGuidelinesContainer != null)
        {
            WorldGridLinesContainer guidelinesComponent = GridGuidelines.gridGuidelinesContainer.GetComponent<WorldGridLinesContainer>();
            guidelinesComponent.isActive = false;
            GridGuidelines.gridGuidelinesContainer.tag = GridGuidelines.GRID_GUIDELINE_MAIN_CONTAINER_PENDING_REMOVAL_TAG_NAME;
            GridGuidelines.gridGuidelinesContainer = null;
            GridGuidelines.clearAllGuidelinesContainers();
        }
    }

    public static void renderGuidelinesAtPosition(Vector3 position, int height = 0)
    {

        GridGuidelines.createGridGuidelinesContainerIfNotExists();

        // Clamp the Y value to a whole number
        position.y = height;

        GridGuidelines.currentGuidelinesRenderFromPosition = position;
        Vector3 primaryTileCenterPosition = WorldGrid.worldPositionToNearestTileCenter(position);

        if (GridGuidelines.currentPrimaryTilePosition == primaryTileCenterPosition)
        {
            // Don't waste time re-rendering at the same location
            return;
        }

        // How far the center of the grid guidelines would be
        float factorToCenter;
        if (GridGuidelines.brushSize == 0 || (WorldGrid.gridSize % 2 == 0 && GridGuidelines.brushSize % 2 > 0))
        {
            factorToCenter = (float)(WorldGrid.gridSize / 2 * (GridGuidelines.brushSize - 1));
        }
        else
        {
            factorToCenter = (float)(WorldGrid.gridSize / 2 * (GridGuidelines.brushSize));
        }

        Vector3 bottomLeftCornerOfGuidelines = primaryTileCenterPosition - new Vector3(
            factorToCenter + WorldGrid.gridSize / 2,
            0,
            factorToCenter + WorldGrid.gridSize / 2
        );
        Vector3 topRightCornerOfGuidelines = primaryTileCenterPosition + new Vector3(
            factorToCenter + WorldGrid.gridSize / 2,
            0,
            factorToCenter + WorldGrid.gridSize / 2
        );
        GridGuidelines.gridGuidelinesWorldCenter = Vector3.Lerp(bottomLeftCornerOfGuidelines, topRightCornerOfGuidelines, 0.5f);


        if (GridGuidelines.gridGuidelinesContainer.transform.childCount > 0)
        {
            GridGuidelines.gridGuidelinesContainer.transform.position = primaryTileCenterPosition;
            GridGuidelines.updateVertexHeightsOfLineRenderers();
        }
        else
        {
            GridGuidelines.gridGuidelineTileCenters = new List<Vector3>();
            for (int x = 0; x < GridGuidelines.brushSize; x++)
            {
                for (int z = 0; z < GridGuidelines.brushSize; z++)
                {
                    int nextXOffset = x * WorldGrid.gridSize;
                    int nextZOffset = z * WorldGrid.gridSize;
                    Vector3 offsetVector = new Vector3(
                        nextXOffset,
                        0,
                        nextZOffset
                    );
                    // Because this is being rendered from the bottom-left corner
                    // and the goal is to have the guidelines be centered at the mouse point,
                    // then every guideContainer must be adjusted by a negative vector
                    // with X and Z coordinates equal to (gridSize * brushSize)/2
                    offsetVector -= new Vector3(factorToCenter, 0, factorToCenter);

                    // offsetVector is the tile center

                    // Clamp to the nearest grid position
                    //offsetVector = WorldGrid.worldPositionToNearestTileCenter(offsetVector);

                    GridGuidelines.gridGuidelineTileCenters.Add(offsetVector);
                    Vector3[] tileVertices = WorldGrid.getLineRenderTileVertices();
                    GameObject guideContainer = GridGuidelines.createGuidelinesObject(offsetVector, tileVertices);

                    guideContainer.transform.parent = GridGuidelines.gridGuidelinesContainer.transform;
                }
            }
        }

        // Fire event that the grid guidelines position has changed
        if (GridGuidelines.GridPrimaryTilePositionChanged != null)
        {
            GridGuidelines.GridPrimaryTilePositionChanged.Invoke(GridGuidelines.currentPrimaryTilePosition, primaryTileCenterPosition);
        }
        GridGuidelines.currentPrimaryTilePosition = primaryTileCenterPosition;
    }

    /**
     * Updates the heights of the line renderers to match the vertex data their
     * corresponding tile's vertices have
     */
    public static void updateVertexHeightsOfLineRenderers()
    {
        GameObject[] guideContainers = GameObject.FindGameObjectsWithTag(GridGuidelines.GRID_GUIDELINE_TAG_NAME);
        foreach (GameObject guideContainer in guideContainers)
        {
            Vector3 guideContainerWorldSpaceCenter = guideContainer.transform.position;
            LineRenderer lRender = guideContainer.GetComponent<LineRenderer>();
            Vector3[] newVertices = new Vector3[5];
            int numPositions = lRender.positionCount;
            int currentLayer = TileBuilderWindow.instance.gridLayer;
            for (int i = 0; i < numPositions - 1; i++)
            {
                Vector3 existingVertex = lRender.GetPosition(i);
                Vector3 existingVertexWorldSpace = guideContainerWorldSpaceCenter + existingVertex;
                VertexLocation2D v2DLocation = new VertexLocation2D(currentLayer, existingVertexWorldSpace.x, existingVertexWorldSpace.z);
                VertexLocation3D v3DLocation = WorldGrid.getVertexAt2DLocation(v2DLocation);
                Vector3 newVertex = new Vector3(existingVertex.x, v3DLocation.y, existingVertex.z);
                newVertices[i] = newVertex;
                if (i == 0)
                {
                    newVertices[4] = newVertex;
                }
            }

            lRender.SetPositions(newVertices);
        }
    }

    /**
     * Updates the current material being used for the guideline rendering
     */
    public static void updateGuidelinesMaterial()
    {
        GameObject[] guideContainers = GameObject.FindGameObjectsWithTag(GridGuidelines.GRID_GUIDELINE_TAG_NAME);
        foreach(GameObject guideContainer in guideContainers)
        {
            LineRenderer lRender = guideContainer.GetComponent<LineRenderer>();
            lRender.sharedMaterial = GridGuidelines.currentGuidelinesMaterial;
        }
    }

    /**
     * Creates the GameObject and LineRender needed for a grid guideline
     * with a give set of vertices
     */
    public static GameObject createGuidelinesObject(Vector3 centerPosition, Vector3[] vertices)
    {
        GameObject guideContainer = new GameObject();
        guideContainer.tag = GridGuidelines.GRID_GUIDELINE_TAG_NAME;
        guideContainer.name = GridGuidelines.GUIDELINES_GUIDETILE_NAME;
        guideContainer.transform.position = centerPosition;

        LineRenderer lRenderer = guideContainer.AddComponent<LineRenderer>();
        lRenderer.sharedMaterial = GridGuidelines.currentGuidelinesMaterial;
        lRenderer.widthMultiplier = 0.05f;
        lRenderer.useWorldSpace = false;
        lRenderer.positionCount = 5;

        // Get the 3D vertex (that has Y height)
        // of the vertex data that is stored
        int currentTileLayer = TileBuilderWindow.instance.gridLayer;
        int iterator = 0;
        Vector3[] verticesWithTerrainHeightIncluded = new Vector3[5];
        foreach (Vector3 vertex in vertices)
        {
            Vector3 vertexInWorldSpace = centerPosition + vertex;
            VertexLocation2D vertexAs2D = new VertexLocation2D(currentTileLayer, vertexInWorldSpace.x, vertexInWorldSpace.z);
            VertexLocation3D vertex3DLocation = WorldGrid.getVertexAt2DLocation(vertexAs2D);
            
            Vector3 locationAdjustedForTerrain = new Vector3(vertex.x, vertex3DLocation.y, vertex.z);
            verticesWithTerrainHeightIncluded[iterator] = locationAdjustedForTerrain;
            if (iterator == 0)
            {
                verticesWithTerrainHeightIncluded[4] = locationAdjustedForTerrain;
            }

            ++iterator;
        }

        lRenderer.SetPositions(verticesWithTerrainHeightIncluded);

        return guideContainer;
    }
}