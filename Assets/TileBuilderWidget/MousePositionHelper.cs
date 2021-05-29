using System;
using UnityEditor;
using UnityEngine;

/**
 * Helper class to assist with processing information
 * about the mouse's position.
 */
public class MousePositionHelper
{

    /**
     * The mouse's position isn't exactly as it should be
     * in the scene view and must be adjusted based on the pixel height
     * of the camera for accurate screen-to-world coordinates
     */
    public static Vector3 GetWorldPositionFromMousePosition(Vector3 mousePosition)
    {
        // How far the distance to the end location is. This will be changed by Raycast()
        float distanceToHitTile = 0;
        float distanceToOriginPlane = 0;

        // Convert the screen position to a ray
        Ray ray = SceneView.lastActiveSceneView.camera.ScreenPointToRay(mousePosition);
        Ray rayToOriginPlane = SceneView.lastActiveSceneView.camera.ScreenPointToRay(mousePosition);

        // Create a plane at a given intersection point that faces updwards
        // Then raycast to it
        Plane hPlane = new Plane(Vector3.up, new Vector3(0,-35,0));
        hPlane.Raycast(ray, out distanceToHitTile);

        // Create a plane at 0,0,0 in case no tile was hit by the negative plane
        Plane originPlane = new Plane(Vector3.up, new Vector3(0, 0, 0));
        originPlane.Raycast(rayToOriginPlane, out distanceToOriginPlane);

        // distance is now the distance to the desired plane
        RaycastHit hit;
        bool didHit = Physics.Raycast(ray.origin, ray.direction, out hit, distanceToHitTile);
        if (didHit)
        {
            return hit.point;
        }
        else
        {
            return rayToOriginPlane.GetPoint(distanceToOriginPlane);
        }
    }

    /**
     * Takes a world position found from a raycast. Then transpiles that location
     * to a grid tile center. Then determines if there is vertex data
     * at that location. Finally, returns the worldPosition provided, but with a Y
     * axis value that is the median of the highest vertex Y value and the lowest vertex Y value.
     */
    public static Vector3 GetWorldPositionAdjustedForMedianTerrainHeight(Vector3 worldPosition)
    {
        int tileLayer = TileBuilderWindow.instance.gridLayer;
        Vector3 tileCenter = WorldGrid.worldPositionToNearestTileCenter(worldPosition);
        Tile tile = WorldGrid.getTileAtWorldPositionOnLayer(tileCenter, tileLayer);
        if (tile != null)
        {
            int[] yAxisValues = new int[4];
            float highestValue = 0;
            float lowestValue = 0;
            foreach(VertexLocation2D v2DLocation in tile.vertex2DLocations)
            {
                VertexLocation3D v3DLocation = WorldGrid.getVertexAt2DLocation(v2DLocation);
                if (v3DLocation.y > highestValue)
                {
                    highestValue = v3DLocation.y;
                }

                if (v3DLocation.y < lowestValue)
                {
                    lowestValue = v3DLocation.y;
                }
            }

            // Adjust the world position
            worldPosition.y = (highestValue + lowestValue)/ 2;
        }

        return worldPosition;
    }
}
