using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [System.NonSerialized]
    public const float MAX_CLICK_RAYCAST_DISTANCE = 500.0f;

    [System.NonSerialized]
    public const float MIN_CAMERA_ZOOM = 5f;

    [System.NonSerialized]
    public const float MAX_CAMERA_ZOOM = 25f;

    [System.NonSerialized]
    public const float MAX_CAMERA_X_POLE_ROTATION_IN_DEGREES = 68f;

    [System.NonSerialized]
    public const float MIN_CAMERA_X_POLE_ROTATION_IN_DEGREES = -10f;

    [SerializeField]
    float hipHeight = 1.0f; // Set in Unity editor, not here!

    [System.NonSerialized]
    public GameObject playerCamera;

    [System.NonSerialized]
    public bool isMouseDown = false;

    [System.NonSerialized]
    public bool isMouseWheelClickDown = false;

    [System.NonSerialized]
    public Vector3 lastMousePosition = new Vector3();

    [System.NonSerialized]
    public bool isMoving = false;

    [System.NonSerialized]
    public Tile moveTarget;

    [System.NonSerialized]
    public float walkSpeed = 7.5f;

    [System.NonSerialized]
    public float rotateSpeed = 7.5f;

    [System.NonSerialized]
    public List<Tile> currentWalkPath;

    [System.NonSerialized]
    public int walkPathIterator = 0;

    [System.NonSerialized]
    public float cameraYPoleRotation = 0f;

    [System.NonSerialized]
    public float cameraYPoleStandardMultiplier = 20f;

    [System.NonSerialized]
    public float cameraYPoleSensitivity = 0.5f;

    [System.NonSerialized]
    public float cameraXPoleRotation = 0f;

    [System.NonSerialized]
    public float cameraZPoleStandardMultiplier = 20f;

    [System.NonSerialized]
    public float cameraZPoleSensitivity = 0.5f;

    [System.NonSerialized]
    public float zoomChangeFactor = 1f;

    [System.NonSerialized]
    public float cameraZoom = 10f;

    [System.NonSerialized]
    public int currentTileLayer = 0;

    /**
     * The tile location this player spawns at
     */
    [System.NonSerialized]
    public TileLocation spawnLocation = new TileLocation(0, 78, -148);

    /**
     * The current tile the player is standing on
     */
    [System.NonSerialized]
    public Tile currentTile;

    /**
     * Raycasts to a tile. Returns null if no tile.
     */
    public static (GameObject, Vector3) RaycastToObject(Ray ray)
    {
        // How far the distance to the end location is. This will be changed by Raycast()
        float distance = 0;

        // Create a plane at a given intersection point that faces updwards
        // Then raycast to it
        Plane hPlane = new Plane(Vector3.up, new Vector3(0,-10,0));
        hPlane.Raycast(ray, out distance);

        if (distance <= 0.01f || distance > PlayerController.MAX_CLICK_RAYCAST_DISTANCE)
        {
            distance = PlayerController.MAX_CLICK_RAYCAST_DISTANCE;
        }

        // distance is now the distance to the desired plane
        RaycastHit hit;
        bool didHit = Physics.Raycast(ray.origin, ray.direction, out hit, distance);
        if (didHit)
        {
            return (hit.transform.gameObject, hit.point);
        }
        else
        {
            return (null, new Vector3());
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Animation anim = gameObject.transform.GetChild(0).GetComponent<Animation>();

        // Check for left mouse button
        if (Input.GetMouseButton(0))
        {
            this.isMouseDown = true;
        }
        else
        {
            if (this.isMouseDown)
            {
                this.isMouseDown = false;
                this.onMouseClick(Input.mousePosition);
            }
        }

        // Check for middle mouse button
        // lol is this if statement even needed
        // just set it to the return value of the condition you dunce - Garet for Garet
        if (Input.GetMouseButton(2))
        {
            if (!this.isMouseWheelClickDown)
            {
                this.isMouseWheelClickDown = true;
                this.lastMousePosition = Input.mousePosition;
            }

            Vector3 delta = Input.mousePosition - this.lastMousePosition;
            this.lastMousePosition = Input.mousePosition;

            // Was there movement?
            // VECTOR MAGNITUDE HEHSYASHSD YEAHH FUCK
            if (delta.magnitude > 0.01f)
            {
                this.cameraYPoleRotation += ((delta.x * Mathf.Deg2Rad) * this.cameraYPoleStandardMultiplier) * this.cameraYPoleSensitivity;
                this.cameraXPoleRotation += ((-delta.y * Mathf.Deg2Rad) * this.cameraZPoleStandardMultiplier) * this.cameraZPoleSensitivity;

                if (this.cameraXPoleRotation > 180f || this.cameraXPoleRotation < -180f)
                {
                    this.cameraXPoleRotation *= -1;
                }

                if (this.cameraYPoleRotation > 180f || this.cameraYPoleRotation < -180f)
                {
                    this.cameraYPoleRotation *= -1;
                }

                if (this.cameraXPoleRotation > PlayerController.MAX_CAMERA_X_POLE_ROTATION_IN_DEGREES)
                {
                    this.cameraXPoleRotation = MAX_CAMERA_X_POLE_ROTATION_IN_DEGREES;
                }else if (this.cameraXPoleRotation < PlayerController.MIN_CAMERA_X_POLE_ROTATION_IN_DEGREES)
                {
                    this.cameraXPoleRotation = MIN_CAMERA_X_POLE_ROTATION_IN_DEGREES;
                }
            }
        }
        else
        {
            this.isMouseWheelClickDown = false;
        }

        int scrollDelta = (int) Input.mouseScrollDelta.y;
        if (scrollDelta > 0)
        {
            this.cameraZoom -= this.zoomChangeFactor;
            if (this.cameraZoom < PlayerController.MIN_CAMERA_ZOOM)
            {
                this.cameraZoom = PlayerController.MIN_CAMERA_ZOOM;
            }
        }
        else if (scrollDelta < 0)
        {
            this.cameraZoom += this.zoomChangeFactor;
            if (this.cameraZoom > PlayerController.MAX_CAMERA_ZOOM)
            {
                this.cameraZoom = PlayerController.MAX_CAMERA_ZOOM;
            }
        }

        if (this.isMoving)
        {

            if (this.moveTarget == null)
            {
                if (this.currentWalkPath.Count > 0)
                {
                    this.moveTarget = this.currentWalkPath[this.walkPathIterator];
                }
                else
                {
                    // No walk path, but the player was "moving"
                    // Reset, they are not moving anymore
                    this.isMoving = false;
                    anim.clip = anim.GetClip("Armature|Idle");
                    anim.Play();
                }
            }

            if (this.moveTarget != null)
            {
                // Only move to the position if necessary
                Vector3 goal2D = new Vector3(this.moveTarget.tileCenterPosition.x, 0, this.moveTarget.tileCenterPosition.z);
                Vector3 current2D = new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z);
                if (Vector3.Distance(goal2D, current2D) < 0.05f)
                {
                    ++this.walkPathIterator;
                    this.currentTile = this.moveTarget;

                    if (this.walkPathIterator > this.currentWalkPath.Count - 1)
                    {
                        anim.clip = anim.GetClip("Armature|Idle");
                        anim.Play();

                        // Snap the player to the tile's standing position
                        gameObject.transform.position = this.getStandingPosition(this.moveTarget);

                        this.isMoving = false;
                        this.currentWalkPath = null;
                        this.walkPathIterator = 0;

                    }
                    else
                    {
                        this.moveTarget = this.currentWalkPath[this.walkPathIterator];
                    }
                }
                else
                {
                    anim.clip = anim.GetClip("Armature|NarutoRun");
                    anim["Armature|NarutoRun"].speed = 3.5f;
                    anim.Play();
                    float walkStep = this.walkSpeed * Time.deltaTime;
                    float rotateStep = this.rotateSpeed * Time.deltaTime;
                    Vector3 standingTarget = this.getStandingPosition(this.moveTarget);
                    gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, standingTarget, walkStep);

                    Vector3 newFacialDirection = (standingTarget - gameObject.transform.position);
                    newFacialDirection.y = 0;
                    Vector3 newForwardDirection = Vector3.RotateTowards(gameObject.transform.forward, newFacialDirection, rotateStep, 0.0f);
                    /*gameObject.transform.LookAt(standingTarget);
                    gameObject.transform.rotation = Quaternion.Euler(0f, gameObject.transform.rotation.eulerAngles.y, 0f);*/
                    /*gameObject.transform.rotation = Quaternion.RotateTowards(
                        gameObject.transform.rotation,
                        Quaternion.Euler(0f, 45f, 0f),
                        rotateStep
                    );*/
                    gameObject.transform.rotation = Quaternion.LookRotation(newForwardDirection);
                }
            }
        }

        // Update the camera's location anD Rutation
        Vector3 playerPosition = gameObject.transform.position;
        this.playerCamera.transform.position = playerPosition;
        this.playerCamera.transform.localRotation = Quaternion.AngleAxis(this.cameraYPoleRotation, new Vector3(0, 1, 0));
        this.playerCamera.transform.localRotation *= Quaternion.AngleAxis(this.cameraXPoleRotation, new Vector3(1, 0, 0));
        this.playerCamera.transform.position -= this.playerCamera.transform.forward * this.cameraZoom;
    }

    /**
     * Gets the Vector3 position that
     * the player will stand on any given tile
     */
    Vector3 getStandingPosition(Tile tileToStand)
    {
        // Get the common standing position for a tile
        // given vertex heights of that tile
        Vector3 tileStandPosition = tileToStand.GetStandingPosition();

        // Return the standing position plus the hip height of this character
        return tileStandPosition + new Vector3(0, this.hipHeight, 0);
    }

    /**
     * Quickly snap-teleports a player to the tile
     */
    public void teleportToTile(Tile tile)
    {
        this.currentTile = tile;
        gameObject.transform.position = this.getStandingPosition(tile);
    }

    void onTileClicked(Tile tileClicked)
    {
        AStarTilePathfinder pathfinder = new AStarTilePathfinder(this.currentTile, tileClicked);
        List<Tile> tilePath = pathfinder.getPathToEndTile();
        this.currentWalkPath = tilePath;
        this.isMoving = true;
        this.moveTarget = null;
        this.walkPathIterator = 0;
    }

    void onMouseClick(Vector3 mousePosition)
    {
        
        Ray mouseRay = this.playerCamera.GetComponent<Camera>().ScreenPointToRay(mousePosition);
        (GameObject objectClicked, Vector3 hitPoint) = PlayerController.RaycastToObject(mouseRay);
        if (Tile.isGameObjectATile(objectClicked))
        {
            this.onTileClicked(new Tile(objectClicked));
        }
        else
        {
            // Is the game object non-null and is tagged TileClickDetector?
            if (objectClicked != null)
            {
                if (objectClicked.tag == "TileClickDetector")
                {
                    // Find the tile that is centered at that relative location
                    int currentTileLayer = this.currentTile.tileLayer;
                    Vector3 nearestGridPosition = WorldGrid.worldPositionToNearestTileCenter(objectClicked.transform.position);
                    Tile clickedTile = WorldGrid.getTileAtWorldPositionOnLayer(nearestGridPosition, currentTileLayer);
                    if (clickedTile != null)
                    {
                        this.onTileClicked(clickedTile);
                    }
                }
            }
        }
    }
}
