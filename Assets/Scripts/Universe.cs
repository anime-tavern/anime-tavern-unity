using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Universe : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        WorldGrid.LoadMapDataFromPhysicalTiles();

        GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
        GameObject player = PlayerBuilder.getBasePlayerObject();
        PlayerController playerController = player.GetComponent<PlayerController>();
        Tile spawnTile = null;
        WorldGrid.map.TryGetValue(playerController.spawnLocation, out spawnTile);
        playerController.teleportToTile(spawnTile);
        player.GetComponent<PlayerController>().playerCamera = cam;
        Vector3 camPosition = new Vector3(0, 12, -10);
        cam.transform.LookAt(player.transform);
        Debug.Log("spanwed");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
