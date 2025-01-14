using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "CubeGrowthRules", menuName = "Rules/CubeGrowthRules")]
public class GrowthRulesData : ScriptableObject
{
    [SerializeField] private SerializedDictionary<CubeLocation, GrowthRule> growthRules = 
        new SerializedDictionary<CubeLocation, GrowthRule>();
    [SerializeField] private SerializedDictionary<LocationChangeRule, CubeLocation> locationTypeChangeRules = 
        new SerializedDictionary<LocationChangeRule, CubeLocation>();
    
    
    public void CheckAndApplyGrowth(ref Dictionary<CubeLocation, Cube> locationToCubeMap, CubeLocation locationToDestroy)
    {
        var cubeLocations = Enum.GetValues(typeof(CubeLocation));
        bool canGrowOrDestroy;
        var index = 2;
        do
        {
            var currentLocationType = (CubeLocation)cubeLocations.GetValue(index);
            //Debug.Log($"i =  + {index}, CheckType = {currentLocationType}");
            
            canGrowOrDestroy = TryFindGrowthSlot(currentLocationType, out var targetGrowthLocation, locationToCubeMap);
            
            if (canGrowOrDestroy)
            {
                if (targetGrowthLocation == CubeLocation.Null)
                {
                    locationToCubeMap.TryGetValue(currentLocationType, out var cubeToDestroy);
                    if (cubeToDestroy)
                    {
                        cubeToDestroy.CloseCube();
                    }
                }
                
                else if (locationToCubeMap.TryGetValue(currentLocationType, out var cube))
                {
                    var growthRule = growthRules[currentLocationType];
                    var targetPositionMultiplier = growthRule.PositionMultiplierMap[targetGrowthLocation];
                    var targetScaleMultiplier  = growthRule.ScaleMultiplierMap[targetGrowthLocation];
                    
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
                        cube.AnimateGrowing(targetScale, targetPosition);
                        if (TryChangeLocationType(ref locationToCubeMap, cube, currentLocationType, targetGrowthLocation))
                        {
                            cube.parentBox.WaitAndControlNeighbors();
                        }
                    }
                    locationToCubeMap.TryGetValue(locationToDestroy, out var cubeToDestroy);
                    if (cubeToDestroy && !cubeToDestroy.gameObject.activeSelf)
                    {
                        cubeToDestroy.DestroyCube();
                    }
                    
                }
            }
            
            index++;
        } while (!canGrowOrDestroy && index < cubeLocations.Length);
        
    }
    
    private bool TryFindGrowthSlot(CubeLocation currentLocation, out CubeLocation availableLocation, Dictionary<CubeLocation, Cube> locationToCubeDict)
    {
        availableLocation = CubeLocation.Default;
        if (!growthRules.TryGetValue(currentLocation, out var growthRule))
        {
            return false;
        }

        if (!locationToCubeDict.Keys.Contains(currentLocation)) return false;
        
        foreach (var location in growthRule.CheckLocations)
        {
            if (location == CubeLocation.Null)
            {
                availableLocation = location;
                return true;
            }

            locationToCubeDict.TryGetValue(location, out var cube);
            
            if (cube && !cube.gameObject.activeSelf)
            {
                availableLocation = location;
                return true;
            }
                
        }
        return false;
    }

    private bool TryChangeLocationType(ref Dictionary<CubeLocation, Cube> locationToCubeMap, 
        Cube cube, CubeLocation currentLocationType,CubeLocation targetGrowthLocation)
    {
        Debug.Log(locationTypeChangeRules.Count);
        foreach (var rule in locationTypeChangeRules)
        {
            var ruleKeyToControl = new KeyValuePair<CubeLocation, CubeLocation>(currentLocationType, targetGrowthLocation);
            
            var ruleKey = rule.Key;
            var newCubeLocation = rule.Value;
            var newLocationMap = ruleKey.NewLocationMap;

            foreach (var mapRule in newLocationMap)
            {
                //Debug.Log("maprulekey" + mapRule.Key + "maprulevalue" + mapRule.Value);
                if (mapRule.Equals(ruleKeyToControl))
                {
                    //Debug.Log("OldLocType: " + cube.cubeLocation);
                    locationToCubeMap.Remove(cube.cubeLocation);

                    cube.cubeLocation = newCubeLocation;
                    locationToCubeMap[cube.cubeLocation] = cube;
                    //Debug.Log("NewLocType: " + cube.cubeLocation);
                    return true;
                }
            }
            
        }
        //Debug.Log("CannotChangeLoc");
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

        [SerializeField] private List<CubeLocation> adjacentParentLocations;
        [SerializeField] private List<CubeLocation> childLocations;

        public List<CubeLocation> CheckLocations => checkLocations;
        public SerializedDictionary<CubeLocation, Vector3> PositionMultiplierMap => positionMultiplierMap;
        public SerializedDictionary<CubeLocation, Vector3> ScaleMultiplierMap => scaleMultiplierMap;
        public List<CubeLocation> AdjacentParentLocations => adjacentParentLocations;
        public List<CubeLocation> ChildLocations => childLocations;

    }
    [Serializable]
    public class LocationChangeRule
    {
        [SerializeField] private SerializedDictionary<CubeLocation, CubeLocation> newLocationMap =
            new SerializedDictionary<CubeLocation, CubeLocation>();

        public SerializedDictionary<CubeLocation, CubeLocation> NewLocationMap
        {
            get => newLocationMap;
            set => newLocationMap = value;
        }

    }
}
