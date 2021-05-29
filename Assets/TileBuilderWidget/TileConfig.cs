using UnityEditor;
using UnityEngine;

/**
 * Component attached to all game tiles
 */
[ExecuteInEditMode]
public class TileConfig : MonoBehaviour
{

    /**
     * The layer of the world map this tile is on
     */
    public int layer;

    /**
     * Size of the tile
     */
    public int size;

    /**
     * Allows the use of "worldStandYOverride" to be used for the tile's
     * Y-pos standing position in world space.
     */
    public bool overrideYStandPos = false;

    /**
     * A world-space Y-value override for the tile standing position.
     * This is mainly used for when a tile needs to be walkable on a dock/bridge, etc.
     * This is only checked if the "override Y standing position" is checked for this tile.
     */
    public float worldStandYOverride = 0.0f;

    /**
     * Flags for which borders of this tile are not passable
     */
    public bool isNorthBorderImpassable = false;
    public bool isEastBorderImpassable = false;
    public bool isSouthBorderImpassable = false;
    public bool isWestBorderImpassable = false;


    /**
     * Temporary tile render for editing.
     */
    [System.NonSerialized]
    public GameObject northBorderEditorRender = null;
    [System.NonSerialized]
    public GameObject eastBorderEditorRender = null;
    [System.NonSerialized]
    public GameObject southBorderEditorRender = null;
    [System.NonSerialized]
    public GameObject westBorderEditorRender = null;

    public void Update()
    {
#if UNITY_EDITOR
        // Render walkability identifiers in edit mode when the brush type is set to it
        if (TileBuilderWindow.instance.brushType == TileBuilderWindow.BrushType.SetWalkable)
        {
            Vector3 camPosition = SceneView.lastActiveSceneView.camera.transform.position;
            camPosition = new Vector3(camPosition.x, 0, camPosition.z);
            Vector3 thisPosition = new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z);
            if (Vector3.Distance(camPosition, thisPosition) < 65)
            {
                Vector3[] vertices = gameObject.GetComponent<MeshFilter>().sharedMesh.vertices;
                Material lineRenderMaterial = (Material)Resources.Load("Materials/_TileRender_Red");
                /*
                    Tiles render bottom left, top left, top right, bottom right
                */
                // Render a cool tile yo
                // 5/10/2021 update. Render up to 4 thick lines at the borders of the tile
                // to signify which borders are impassable by players/NPCs/hooligans
                if (this.northBorderEditorRender == null)
                {
                    if (this.isNorthBorderImpassable)
                    {
                        Vector3 worldTopLeftVertex = gameObject.transform.position + vertices[1];
                        Vector3 worldTopRightVertex = gameObject.transform.position + vertices[2];
                        float distance_size = Vector3.Distance(worldTopRightVertex, worldTopLeftVertex);
                        GameObject northBorder = Instantiate((GameObject)Resources.Load("lineRender"));
                        northBorder.GetComponent<Renderer>().material = lineRenderMaterial;
                        northBorder.transform.localScale = new Vector3(0.25f, 0.01f, distance_size);
                        northBorder.transform.parent = gameObject.transform;
                        northBorder.transform.position = worldTopLeftVertex;
                        northBorder.transform.LookAt(worldTopRightVertex);
                        northBorder.transform.position += northBorder.transform.forward * (northBorder.transform.localScale.z / 2);
                        this.northBorderEditorRender = northBorder;
                    }
                }
                else
                {
                    if (this.isNorthBorderImpassable)
                    {
                        if (this.northBorderEditorRender.activeSelf == false)
                        {
                            this.northBorderEditorRender.SetActive(true);
                        }
                    }
                    else
                    {
                        // Exists, but is now passable
                        // Remove the object
                        MonoBehaviour.DestroyImmediate(this.northBorderEditorRender);
                        this.northBorderEditorRender = null;
                    }
                    
                }

                if (this.eastBorderEditorRender == null)
                {
                    if (this.isEastBorderImpassable)
                    {
                        Vector3 worldTopRightVertex = gameObject.transform.position + vertices[2];
                        Vector3 bottomRightVertex = gameObject.transform.position + vertices[3];
                        float distance_size = Vector3.Distance(worldTopRightVertex, bottomRightVertex);
                        GameObject eastBoarder = Instantiate((GameObject)Resources.Load("lineRender"));
                        eastBoarder.GetComponent<Renderer>().material = lineRenderMaterial;
                        eastBoarder.transform.localScale = new Vector3(0.25f, 0.01f, distance_size);
                        eastBoarder.transform.parent = gameObject.transform;
                        eastBoarder.transform.position = worldTopRightVertex;
                        eastBoarder.transform.LookAt(bottomRightVertex);
                        eastBoarder.transform.position += eastBoarder.transform.forward * (eastBoarder.transform.localScale.z / 2);
                        this.eastBorderEditorRender = eastBoarder;
                    }
                }
                else
                {
                    if (this.isEastBorderImpassable)
                    {
                        if (this.eastBorderEditorRender.activeSelf == false)
                        {
                            this.eastBorderEditorRender.SetActive(true);
                        }
                    }
                    else
                    {
                        // Exists, but is now passable
                        // Remove the object
                        MonoBehaviour.DestroyImmediate(this.eastBorderEditorRender);
                        this.eastBorderEditorRender = null;
                    }
                    
                }

                if (this.southBorderEditorRender == null)
                {
                    if (this.isSouthBorderImpassable)
                    {
                        Vector3 bottomRightVertex = gameObject.transform.position + vertices[3];
                        Vector3 bottomLeftVertex = gameObject.transform.position + vertices[0];
                        float distance_size = Vector3.Distance(bottomLeftVertex, bottomRightVertex);
                        GameObject southBorder = Instantiate((GameObject)Resources.Load("lineRender"));
                        southBorder.GetComponent<Renderer>().material = lineRenderMaterial;
                        southBorder.transform.localScale = new Vector3(0.25f, 0.01f, distance_size);
                        southBorder.transform.parent = gameObject.transform;
                        southBorder.transform.position = bottomRightVertex;
                        southBorder.transform.LookAt(bottomLeftVertex);
                        southBorder.transform.position += southBorder.transform.forward * (southBorder.transform.localScale.z / 2);
                        this.southBorderEditorRender = southBorder;
                    }
                }
                else
                {
                    if (this.isSouthBorderImpassable)
                    {
                        if (this.southBorderEditorRender.activeSelf == false)
                        {
                            this.southBorderEditorRender.SetActive(true);
                        }
                    }
                    else
                    {
                        // Exists, but is now passable
                        // Remove the object
                        MonoBehaviour.DestroyImmediate(this.southBorderEditorRender);
                        this.southBorderEditorRender = null;
                    }
                }

                if (this.westBorderEditorRender == null)
                {
                    if (this.isWestBorderImpassable)
                    {
                        Vector3 bottomLeftVertex = gameObject.transform.position + vertices[0];
                        Vector3 topLeftVertex = gameObject.transform.position + vertices[1];
                        float distance_size = Vector3.Distance(bottomLeftVertex, topLeftVertex);
                        GameObject westBorder = Instantiate((GameObject)Resources.Load("lineRender"));
                        westBorder.GetComponent<Renderer>().material = lineRenderMaterial;
                        westBorder.transform.localScale = new Vector3(0.25f, 0.01f, distance_size);
                        westBorder.transform.parent = gameObject.transform;
                        westBorder.transform.position = bottomLeftVertex;
                        westBorder.transform.LookAt(topLeftVertex);
                        westBorder.transform.position += westBorder.transform.forward * (westBorder.transform.localScale.z / 2);
                        this.westBorderEditorRender = westBorder;
                    }
                }
                else
                {
                    if (this.isWestBorderImpassable)
                    {
                        if (this.westBorderEditorRender.activeSelf == false)
                        {
                            this.westBorderEditorRender.SetActive(true);
                        }
                    }
                    else
                    {
                        // Exists, but is now passable
                        // Remove the object
                        MonoBehaviour.DestroyImmediate(this.westBorderEditorRender);
                        this.westBorderEditorRender = null;
                    }
                }
            }
            else
            {
                // Too far, deactive renders
                if (this.northBorderEditorRender != null && this.northBorderEditorRender.activeSelf == true)
                {
                    this.northBorderEditorRender.SetActive(false);
                }
                if (this.eastBorderEditorRender != null && this.eastBorderEditorRender.activeSelf == true)
                {
                    this.eastBorderEditorRender.SetActive(false);
                }
                if (this.southBorderEditorRender != null && this.southBorderEditorRender.activeSelf == true)
                {
                    this.southBorderEditorRender.SetActive(false);
                }
                if (this.westBorderEditorRender != null && this.westBorderEditorRender.activeSelf == true)
                {
                    this.westBorderEditorRender.SetActive(false);
                }
            }
        }
        else
        {
            if (this.northBorderEditorRender != null)
            {
                MonoBehaviour.DestroyImmediate(this.northBorderEditorRender);
                this.northBorderEditorRender = null;
            }
            if (this.eastBorderEditorRender != null)
            {
                MonoBehaviour.DestroyImmediate(this.eastBorderEditorRender);
                this.eastBorderEditorRender = null;
            }
            if (this.southBorderEditorRender != null)
            {
                MonoBehaviour.DestroyImmediate(this.southBorderEditorRender);
                this.southBorderEditorRender = null;
            }
            if (this.westBorderEditorRender != null)
            {
                MonoBehaviour.DestroyImmediate(this.westBorderEditorRender);
                this.westBorderEditorRender = null;
            }
        }
#endif
    }

    public void OnDestroy()
    {
        //TileLocation tLocation = new TileLocation(this.layer, (int) gameObject.transform.position.x, (int) gameObject.transform.position.z);
        //WorldGrid.map.Remove(tLocation);
    }
}
