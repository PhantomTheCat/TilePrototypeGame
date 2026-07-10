using UnityEngine;

public class CameraManager : MonoBehaviour
{
    //Properties
    public static CameraManager Instance;
    [HideInInspector] public Camera MainCamera;

    [Header("Camera Settings")]
    public Vector3 CharacterOffset = new Vector3(0, 1, -10);

    //Methods
    private void Awake()
    {
        Instance = this;
        MainCamera = Camera.main;
        MainCamera.transform.position = CharacterOffset;
    }

    public void UpdateCameraParent(Transform newParent)
    {
        MainCamera.transform.parent = newParent;
        MainCamera.transform.position = newParent.position + CharacterOffset;
    }
}
