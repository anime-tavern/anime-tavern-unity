using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class WorldGridSettings : MonoBehaviour
{

    [SerializeField]
    public int GridSize;

    [System.NonSerialized]
    public TileBuilderWindow tileBuilderWindow;

    public void OnValidate()
    {
        // Rerender the grid
        WorldGrid.gridSize = this.GridSize;
        #if UNITY_EDITOR
        // Avoid a ton of warnings appearing because of doing object methods OnValidate
        UnityEditor.EditorApplication.delayCall += () =>
        {
            try
            {
                GridGuidelines.renderGuidelinesAtPosition(GridGuidelines.currentGuidelinesRenderFromPosition);
            }
            catch (System.NullReferenceException) { }
        };
        #endif
    }

    public void destroyOtherObject(GameObject otherObject)
    {
        #if UNITY_EDITOR
        // Without this, DestroyImmediate will not function when called from OnValidate
        UnityEditor.EditorApplication.delayCall += () =>
        {
            DestroyImmediate(otherObject);
        };
        #endif
    }

#if UNITY_EDITOR
    public void OnDestroy()
    {
        // When this gets destroyed, it's probably best to
        // tell the TileBuilderWindow to re-enable the grid creator group
        tileBuilderWindow.WorldGridSettingsDestroyed();
    }
#endif
}