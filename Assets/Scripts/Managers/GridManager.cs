using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Dictionary<Vector2, Tile> Tiles { get; set; }
    
    [Header("Tile Properties")]
    [SerializeField] [Range(0, 10)] public int width;
    [SerializeField] [Range(0, 10)] public int height;
    [SerializeField] [Range(0, 2)] public int gridStep = 1; //Distance between 2 tiles
    [SerializeField] private Tile tilePrefab;
    

    public static GridManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }
    
    private void Start()
    {
        GenerateGridsInGame();
    }

    private void GenerateGridsInGame()
    {
        Tiles = new Dictionary<Vector2, Tile>();

        for (var x = 0; x < width; x += gridStep)
        {
            for (var y = 0; y < height; y += gridStep)
            {
                CreateTileInGame(x, y);
            }
        }

    }
    private void CreateTileInGame(int x, int y)
    {
        var tile = Instantiate(tilePrefab, transform);
        
        tile.transform.position = new Vector3(x, 0.6f, y);
        tile.x = x;
        tile.y = y;
        Tiles.Add(new Vector2(x, y), tile);
        
    }
    
}
