using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;
    public bool hasObstacle;
    public bool isFull = false;

    public void SetTileFull(bool isFull)
    {
        this.isFull = isFull;
    }
    
}
