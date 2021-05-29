using System;
using UnityEditor;
using UnityEngine;

public class TileBuilderWindow : EditorWindow
{

    public const int MIN_BRUSH_STRENGTH_SLIDER_VALUE = 1;
    public const int MAX_BRUSH_STRENGTH_SLIDER_VALUE = 10;
    public const float MAX_BRUSH_STRENGTH = 0.05f; // For the height tool
    public const float MIN_BRUSH_STRENGTH = 0.001f; // For the height tool
    public const float MIN_SMOOTHEN_BRUSH_STRENGTH = 0.001f; 
    public const float MAX_SMOOTHEN_BRUSH_STRENGTH = 0.1f; 

    public enum BrushType: int { 
        None = 0,
        Rectangular = 1, 
        Circular = 2,
        Height = 3,
        Flatten = 4,
        Smoothen = 5,
        Remove = 6,
        SetWalkable = 7,
    };

    public UnityEngine.Object materialField;
    public int brushSize = 1;
    public int brushStrengthAsInteger = (MIN_BRUSH_STRENGTH_SLIDER_VALUE + MAX_BRUSH_STRENGTH_SLIDER_VALUE) / 2;
    public WorldGrid worldGrid;
    public int gridSize;
    public int gridLayer;
    public bool isCreateWorldGridUIDisabled = false;
    public bool isNorthFlagUnwalkable = false;
    public bool isEastFlagUnwalkable = false;
    public bool isSouthFlagUnwalkable = false;
    public bool isWestFlagUnwalkable = false;
    public bool createWorldGridButtonPressed;
    public BrushType brushType = BrushType.None;

    public static TileBuilderWindow instance;

    [MenuItem("Anime Tavern/Tile Builder")]
    public static void ShowWindow()
    {
        GetWindow(typeof(TileBuilderWindow));
    }

    /**
     * When the window becomes active (not necessarily shown) in Unity
     */
    public void OnEnable()
    {
        TileBuilderWindow.instance = this;
        TagHelper.AddTagIfNotExists(GridGuidelines.GRID_GUIDELINE_MAIN_CONTAINER_TAG_NAME);
        TagHelper.AddTagIfNotExists(GridGuidelines.GRID_GUIDELINE_MAIN_CONTAINER_PENDING_REMOVAL_TAG_NAME);
        TagHelper.AddTagIfNotExists(WorldGrid.GAME_TILE_TAG);

        // Does the world grid already exist?
        if (GameObject.Find("/" + WorldGrid.GRID_OBJECT_NAME) == null)
        {
            if (this.createWorldGridButtonPressed)
            {
                this.isCreateWorldGridUIDisabled = true;
                this.createGrid();
            }
        } 
        else
        {
            this.isCreateWorldGridUIDisabled = true;
            this.loadGrid();
        }

        Debug.Log("Loading existing map tiles.");
        WorldGrid.LoadMapDataFromPhysicalTiles();
        Debug.Log("Existing map loaded.");
    }

    public void OnDisable()
    {
    }

    /**
     * When the window is shown/visible or the user has selected it in the editor
     */
    public void OnGUI()
    {

        // Setup initial brush size
        GridGuidelines.brushSize = this.brushSize;

        // Create the grid UI elements
        EditorGUI.BeginDisabledGroup(this.isCreateWorldGridUIDisabled);
        this.gridSize = EditorGUILayout.IntField("Grid Size", this.gridSize);
        this.createWorldGridButtonPressed = GUILayout.Button("Create Grid in Scene");
        EditorGUI.EndDisabledGroup();
        
        this.DrawUILine(new Color(0.3f,0.3f,0.3f), 2, 28);
        this.materialField = EditorGUILayout.ObjectField("Tile material", materialField, typeof(Material), false);

        // Brush size and change detection for it
        EditorGUI.BeginChangeCheck();
        this.brushSize = EditorGUILayout.IntSlider("Brush size", brushSize, 1, 15);

        if (EditorGUI.EndChangeCheck())
        {
            GridGuidelines.brushSize = this.brushSize;
            if (this.brushType != BrushType.None)
            {
                GridGuidelines.disableRendering();
                GridGuidelines.enableRendering();
            }
        }

        this.DrawUILine(new Color(0.3f, 0.3f, 0.3f), 2, 28);
        this.gridLayer = EditorGUILayout.IntField("Grid Layer", this.gridLayer);

        if (GUILayout.Button("Place Tiles"))
        {
            this.swapToTool(BrushType.Rectangular);
        }

        if (GUILayout.Button("Remove Tiles"))
        {
            this.swapToTool(BrushType.Remove);
        }

        this.DrawUILine(new Color(0.3f, 0.3f, 0.3f), 2, 28);

        this.brushStrengthAsInteger = EditorGUILayout.IntSlider("Strength", this.brushStrengthAsInteger, TileBuilderWindow.MIN_BRUSH_STRENGTH_SLIDER_VALUE, TileBuilderWindow.MAX_BRUSH_STRENGTH_SLIDER_VALUE);

        if (GUILayout.Button("Height Tool"))
        {
            this.swapToTool(BrushType.Height);
        }

        if (GUILayout.Button("Flatten Tool"))
        {
            this.swapToTool(BrushType.Flatten);
        }

        if (GUILayout.Button("Smoothen Tool"))
        {
            this.swapToTool(BrushType.Smoothen);
        }

        this.DrawUILine(new Color(0.3f, 0.3f, 0.3f), 2, 28);

        if (GUILayout.Button("(Flag) Set Walkable"))
        {
            this.swapToTool(BrushType.SetWalkable);
        }

        EditorGUILayout.Space();
        this.isNorthFlagUnwalkable = EditorGUILayout.ToggleLeft("North unwalkable", this.isNorthFlagUnwalkable);
        this.isEastFlagUnwalkable = EditorGUILayout.ToggleLeft("East unwalkable", this.isEastFlagUnwalkable);
        this.isSouthFlagUnwalkable = EditorGUILayout.ToggleLeft("South unwalkable", this.isSouthFlagUnwalkable);
        this.isWestFlagUnwalkable = EditorGUILayout.ToggleLeft("West unwalkable", this.isWestFlagUnwalkable);

        this.DrawUILine(new Color(0.3f, 0.3f, 0.3f), 2, 28);
        if (GUILayout.Button("Sync Tiles With Map Data"))
        {
            WorldGrid.LoadMapDataFromPhysicalTiles();
        }
    }

    public void swapToTool(BrushType brushType)
    {
        if (brushType == this.brushType)
        {
            // Same brush type. Disable rendering
            this.brushType = BrushType.None;
            GridGuidelines.disableRendering();
        }
        else
        {
            this.brushType = brushType;

            if (brushType == BrushType.Remove)
            {
                GridGuidelines.currentGuidelinesMaterial = (Material)Resources.Load("Materials/LineRenderMaterialDanger");
            }
            else
            {
                GridGuidelines.currentGuidelinesMaterial = (Material)Resources.Load("Materials/LineRenderMaterial");
            }

            GridGuidelines.updateGuidelinesMaterial();
            GridGuidelines.enableRendering();
        }
    }

    /**
     * Renders a horizontal line in the current window
     */
    public void DrawUILine(Color color, int thickness = 2, int padding = 10)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x = 10;
        r.width -= 14;
        EditorGUI.DrawRect(r, color);
    }

    /**
     * Creates a new instance of WorldGrid. Used when there is no WorldGrid GameObject in the scene to be
     * loaded from.
     */
    public void createGrid()
    {
        this.worldGrid = new WorldGrid(this.gridSize, this);
        WorldGrid.settingsComponent.tileBuilderWindow = this;
    }

    /**
     * Loads a WorldGrid instance from an existing GameObject representing the scene's world grid
     */
    public void loadGrid()
    {
        if (this.worldGrid == null) {
            GameObject worldGridObject = GameObject.Find("/" + WorldGrid.GRID_OBJECT_NAME);
            this.worldGrid = new WorldGrid(worldGridObject, this);
            WorldGrid.settingsComponent.tileBuilderWindow = this;
        }
    }

    /**
     * Called when the WorldGrid GameObject is destroyed
     */
    public void WorldGridSettingsDestroyed()
    {
        this.isCreateWorldGridUIDisabled = false;
    }
}
