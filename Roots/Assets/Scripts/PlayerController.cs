using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private GameObject gameController;

    public bool mouseControl;

    public float playerSpeed = 2f;

    public GameObject trail;

    private Camera playerCamera;

    private float leftBounds;
    private float rightBounds;

    private float leftCameraBounds;
    private float rightCameraBounds;

    private float maxDepth;

    private Vector2 resolution;

    // Start is called before the first frame update
    void Start()
    {
        resolution = new Vector2(Screen.width, Screen.height);
        GameObject cameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        //Debug.Log(playerCamera);
        playerCamera = cameraObject.GetComponent<Camera>();
        //leftBounds = -5f;
        //rightBounds = 5f;
        //recalculateBounds();

        gameController = GameObject.FindGameObjectWithTag("GameController");
        //LevelController controller = gameController.GetComponent<LevelController>();
        //controller.speedUpgradeListener.AddListener(onSpeedUpgrade);
        setSpeedUpgrade(LevelController.getSpeedUpgradeValue());
        //controller.visionUpgradeListener.AddListener(onVisionUpgrade);
        setVisionUpgrade(LevelController.getVisionUpgradeValue());

        switch (LevelController.getdrillUpgradeValue())
        {
            case 0:
                maxDepth = gameController.GetComponent<WorldGen>().partialStoneDepth - 1;
                break;
            case 1:
                maxDepth = gameController.GetComponent<WorldGen>().StoneDepth - 1;
                break;
            case 2:
                maxDepth = gameController.GetComponent<WorldGen>().endGameDepth + 5;
                break;
            default:
                Debug.LogError("drill upgrade value not in range");
                maxDepth = 5;
                break;
        }
        maxDepth = -maxDepth;
    }

    // Update is called once per frame
    void Update()
    {
        if (mouseControl)
            processMouseMovement();
        else
            processKeyboardMovement();

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, leftBounds, rightBounds),
            Mathf.Clamp(transform.position.y, maxDepth, 0.0f),
            transform.position.z
            );

        // Make camera follow player
        playerCamera.transform.position =
            new Vector3(Mathf.Clamp(transform.position.x, leftCameraBounds, rightCameraBounds),
            transform.position.y,
            -10f);
        
        if ((resolution.x != Screen.width || resolution.y != Screen.height))
        {
            resolution = new Vector2(Screen.width, Screen.height);
            recalculateBounds();
        }
    }

    // Temporary movement algorithm for testing
    void processKeyboardMovement()
    {
        Vector3 movement = new Vector3();

        if (Input.GetKey(KeyCode.DownArrow))
        {
            movement += new Vector3(0, -1);
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            movement += new Vector3(0, 1);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            movement += new Vector3(-1, 0);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            movement += new Vector3(1, 0);
        }

        transform.position += movement.normalized * playerSpeed * Time.deltaTime;
    }

    void processMouseMovement()
    {
        Vector3 mousePos = playerCamera.ScreenToWorldPoint(Input.mousePosition);

        Vector3 movement = mousePos - transform.position;

        movement.z = 0f;

        transform.position += movement.normalized * playerSpeed * Time.deltaTime;
    }

    void recalculateBounds()
    {
        float vertExtent = playerCamera.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;
        int tileWidth = GameObject.FindGameObjectWithTag("GameController").GetComponent<WorldGen>().tileWidth;
        float bound = Mathf.Max(tileWidth / 2 - horzExtent, 0f);
        leftCameraBounds = -bound;
        rightCameraBounds = bound;
        leftBounds = -tileWidth / 2;
        rightBounds = tileWidth / 2;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision);
        GameObject other = collision.gameObject;
        if (other.CompareTag("Nutrient"))
        {
            gameController.GetComponent<LevelController>().nutrientCount++;
            Destroy(other);
            LevelController.upgradeSpeed();
        }
        else if (other.CompareTag("Water"))
        {
            gameController.GetComponent<LevelController>().waterCount++;
            Destroy(other);
            LevelController.upgradeVision();
        }
    }

    private void FixedUpdate()
    {
        Instantiate(trail, transform.position, transform.rotation);
    }

    public void setSpeedUpgrade(float newSpeed)
    {
        playerSpeed = newSpeed;
    }

    public void setVisionUpgrade(float newVision)
    {
        playerCamera.orthographicSize = newVision;
        recalculateBounds();
    }
}
