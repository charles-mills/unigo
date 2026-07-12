using System;
using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using UnityEngine;
using World;

public class UniStopDataRecord : MonoBehaviour
{
    public GameObject uniStopPrefab;
    public GameObject gameMap;
    public TextAsset filePath;

    public Camera arCamera;

    private string stopInformationParsed = "";
    private UniStopItems stopStoredInformation;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (filePath == null)
        {
            Debug.LogWarning("Could not find the file path to UniStopLocations.json");
            return;
        }

        stopInformationParsed = filePath.text;
        Debug.Log("Loaded in the UniStopLocations.json file");

        var parsedJsonFile = JsonUtility.FromJson<JsonFileWrapper>(stopInformationParsed);

        foreach (var currentUniStop in parsedJsonFile.unistops)
        {
            var uniStopToPlace = Instantiate(uniStopPrefab, gameMap.transform);

            var arcLocation = uniStopToPlace.GetComponent<ArcGISLocationComponent>();
            arcLocation.Position = new ArcGISPoint(
                currentUniStop.longitude,
                currentUniStop.latitude,
                0,
                new ArcGISSpatialReference(4326)
            );

            arcLocation.Rotation = new ArcGISRotation(0, 90, 0);

            var grounded = uniStopToPlace.GetComponent<GroundedObject>();
            if (grounded != null) grounded.SnapToGround();

            var filler = uniStopToPlace.GetComponentInChildren<UniStopInformationFiller>();
            if (filler != null) filler.Initialise(currentUniStop.name, currentUniStop.description);
        }
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        var ray = arCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit)) return;
        var filler = hit.transform.GetComponentInChildren<UniStopInformationFiller>();
        if (filler == null) return;

        filler.Toggle();
    }

    [Serializable]
    public struct UniStopItems
    {
        public string id;
        public double latitude;
        public double longitude;
        public string name;
        public string description;
    }


    [Serializable]
    public struct JsonFileWrapper
    {
        public UniStopItems[] unistops;
    }
}