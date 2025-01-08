using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BoxManager : MonoBehaviour
{
    public GameObject cubePrefab;  
    public Transform startPos;
    private Transform _box;
    
    private float _boxSize = 1f;

    private void Start()
    {
        var randomCombination = (BoxCombination)UnityEngine.Random.Range(1, 2);
        CreateBox(randomCombination);
        CreateBox(randomCombination);
    }

    private void CreateBox(BoxCombination combination)
    {
        switch (combination)
        {
            case BoxCombination.FourCubes:
                CreateFourCubes();
                break;

            case BoxCombination.TwoCubesOnePrism:
                CreateTwoCubesOnePrism();
                break;

            case BoxCombination.TwoPrisms:
                CreateTwoPrisms();
                break;

            case BoxCombination.OneBigCube:
                CreateOneBigCube();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(combination), combination, null);
        }
        
        
    }
    
    private void CreateFourCubes()
    {
        var cubeSize = _boxSize / 2f;
        var box = new GameObject("Box");
        box.transform.position = new Vector3(startPos.position.x, 0, startPos.position.z);
        box.transform.SetParent(transform);
        
        for (var x = 0; x < 2; x++)
        {
            for (var z = 0; z < 2; z++)
            {
                var position = new Vector3(x * cubeSize - cubeSize / 2, 
                    0, z * cubeSize - cubeSize / 2);
                
                CreateObject(cubePrefab, position, Vector3.one * cubeSize, box.transform);
            }
        }
    }
    
    private void CreateTwoCubesOnePrism()
    {
        var cubeSize = _boxSize / 2f;
        var box = new GameObject("Box");
        box.transform.position = new Vector3(startPos.position.x, 0, startPos.position.z);
        box.transform.SetParent(transform);

        var randPos = UnityEngine.Random.Range(0,4);
        switch (randPos)
        {
            case 0:
                SpawnVerticalPrismRight(cubeSize, box);
                break;
            case 1:
                SpawnVerticalPrismLeft(cubeSize, box);
                break;
            case 2:
                SpawnHorizontalPrismUp(cubeSize, box);
                break;
            case 3:
                SpawnHorizontalPrismDown(cubeSize, box);
                break;
        }
    }

    private void SpawnVerticalPrismRight(float cubeSize, GameObject box)
    {
        // Right Vertical Prism 
        CreateObject(cubePrefab, new Vector3(cubeSize / 2, 0, 0), 
            new Vector3(cubeSize, cubeSize , cubeSize * 2), box.transform);
        
        // Cubes
        CreateObject(cubePrefab, new Vector3(-cubeSize / 2, 0, cubeSize / 2),
            Vector3.one * cubeSize, box.transform);  // Left Up Cube
        CreateObject(cubePrefab, new Vector3(-cubeSize / 2, 0, -cubeSize / 2), 
            Vector3.one * cubeSize, box.transform); // Left Down Cube
    }
    private void SpawnVerticalPrismLeft(float cubeSize, GameObject box)
    {
        // Left Vertical Prism 
        CreateObject(cubePrefab, new Vector3(-cubeSize / 2, 0, 0), 
            new Vector3(cubeSize, cubeSize , cubeSize * 2), box.transform);
        
        // Cubes
        CreateObject(cubePrefab, new Vector3(cubeSize / 2, 0, cubeSize / 2),
            Vector3.one * cubeSize, box.transform);  // Right Up Cube
        CreateObject(cubePrefab, new Vector3(cubeSize / 2, 0, -cubeSize / 2), 
            Vector3.one * cubeSize, box.transform); // Right Down Cube
    }
    private void SpawnHorizontalPrismUp(float cubeSize, GameObject box)
    {
        // Horizontal Up Prism 
        CreateObject(cubePrefab, new Vector3(0, 0, cubeSize / 2), 
            new Vector3(cubeSize * 2, cubeSize , cubeSize), box.transform);
        
        // Cubes
        CreateObject(cubePrefab, new Vector3(-cubeSize / 2, 0, -cubeSize / 2), 
            Vector3.one * cubeSize, box.transform); // Left Down Cube
        CreateObject(cubePrefab, new Vector3(cubeSize / 2, 0, -cubeSize / 2),
            Vector3.one * cubeSize, box.transform);  // Right Down Cube
    }
    private void SpawnHorizontalPrismDown(float cubeSize, GameObject box)
    {
        // Horizontal Down Prism 
        CreateObject(cubePrefab, new Vector3(0, 0, -cubeSize / 2), 
            new Vector3(cubeSize * 2, cubeSize , cubeSize), box.transform);
        
        // Cubes
        CreateObject(cubePrefab, new Vector3(-cubeSize / 2, 0, cubeSize / 2), 
            Vector3.one * cubeSize, box.transform); // Left Up Cube
        CreateObject(cubePrefab, new Vector3(cubeSize / 2, 0, cubeSize / 2),
            Vector3.one * cubeSize, box.transform);  // Right Up Cube
    }


    private void CreateTwoPrisms()
    {
        var cubeSize = _boxSize / 2f;
        var box = new GameObject("Box");
        box.transform.position = new Vector3(startPos.position.x, 0, startPos.position.z);
        box.transform.SetParent(transform);

        var randPos = UnityEngine.Random.Range(0,2);
        switch (randPos)
        {
            case 0:
                SpawnVerticalPrisms(cubeSize, box);
                break;
            case 1:
                SpawnHorizontalPrisms(cubeSize, box);
                break;
        }

    }

    private void SpawnHorizontalPrisms(float cubeSize, GameObject box)
    {
        // Horizontal Down Prism 
        CreateObject(cubePrefab, new Vector3(0, 0, -cubeSize / 2), 
            new Vector3(cubeSize * 2, cubeSize , cubeSize), box.transform);
        // Horizontal Up Prism 
        CreateObject(cubePrefab, new Vector3(0, 0, cubeSize / 2), 
            new Vector3(cubeSize * 2, cubeSize , cubeSize), box.transform);
        
    }

    private void SpawnVerticalPrisms(float cubeSize, GameObject box)
    {
        // Left Vertical Prism 
        CreateObject(cubePrefab, new Vector3(-cubeSize / 2, 0, 0), 
            new Vector3(cubeSize, cubeSize , cubeSize * 2), box.transform);
        
        // Right Vertical Prism 
        CreateObject(cubePrefab, new Vector3(cubeSize / 2, 0, 0), 
            new Vector3(cubeSize, cubeSize , cubeSize * 2), box.transform);
        
    }

    private void CreateOneBigCube()
    {
        var cubeSize = _boxSize;
        var box = new GameObject("Box");
        box.transform.position = new Vector3(startPos.position.x, 0, startPos.position.z);
        box.transform.SetParent(transform);
        
        CreateObject(cubePrefab, Vector3.zero, Vector3.one * cubeSize, box.transform);
    }

    private void CreateObject(GameObject prefab, Vector3 position, Vector3 size, Transform boxTransform)
    {
        var obj = Instantiate(prefab, boxTransform);
        position.y = startPos.position.y;
        obj.transform.localPosition = position;
        size.y /= 2;
        obj.transform.localScale = size;
    }
}
