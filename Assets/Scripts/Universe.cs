using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Universe : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        UnityEngine.Debug.Log("Loading world grid");
        WorldGrid.LoadMapDataFromPhysicalTiles();
        stopWatch.Stop();
        long ts = stopWatch.ElapsedMilliseconds;
        UnityEngine.Debug.Log("World grid loaded in " + ts + "ms");

        GameObject spawnLocationMarker = GameObject.FindGameObjectWithTag("SpawnLocation");
        Vector3 nearestTileCenter = WorldGrid.worldPositionToNearestTileCenter(spawnLocationMarker.transform.position);
        Tile spawnTile = WorldGrid.getTileAtWorldPositionOnLayer(nearestTileCenter, 0);

        GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
        GameObject player = PlayerBuilder.getBasePlayerObject();
        PlayerController playerController = player.GetComponent<PlayerController>();
        playerController.teleportToTile(spawnTile);
        player.GetComponent<PlayerController>().playerCamera = cam;
        Vector3 camPosition = new Vector3(0, 12, -10);
        cam.transform.LookAt(player.transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
