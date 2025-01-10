using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "CubeGrowthRules", menuName = "Rules/CubeGrowthRules")]
public class GrowthRulesData : ScriptableObject
{
    [SerializeField] private SerializedDictionary<CubeLocation, GrowthRule> growthRules = 
        new SerializedDictionary<CubeLocation, GrowthRule>();
    [SerializeField] private SerializedDictionary<CubeLocation, LocationChangeRule> locationTypeChangeRules = 
        new SerializedDictionary<CubeLocation, LocationChangeRule>();

    public void CheckAndApplyGrowth(Dictionary<CubeLocation, Cube> locationToCubeMap)
    {
        var cubeLocations = Enum.GetValues(typeof(CubeLocation));
        bool canGrow;
        var index = 2;
        do
        {
            var currentLocationType = (CubeLocation)cubeLocations.GetValue(index);
            Debug.Log($"i =  + {index}, CheckType = {currentLocationType}");
            canGrow = TryFindGrowthSlot(currentLocationType, out var targetGrowthLocation, locationToCubeMap);
        
            if (canGrow)
            {
                var growthRule = growthRules[currentLocationType];
                var targetPositionMultiplier = growthRule.PositionMultiplierMap[targetGrowthLocation];
                var targetScaleMultiplier  = growthRule.ScaleMultiplierMap[targetGrowthLocation];
                var animationDuration = 0.3f;
        
                if (locationToCubeMap.TryGetValue(currentLocationType, out var cube))
                {
                    var currentScale = cube.transform.localScale;
                    var currentPosition = cube.transform.localPosition;
                    
                    var targetScale = new Vector3(
                        currentScale.x * targetScaleMultiplier.x,
                        currentScale.y * targetScaleMultiplier.y,
                        currentScale.z * targetScaleMultiplier.z
                    );
                    var targetPosition = new Vector3(
                        currentPosition.x * targetPositionMultiplier.x,
                        currentPosition.y * targetPositionMultiplier.y,
                        currentPosition.z * targetPositionMultiplier.z
                    );

                    if (cube.gameObject.activeSelf)
                    {
                        cube.AnimateGrowing(targetScale, targetPosition, animationDuration);
                        //TODO Yeni Location Types atansın
                        //TODO tekrar destroy olacak cube var mı diye bakılsın
                        
                        locationToCubeMap.TryGetValue(targetGrowthLocation, out var cubeToDestroy);
                        if (cubeToDestroy && !cubeToDestroy.gameObject.activeSelf)
                            cubeToDestroy.DestroyCube();
                    }
                }
            }
            else
            {
                if (targetGrowthLocation == CubeLocation.Null)
                {
                    locationToCubeMap.TryGetValue(currentLocationType, out var boxToDestroy);
                    if (boxToDestroy && !boxToDestroy.gameObject.activeSelf)
                    {
                        boxToDestroy.GetComponentInParent<BoxController>().DestroyBox();
                        break;
                    }
                }
            }
        
            index++;
        } while (!canGrow && index < cubeLocations.Length);
        
    }
    private bool TryFindGrowthSlot(CubeLocation currentLocation, out CubeLocation availableLocation, Dictionary<CubeLocation, Cube> locationToCubeDict)
    {
        availableLocation = CubeLocation.Default;
        if (!growthRules.TryGetValue(currentLocation, out var growthRule))
        {
            Debug.Log($"Growth rules not found for slot: {currentLocation}");
            return false;
        }

        if (!locationToCubeDict.Keys.Contains(currentLocation)) return false;
        
        Debug.Log("growthRule.CheckLocations.Count: " + growthRule.CheckLocations.Count);
        foreach (var location in growthRule.CheckLocations)
        {
            if (location == CubeLocation.Null)
            {
                availableLocation = location;
                break;
            }
            
            Debug.Log($"Location: {location}");
            locationToCubeDict.TryGetValue(location, out var cube);
            if (cube && !cube.gameObject.activeSelf)
            {
                availableLocation = location;
                Debug.Log("Available Slot: " + availableLocation);
                return true;
            }
        }
        return false;
    }
    

    [Serializable]
    public class GrowthRule
    {
        [SerializeField] private List<CubeLocation> checkLocations = new List<CubeLocation>();
        [SerializeField] private SerializedDictionary<CubeLocation, Vector3> positionMultiplierMap = 
            new SerializedDictionary<CubeLocation, Vector3>();
        [SerializeField] private SerializedDictionary<CubeLocation, Vector3> scaleMultiplierMap = 
            new SerializedDictionary<CubeLocation, Vector3>();

        public List<CubeLocation> CheckLocations => checkLocations;
        public SerializedDictionary<CubeLocation, Vector3> PositionMultiplierMap => positionMultiplierMap;
        public SerializedDictionary<CubeLocation, Vector3> ScaleMultiplierMap => scaleMultiplierMap;
    }
    [Serializable]
    public class LocationChangeRule
    {
        [SerializeField] private SerializedDictionary<CubeLocation, CubeLocation> newLocationMap =
            new SerializedDictionary<CubeLocation, CubeLocation>();
        public SerializedDictionary<CubeLocation, CubeLocation> NewLocationMap => newLocationMap;

    }
}
