using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarTilePathfinder
{

    public Tile startTile;
    public Tile endTile;
    public Dictionary<Tile, TileTracker> closedList = new Dictionary<Tile, TileTracker>();
    public Dictionary<Tile, TileTracker> openList = new Dictionary<Tile, TileTracker>();

    public AStarTilePathfinder(Tile startTile, Tile endTile)
    {
        this.startTile = startTile;
        this.endTile = endTile;

        // Debug cube
        /*GameObject debugCube = MonoBehaviour.Instantiate((GameObject)Resources.Load("debugCube1"));
        debugCube.transform.position = endTile.GetStandingPosition();*/
    }

    public TileTracker getBestOpenTileTracker()
    {
        float lastBestFScore = 1000000f;
        TileTracker lastBestTileTracker = null;
        foreach (TileTracker tileTracker in this.openList.Values)
        {
            if (lastBestTileTracker == null)
            {
                lastBestFScore = tileTracker.fScore;
                lastBestTileTracker = tileTracker;
            }
            else
            {
                if (tileTracker.fScore < lastBestFScore)
                {
                    lastBestFScore = tileTracker.fScore;
                    lastBestTileTracker = tileTracker;
                }
            }
        }

        return lastBestTileTracker;
    }

    public List<Tile> getPathToEndTile()
    {

        if (this.startTile == this.endTile)
        {
            // We're here. Yay. We didn't have to even move. Amazing
            return new List<Tile>();
        }

        this.openList.Add(this.startTile, new TileTracker(this.startTile));

        // The open list should be non-empty now
        TileTracker currentTileTracker = null;
        while (this.openList.Count > 0)
        {
            currentTileTracker = this.getBestOpenTileTracker();

            if (currentTileTracker.tile == this.endTile)
            {
                break;
            }

            Tile currentTile = currentTileTracker.tile;
            this.openList.Remove(currentTile);

            if (!this.closedList.ContainsKey(currentTile))
            {
                this.closedList.Add(currentTile, currentTileTracker);
            }
           

            // Debug cube
            /*GameObject debugCube = MonoBehaviour.Instantiate((GameObject)Resources.Load("debugCube2"));
            debugCube.transform.position = currentTile.GetStandingPosition();*/

            // Get tiles that can be walked to from the current tile
            // to consider
            List<Tile> walkableTiles = this.getWalkableTilesFromTile(currentTile);
            foreach (Tile tile in walkableTiles)
            {
                // stepCost
                float stepCost = currentTileTracker.gScore + Vector3.Distance(currentTile.tileCenterPosition, tile.tileCenterPosition);

                TileTracker existingTileInClosedList;
                this.closedList.TryGetValue(tile, out existingTileInClosedList);
                if (existingTileInClosedList != null && stepCost >= existingTileInClosedList.gScore)
                {
                    continue;
                }

                // Verify the current tile isn't in the open list
                TileTracker existingTileInOpenList;
                this.openList.TryGetValue(tile, out existingTileInOpenList);
                if (existingTileInOpenList == null)
                {
                    // Add it
                    existingTileInOpenList = new TileTracker(tile, currentTileTracker);
                    existingTileInOpenList.gScore = stepCost;
                    existingTileInOpenList.hScore = Vector3.Distance(tile.tileCenterPosition, this.endTile.tileCenterPosition);
                    existingTileInOpenList.fScore = existingTileInOpenList.gScore + existingTileInOpenList.hScore;
                    this.openList.Add(tile, existingTileInOpenList);
                }
                else
                {
                    if (stepCost < existingTileInOpenList.gScore)
                    {
                        // Reparent it and get the new score
                        existingTileInOpenList.parentTracker = currentTileTracker;
                        existingTileInOpenList.gScore = stepCost;
                        existingTileInOpenList.fScore = stepCost + existingTileInOpenList.hScore;
                    }
                }
            }
        }

        if (currentTileTracker != null)
        {
            // There is a path

            // Possibly travel anyways even though the end path wasn't met?
            if (currentTileTracker.tile == this.endTile)
            {
                List<Tile> tilesToTraverse = new List<Tile>();
                tilesToTraverse.Add(currentTileTracker.tile);
                TileTracker nextParent = currentTileTracker.parentTracker;
                while (nextParent != null)
                {
                    // Don't add the start tile to the paths to traverse
                    if (nextParent.tile != this.startTile)
                    {
                        tilesToTraverse.Add(nextParent.tile);
                    }
                    nextParent = nextParent.parentTracker;
                }
                tilesToTraverse.Reverse();
                return tilesToTraverse;
            }
            else
            {
                // End tile wasn't there
                return new List<Tile>();
            }
        }
        else
        {
            // No path. Sad days
            return new List<Tile>();
        }
    }

    public List<Tile> getWalkableTilesFromTile(Tile currentTile)
    {
        TileConfig currentTileConfig = currentTile.meshObject.GetComponent<TileConfig>();
        int tileLayer = currentTile.tileLayer;
        int tileX = (int)currentTile.tileCenterPosition.x;
        int tileZ = (int)currentTile.tileCenterPosition.z;
        int gridSize = WorldGrid.gridSize;
        List<Tile> walkableTiles = new List<Tile>();
        // Only consider corners if local adjacent tiles are both free.
        // For example, for the top-left corner to be considered,
        // the top and left tiles must both be traversable

        // First consider top, left, rigth, and bottom tiles
        TileLocation topTileLocation = new TileLocation(tileLayer, tileX, tileZ + gridSize);
        TileLocation leftTileLocation = new TileLocation(tileLayer, tileX - gridSize, tileZ);
        TileLocation bottomTileLocation = new TileLocation(tileLayer, tileX, tileZ - gridSize);
        TileLocation rightTileLocation = new TileLocation(tileLayer, tileX + gridSize, tileZ);
        Tile topTile = null;
        Tile leftTile = null;
        Tile rightTile = null;
        Tile bottomTile = null;
        bool topTileReachable = false;
        bool leftTileReachable = false;
        bool rightTileReachable = false;
        bool bottomTileReachable = false;

        if (!currentTileConfig.isNorthBorderImpassable)
        {
            WorldGrid.map.TryGetValue(topTileLocation, out topTile);
        }

        if (!currentTileConfig.isWestBorderImpassable)
        {
            WorldGrid.map.TryGetValue(leftTileLocation, out leftTile);
        }

        if (!currentTileConfig.isSouthBorderImpassable)
        {
            WorldGrid.map.TryGetValue(bottomTileLocation, out bottomTile);
        }

        if (!currentTileConfig.isEastBorderImpassable)
        {
            WorldGrid.map.TryGetValue(rightTileLocation, out rightTile);
        }

        if (topTile != null)
        {
            // Is it walkable?
            TileConfig topTileConfig = topTile.meshObject.GetComponent<TileConfig>();
            if (topTileConfig != null)
            {
                // Check if the top tile has its south border passable
                if (!topTileConfig.isSouthBorderImpassable)
                {
                    walkableTiles.Add(topTile);
                    topTileReachable = true;
                }
            }
        }

        if (leftTile != null)
        {
            // Is it walkable?
            TileConfig leftTileConfig = leftTile.meshObject.GetComponent<TileConfig>();
            if (leftTileConfig != null)
            {
                // Check if the left tile has its east border passable
                if (!leftTileConfig.isEastBorderImpassable)
                {
                    walkableTiles.Add(leftTile);
                    leftTileReachable = true;
                }
            }
        }

        if (rightTile != null)
        {
            // Is it walkable?
            TileConfig rightTileConfig = rightTile.meshObject.GetComponent<TileConfig>();
            if (rightTileConfig != null)
            {
                // Check if the right tile has its west border passable
                if (!rightTileConfig.isWestBorderImpassable)
                {
                    walkableTiles.Add(rightTile);
                    rightTileReachable = true;
                }
            }
        }

        if (bottomTile != null)
        {
            // Is it walkable?
            TileConfig bottomTileConfig = bottomTile.meshObject.GetComponent<TileConfig>();
            if (bottomTileConfig != null)
            {
                // Check if the bottom tile has its north border passable
                if (!bottomTileConfig.isNorthBorderImpassable)
                {
                    walkableTiles.Add(bottomTile);
                    bottomTileReachable = true;
                }
            }
        }

        // Now check the corner tiles, if their local two adjacent tiles are reachable
        if (topTileReachable && leftTileReachable)
        {
            // top left tile
            TileLocation topLeftTileLocation = new TileLocation(tileLayer, tileX - gridSize, tileZ + gridSize);
            Tile topLeftTile;
            WorldGrid.map.TryGetValue(topLeftTileLocation, out topLeftTile);
            if (topLeftTile != null)
            {
                // Is it walkable?
                TileConfig topLeftTileConfig = topLeftTile.meshObject.GetComponent<TileConfig>();
                if (topLeftTileConfig != null)
                {
                    // Check if both the right and bottom (east and south) borders of the top-left
                    // tile are passable
                    if (!topLeftTileConfig.isEastBorderImpassable && !topLeftTileConfig.isSouthBorderImpassable)
                    {
                        walkableTiles.Add(topLeftTile);
                    }
                }
            }
        }

        if (topTileReachable && rightTileReachable)
        {
            // top right tile
            TileLocation topRightTileLocation = new TileLocation(tileLayer, tileX + gridSize, tileZ + gridSize);
            Tile topRightTile;
            WorldGrid.map.TryGetValue(topRightTileLocation, out topRightTile);
            if (topRightTile != null)
            {
                // Is it walkable?
                TileConfig topRightTileConfig = topRightTile.meshObject.GetComponent<TileConfig>();
                if (topRightTileConfig != null)
                {
                    // Check if both the left and bottom (west and south) borders of the top-right
                    // tile are passable
                    if (!topRightTileConfig.isWestBorderImpassable && !topRightTileConfig.isSouthBorderImpassable)
                    {
                        walkableTiles.Add(topRightTile);
                    }
                }
            }
        }

        if (bottomTileReachable && rightTileReachable)
        {
            // bottom right tile
            TileLocation bottomRightTileLocation = new TileLocation(tileLayer, tileX + gridSize, tileZ - gridSize);
            Tile bottomRightTile;
            WorldGrid.map.TryGetValue(bottomRightTileLocation, out bottomRightTile);
            if (bottomRightTile != null)
            {
                // Is it walkable?
                TileConfig bottomRightTileConfig = bottomRightTile.meshObject.GetComponent<TileConfig>();
                if (bottomRightTileConfig != null)
                {
                    // Check if both the top and left (north and west) borders of the bottom-right
                    // tile are passable
                    if (!bottomRightTileConfig.isNorthBorderImpassable && !bottomRightTileConfig.isWestBorderImpassable)
                    {
                        walkableTiles.Add(bottomRightTile);
                    }
                }
            }
        }

        if (bottomTileReachable && leftTileReachable)
        {
            // bottom left tile
            TileLocation bottomLeftTileLocation = new TileLocation(tileLayer, tileX - gridSize, tileZ - gridSize);
            Tile bottomLeftTile;
            WorldGrid.map.TryGetValue(bottomLeftTileLocation, out bottomLeftTile);
            if (bottomLeftTile != null)
            {
                // Is it walkable?
                TileConfig bottomLeftTileConfig = bottomLeftTile.meshObject.GetComponent<TileConfig>();
                if (bottomLeftTileConfig != null)
                {
                    // Check if both the top and right (north and east) borders of the bottom-left
                    // tile are passable
                    if (!bottomLeftTileConfig.isNorthBorderImpassable && !bottomLeftTileConfig.isEastBorderImpassable)
                    {
                        walkableTiles.Add(bottomLeftTile);
                    }
                }
            }
        }

        return walkableTiles;
    }
}
