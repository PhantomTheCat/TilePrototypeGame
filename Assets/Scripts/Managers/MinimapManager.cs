using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapManager : MonoBehaviour
{
    //Properties
    public static MinimapManager Instance;

    [Header("References")]
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private RawImage minimapImage;
    [SerializeField] private GameObject heroTrackerPrefab;

    [Header("Settings")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 0, -30);
    [SerializeField] private Vector3 heroTrackerOffset = new Vector3(0, 0, -25);
    [SerializeField] private int minimapSize = 256;
    [SerializeField] private int distanceToInclude = 150;


    //Methods
    private void Awake()
    {
        Instance = this;
    }

    public void TakePicture()
    {
        //Prepare the minimap snapshot
        if (minimapImage == null || minimapCamera == null) return;
        List<GameObject> existingTrackers = new List<GameObject>();
        minimapCamera.enabled = true;
        minimapCamera.transform.position = UnitManager.Instance.SelectedHero.transform.position + cameraOffset;

        GridManager gridManager = GridManager.Instance;
        gridManager.PrepareMinimapSnapshot(distanceToInclude);

        if (heroTrackerPrefab != null)
        {
            foreach (BaseHero hero in UnitManager.Instance.Heroes)
            {
                if (hero == null) continue;
                GameObject heroTracker = Instantiate(heroTrackerPrefab, minimapCamera.transform);
                heroTracker.transform.position = hero.transform.position + heroTrackerOffset;
                existingTrackers.Add(heroTracker);
            }
        }

        RenderTexture renderTexture = new RenderTexture(minimapSize, minimapSize, 16);
        renderTexture.Create();

        minimapCamera.targetTexture = renderTexture;
        minimapCamera.Render();
        minimapImage.texture = renderTexture;

        //Set things back to default
        for (int i = 0; i < existingTrackers.Count; i++)
        {
            Destroy(existingTrackers[i]);
        }
        gridManager.UpdateVision();
        minimapCamera.enabled = false;
    }
}
