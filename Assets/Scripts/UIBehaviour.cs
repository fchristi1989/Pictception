using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject testPrefab = null;

    private const float DISTANCE = 0.3f;
    private const float RESIZER = 0.0001f;

    /*
    private const string TITLE = "Pictception";
    private const string EXTENSION = ".png";
    */

    private GameObject selected = null;
    private GameObject frame = null;

    private float oldDistanceY = 0;
    private float distanceY = 0;


    // Start is called before the first frame update
    void Start()
    {
        int min = Mathf.Min(Screen.height, Screen.width);
        frame = GameObject.Find("Frame");
        Image image = frame.GetComponent<Image>();
        image.rectTransform.sizeDelta = new Vector2(min, min);
        frame.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        CheckSelect();
        CheckResize();
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
        int minimum = Mathf.Min(screenShot.width, screenShot.height);
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

        Camera mainCamera = Camera.current;
        selected.transform.parent = mainCamera.transform;
    }

    public void OnMoveReleased()
    {
        if (selected == null)
            return;

        selected.transform.parent = null;
    }

    public void OnDeleteClick()
    {
        if (selected == null)
            return;

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
        if (selected == null)
            return;

        if (Input.touchCount == 2)
        {

            if (Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
            {
                if (oldDistanceY == 0)
                {
                    oldDistanceY = Mathf.Abs(Input.GetTouch(0).position.y - Input.GetTouch(1).position.y);
                    distanceY = oldDistanceY;
                }
                else
                {
                    Vector3 scale = selected.transform.lossyScale;
                    oldDistanceY = distanceY;
                    distanceY = Mathf.Abs(Input.GetTouch(0).position.y - Input.GetTouch(1).position.y);

                    if (distanceY > oldDistanceY)
                        selected.transform.localScale = new Vector3(scale.x * 1.0f + RESIZER, scale.y * 1.0f + RESIZER, scale.z * 1.0f + RESIZER);
                    else
                        selected.transform.localScale = new Vector3(scale.x * 1.0f - RESIZER, scale.y * 1.0f - RESIZER, scale.z * 1.0f - RESIZER);
                }
            }
        }
    }
}
