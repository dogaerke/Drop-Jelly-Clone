using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "NeighborCheckRules", menuName = "Rules/CheckRules")]
public class NeighborCheckRulesData : ScriptableObject
{
    [SerializeField] private SerializedDictionary<BoxLocationForNeighbors, NeighborCheckRules> neighborCheckList =
        new SerializedDictionary<BoxLocationForNeighbors, NeighborCheckRules>();
    
    public SerializedDictionary<BoxLocationForNeighbors, NeighborCheckRules> NeighborCheckList => neighborCheckList;

    
    [Serializable]
    public class NeighborCheckRules
    {
        [SerializeField] private SerializedDictionary<CubeLocation, Location>  mainCheckList = 
            new SerializedDictionary<CubeLocation, Location> ();
        
        public SerializedDictionary<CubeLocation, Location> MainCheckList => mainCheckList;

    }
    
    [Serializable]
    public class Location
    {
        [SerializeField] private List<CubeLocation> loc = new List<CubeLocation>();
        public List<CubeLocation> Loc => loc;
    }
    
}


