using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "CubeGrowthRules", menuName = "Rules/CubeGrowthRules")]
public class CubeGrowthRulesSO : ScriptableObject
{
    [SerializeField] private SerializedDictionary<CubeLocation, GrowthRule> growthRules = new SerializedDictionary<CubeLocation, GrowthRule>();

    public void CheckAndApplyGrowth(Dictionary<CubeLocation, Cube> locationToCubeDict)
    {
        var cubeLocations = Enum.GetValues(typeof(CubeLocation));
        bool canGrowth;
        var i = 1;
        do
        {
            var checkType = (CubeLocation)cubeLocations.GetValue(i);
            Debug.Log($"i =  + {i}, CheckType = {checkType}");
            canGrowth = TryGetGrowthSlot(checkType, out var growthSlot, locationToCubeDict);
        
            if (canGrowth)
            {
                var slotRule = growthRules[checkType];
                var growthPosition = slotRule.SlotPositionMap[growthSlot];
                var growthScale = slotRule.SlotScaleMap[growthSlot];
                const float duration = 0.5f;
        
                if (locationToCubeDict.TryGetValue(checkType, out var cube))
                {
                    var localScale = cube.transform.localScale;
                    var localPosition = cube.transform.localPosition;
        
                    var scaleX = localScale.x * growthScale.x;
                    var scaleY = localScale.y * growthScale.y;
                    var scaleZ = localScale.z * growthScale.z;
                    var targetScale = new Vector3(scaleX, scaleY, scaleZ);
        
                    var posX = localPosition.x * growthPosition.x;
                    var posY = localPosition.y * growthPosition.y;
                    var posZ = localPosition.z * growthPosition.z;
                    var targetPosition = new Vector3(posX, posY, posZ);

                    if (cube.gameObject.activeSelf)
                    {
                        cube.AnimateGrowing(targetScale, targetPosition, duration);
                        
                    }
                }
            }
        
            i++;
        } while (!canGrowth && i < cubeLocations.Length);
        
    }
    private bool TryGetGrowthSlot(CubeLocation targetLocation, out CubeLocation availableLocation, Dictionary<CubeLocation, Cube> locationToCubeDict)
    {
        availableLocation = CubeLocation.Default;
        if (!growthRules.TryGetValue(targetLocation, out var growthRule))
        {
            Debug.Log($"Growth rules not found for slot: {targetLocation}");
            return false;
        }

        Debug.Log("growthRule.CheckLocations.Count: " + growthRule.CheckLocations.Count);
        foreach (var location in growthRule.CheckLocations)
        {
            Debug.Log($"Location: {location}");
            locationToCubeDict.TryGetValue(location, out var cube);
            if (!cube.gameObject.activeSelf)
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
        [SerializeField] private SerializedDictionary<CubeLocation, Vector3> slotPositionMap = new SerializedDictionary<CubeLocation, Vector3>();
        [SerializeField] private SerializedDictionary<CubeLocation, Vector3> slotScaleMap = new SerializedDictionary<CubeLocation, Vector3>();

        public List<CubeLocation> CheckLocations => checkLocations;
        public SerializedDictionary<CubeLocation, Vector3> SlotPositionMap => slotPositionMap;
        public SerializedDictionary<CubeLocation, Vector3> SlotScaleMap => slotScaleMap;
    }
}
