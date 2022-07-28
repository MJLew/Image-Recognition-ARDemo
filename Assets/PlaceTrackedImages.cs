using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class PlaceTrackedImages : MonoBehaviour
{
    // Reference to AR tracked image manager component
    private ARTrackedImageManager _trackedImagesManager;

    // List of prefabs to instantiate
    public GameObject[] ARPrefabs;

    // Dictionary of created prefabs
    private readonly Dictionary<string, GameObject> _instantiatedPrefabs = new Dictionary<string, GameObject>();

    private void Awake()
    {
        // Cache a reference to the Tracked Image Manager component
        _trackedImagesManager = GetComponent<ARTrackedImageManager>();
    }

    private void OnEnable()
    {
        // Attach event handler when tracked images change, so it calls the function every time the images in the scene changes
        _trackedImagesManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        // Remove event handler
        _trackedImagesManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // When a new recognizeable image has been added to the scene

        // Loop through all new tracked images that have been detected
        foreach (var trackedImage in eventArgs.added)
        {
            // Get the name of the reference image
            var imageName = trackedImage.referenceImage.name;
            // Now loop over the array of prefabs
            foreach (var curPrefab in ARPrefabs)
            {
                // Check whether this prefab matches the tracked image name, and that the prefab hasn't already been created
                if (string.Compare(curPrefab.name, imageName, System.StringComparison.OrdinalIgnoreCase) == 0 && !_instantiatedPrefabs.ContainsKey(imageName))
                {
                    //Instantiate the prefab, parenting it to the ARTrackedImage
                    var newPrefab = Instantiate(curPrefab, trackedImage.transform);
                    //Add the created prefab to our array
                    _instantiatedPrefabs[imageName] = newPrefab;
                }
            }
        }

        // The existing tracked images have been updated in the scene
        foreach (var trackedImage in eventArgs.updated)
        {
            // Set the prafab to be active or not, depending on whether the image is being tracked
            _instantiatedPrefabs[trackedImage.referenceImage.name].SetActive(trackedImage.trackingState == TrackingState.Tracking);
        }

        // If the AR subsystem has given up on looking for the tracked image
        foreach (var trackedImage in eventArgs.removed)
        {
            // Destroy the prefab
            Destroy(_instantiatedPrefabs[trackedImage.referenceImage.name]);
            // Remove the inastance from the array
            _instantiatedPrefabs.Remove(trackedImage.referenceImage.name);
            // or you can simply set the prefab to inactive
            // _instantiatedPrefabs(trackedImage.referenceImage.name].SetActive(false);
        }
    }
}
