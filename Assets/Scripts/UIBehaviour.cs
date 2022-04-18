using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject testPrefab = null;

    private const float DISTANCE = 0.3f;
    private const float FOTORESIZER = 0.0001f;
    private const float FRAMERESIZER = 0.01f;

    /*
    private const string TITLE = "Pictception";
    private const string EXTENSION = ".png";
    */

    private GameObject selected = null;
    private GameObject frame = null;
    private GameObject movingTarget = null;

    private float oldDistanceY = 0;
    private float distanceY = 0;

    private float digitalZoom = 1.0f;


    // Start is called before the first frame update
    void Start()
    {
        int min = Mathf.Min(Screen.height, Screen.width);
        frame = GameObject.Find("Frame");
        Image image = frame.GetComponent<Image>();
        image.rectTransform.sizeDelta = new Vector2(min, min);
        frame.SetActive(false);

        movingTarget = new GameObject("Moving Target");
        movingTarget.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        CheckSelect();
        CheckResize();
        CheckMoving();
    }

    private IEnumerator RecordFrame()
    {
        GameObject btnCamera = GameObject.Find("BtnCamera");
        GameObject btnClone = GameObject.Find("BtnClone");
        GameObject btnMove = GameObject.Find("BtnMove");
        GameObject btnTrash = GameObject.Find("BtnTrash");

        btnCamera.SetActive(false);
        btnClone.SetActive(false);
        btnMove.SetActive(false);
        btnTrash.SetActive(false);

        yield return new WaitForEndOfFrame();
        Texture2D screenShot = ScreenCapture.CaptureScreenshotAsTexture();

        btnCamera.SetActive(true);
        btnClone.SetActive(true);
        btnMove.SetActive(true);
        btnTrash.SetActive(true);

        GameObject photo = GameObject.Instantiate(testPrefab);
        
        Renderer renderer = photo.GetComponent<Renderer>();
        Material material = renderer.material;

        int minimum = (int) Mathf.Min(screenShot.width * digitalZoom, screenShot.height * digitalZoom);

        int centerX = screenShot.width / 2;
        int centerY = screenShot.height / 2;

        Texture2D texture = new Texture2D(minimum, minimum);

        for (int x = 0; x < minimum; x++)
        {
            for (int y = 0; y < minimum; y++)
            {
                texture.SetPixel(x, y, screenShot.GetPixel(x + centerX - minimum / 2, y + centerY - minimum / 2));
            }
        }

        Destroy(screenShot);
        screenShot = null;

        texture.Apply();
        material.mainTexture = texture;

        Arrange(photo);

        // cleanup
        // Object.Destroy(texture);
    }

    public void OnCameraPressed()
    {
        frame.SetActive(true);
    }

    public void OnCameraReleased()
    {
        frame.SetActive(false);
        StartCoroutine(RecordFrame());
    }

    public void OnCloneClick()
    {
        if (selected == null)
            return;

        GameObject photo = Instantiate(selected);
        Arrange(photo);
        photo.transform.GetChild(0).gameObject.SetActive(false);
    }

    public void OnMovePressed()
    {
        if (selected == null)
            return;

        movingTarget.SetActive(true);
        movingTarget.transform.position = selected.transform.position;
        movingTarget.transform.rotation = selected.transform.rotation;

        Camera mainCamera = Camera.current;
        movingTarget.transform.parent = mainCamera.transform;
    }

    public void OnMoveReleased()
    {
        if (selected == null)
            return;

        movingTarget.SetActive(false);
    }

    public void OnDeleteClick()
    {
        if (selected == null) return;

        Destroy(selected);
        selected = null;
    }

    /*
    public void OnSaveClick()
    {
        string date = System.DateTime.Now.ToString();
        date = date.Replace("/", "");
        date = date.Replace(" ", "");
        date = date.Replace(":", "");
        ScreenCapture.CaptureScreenshot(TITLE + date + EXTENSION);
    }
    */

    private void Arrange(GameObject photo)
    {
        Camera mainCamera = Camera.current;
        photo.transform.parent = mainCamera.transform;
        photo.transform.localPosition = new Vector3(0, 0, DISTANCE);
        photo.transform.localRotation = Quaternion.Euler(90, 180, 0);
        photo.transform.parent = null;
    }

    private void CheckSelect()
    {
        if (Input.touches.Length != 1)
            return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase != TouchPhase.Ended)
            return;

        Raycast raycast = new Raycast(touch.position);
        GameObject target = raycast.Target;

        if (target == null)
            return;

        if (selected != null)
        {
            selected.transform.GetChild(0).gameObject.SetActive(false);

            if (selected != target)
            {
                target.transform.GetChild(0).gameObject.SetActive(true);
                selected = target;
            }
            else
                selected = null;
        }
        else
        {
            target.transform.GetChild(0).gameObject.SetActive(true);
            selected = target;
        }
    }

    private void CheckResize()
    {
        if (Input.touchCount != 2)
            return;

        
        if (Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
        {
            if (oldDistanceY == 0)
            {
                oldDistanceY = Mathf.Abs(Input.GetTouch(0).position.y - Input.GetTouch(1).position.y);
                distanceY = oldDistanceY;
            }
            else
            {
                oldDistanceY = distanceY;
                distanceY = Mathf.Abs(Input.GetTouch(0).position.y - Input.GetTouch(1).position.y);

                if (selected == null)
                {
                    frame.SetActive(true);

                    if (distanceY > oldDistanceY && digitalZoom < 1f)
                    {
                        digitalZoom += FRAMERESIZER;
                    }
                    else if (distanceY < oldDistanceY && digitalZoom > 0f)
                    {
                        digitalZoom -= FRAMERESIZER;
                    }

                    float min = Mathf.Min(Screen.height, Screen.width);
                    Image image = frame.GetComponent<Image>();
                    image.rectTransform.sizeDelta = new Vector2(min * digitalZoom, min * digitalZoom);
                }
                else
                {
                    Vector3 scale = selected.transform.lossyScale;

                    if (distanceY > oldDistanceY)
                        selected.transform.localScale = new Vector3(scale.x * 1.0f + FOTORESIZER, scale.y * 1.0f + FOTORESIZER, scale.z * 1.0f + FOTORESIZER);
                    else
                        selected.transform.localScale = new Vector3(scale.x * 1.0f - FOTORESIZER, scale.y * 1.0f - FOTORESIZER, scale.z * 1.0f - FOTORESIZER);
                }
            }
        }
        else if (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(1).phase == TouchPhase.Ended)
            frame.SetActive(false);
    }

    private void CheckMoving()
    {
        if (selected == null) return;
        if (!movingTarget.activeSelf) return;

        Vector3 selectedPosition = selected.transform.position;
        Vector3 targetPosition = movingTarget.transform.position;
        float posX = (selectedPosition.x * 3 + targetPosition.x) / 4;
        float posY = (selectedPosition.y * 3 + targetPosition.y) / 4;
        float posZ = (selectedPosition.z * 3 + targetPosition.z) / 4;
        selected.transform.position = new Vector3(posX, posY, posZ);

        Quaternion selectedRotation = selected.transform.rotation;
        Quaternion targetRotation = movingTarget.transform.rotation;
        float rotX = (selectedRotation.x * 3 + targetRotation.x) / 4;
        float rotY = (selectedRotation.y * 3 + targetRotation.y) / 4;
        float rotZ = (selectedRotation.z * 3 + targetRotation.z) / 4;
        float rotW = (selectedRotation.w * 3 + targetRotation.w) / 4;
        selected.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);

    }
}
