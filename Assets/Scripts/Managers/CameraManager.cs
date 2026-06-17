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
    }

    private void LateUpdate()
    {
        FollowSelectedCharacter();

        //TODO: Add dynamic camera movement and zooming based on player actions and room size, if necessary.
    }

    private void FollowSelectedCharacter()
    {
        MainCamera.transform.position = UnitManager.Instance.SelectedHero.transform.position + CharacterOffset;
    }
}
