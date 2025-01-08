using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private Dictionary<Vector2, Tile> Tiles { get; set; }
    
    [Header("Tile Properties")]
    [SerializeField] [Range(0, 10)] public int width;
    [SerializeField] [Range(0, 10)] public int height;
    [SerializeField] private Tile tilePrefab;

    private void Start()
    {
        GenerateGridsInGame();
    }

    private void GenerateGridsInGame()
    {
        Tiles = new Dictionary<Vector2, Tile>();

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                CreateTileInGame(x, y);
            }
        }

    }
    private void CreateTileInGame(int x, int y)
    {
        var tile = Instantiate(tilePrefab, transform);
        
        tile.transform.position = new Vector3(x, 0.06f, y);
        tile.x = x;
        tile.y = y;
        Tiles.Add(new Vector2(x, y), tile);
        
    }
    
}
