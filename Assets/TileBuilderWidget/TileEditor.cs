using UnityEditor;
using UnityEngine;

/**
 * Custom editor for when a game tile is selected
 */
[CustomEditor(typeof(TileConfig))]
public class TileEditor : Editor
{
    /**
     * Place handles on a tile
     */
    private void OnSceneGUI()
    {
        Tools.hidden = true;
        TileConfig tile = (TileConfig)target;
    }

    private void OnDisable()
    {
        Tools.hidden = false;
    }
}
