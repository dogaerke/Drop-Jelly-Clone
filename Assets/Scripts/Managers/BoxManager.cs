using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class BoxManager : MonoBehaviour
{
    public GameObject cubePrefab;  
    public GameObject boxPrefab;
    public Transform startPos;
    public List<BoxController> boxList;
    private GameObject _box;
    private float _boxSize = 1f;
    private Color[] colors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan};
    
    public static BoxManager Instance { get; private set; }
    
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
        SpawnNewBox();
    }

    private void OnDestroy()
    {
        boxList.Clear();
    }

    public void SpawnNewBox()
    {
        var randomCombination = (BoxCombination)UnityEngine.Random.Range(0, 4);
        CreateBox(randomCombination);
    }
    
    private void CreateBox(BoxCombination combination)
    {
        _box = Instantiate(boxPrefab, transform);
        boxList.Add(_box.GetComponent<BoxController>());
        _box.transform.position = new Vector3(startPos.position.x, 0, startPos.position.z );
        
        _box.GetComponent<BoxController>().isActive = true;

        switch (combination)
        {
            case BoxCombination.FourCubes:
                CreateFourCubes(_box);
                break;

            case BoxCombination.TwoCubesOnePrism:
                CreateTwoCubesOnePrism(_box);
                break;

            case BoxCombination.TwoPrisms:
                CreateTwoPrisms(_box);
                break;

            case BoxCombination.OneBigCube:
                CreateOneBigCube(_box);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(combination), combination, null);
        }
        
        
    }
    
    private void CreateFourCubes(GameObject box)
    {
        var cubeSize = _boxSize / 2f;
        var usedColors = new List<Color>();
        
        for (var x = 0; x < 2; x++)
        {
            for (var z = 0; z < 2; z++)
            {
                var position = new Vector3(x * cubeSize - cubeSize / 2, 
                    0, z * cubeSize - cubeSize / 2);
                
                var newColor = GetUniqueColor(usedColors);
                usedColors.Add(newColor);
                CreateObject(cubePrefab, position, Vector3.one * cubeSize, box.transform, newColor);
            }
        }
    }
    
    private void CreateTwoCubesOnePrism(GameObject box)
    {
        var cubeSize = _boxSize / 2f;
        var usedColors = new List<Color>();

        var randPos = UnityEngine.Random.Range(0,4);
        switch (randPos)
        {
            case 0:
                SpawnVerticalPrismRight(cubeSize, box, usedColors);
                break;
            case 1:
                SpawnVerticalPrismLeft(cubeSize, box, usedColors);
                break;
            case 2:
                SpawnHorizontalPrismUp(cubeSize, box, usedColors);
                break;
            case 3:
                SpawnHorizontalPrismDown(cubeSize, box, usedColors);
                break;
        }
    }

    private void SpawnVerticalPrismRight(float cubeSize, GameObject box, List<Color> usedColors)
    {
        var newColor = GetUniqueColor(usedColors);
        usedColors.Add(newColor);
        // Right Vertical Prism 
        CreateObject(cubePrefab, new Vector3(cubeSize / 2, 0, 0), 
            new Vector3(cubeSize, cubeSize , cubeSize * 2), box.transform, newColor);
        
        newColor = GetUniqueColor(usedColors);
        usedColors.Add(newColor);
        // Cubes
        CreateObject(cubePrefab, new Vector3(-cubeSize / 2, 0, cubeSize / 2),
            Vector3.one * cubeSize, box.transform, newColor);  // Left Up Cube
        newColor = GetUniqueColor(usedColors);
        usedColors.Add(newColor);
        
        CreateObject(cubePrefab, new Vector3(-cubeSize / 2, 0, -cubeSize / 2), 
            Vector3.one * cubeSize, box.transform, newColor); // Left Down Cube
    }
    private void SpawnVerticalPrismLeft(float cubeSize, GameObject box, List<Color> usedColors)
    {
        var newColor = GetUniqueColor(usedColors);
        usedColors.Add(newColor);
        
        // Left Vertical Prism 
        CreateObject(cubePrefab, new Vector3(-cubeSize / 2, 0, 0), 
            new Vector3(cubeSize, cubeSize , cubeSize * 2), box.transform, newColor);
        
        newColor = GetUniqueColor(usedColors);
        usedColors.Add(newColor);
        
        // Cubes
        CreateObject(cubePrefab, new Vector3(cubeSize / 2, 0, cubeSize / 2),
            Vector3.one * cubeSize, box.transform, newColor);  // Right Up Cube
        
        newColor = GetUniqueColor(usedColors);
        usedColors.Add(newColor);
        
        CreateObject(cubePrefab, new Vector3(cubeSize / 2, 0, -cubeSize / 2), 
            Vector3.one * cubeSize, box.transform, newColor); // Right Down Cube
    }
    private void SpawnHorizontalPrismUp(float cubeSize, GameObject box, List<Color> usedColors)
    {
        var newColor = GetUniqueColor(usedColors);
        usedColors.Add(newColor);
        
        // Horizontal Up Prism 
        CreateObject(cubePrefab, new Vector3(0, 0, cubeSize / 2), 
            new Vector3(cubeSize * 2, cubeSize , cubeSize), box.transform, newColor);
        
        newColor = GetUniqueColor(usedColors);
        usedColors.Add(newColor);
        
        // Cubes
        CreateObject(cubePrefab, new Vector3(-cubeSize / 2, 0, -cubeSize / 2), 
            Vector3.one * cubeSize, box.transform, newColor); // Left Down Cube
        
        newColor = GetUniqueColor(usedColors);
        usedColors.Add(newColor);
        
        CreateObject(cubePrefab, new Vector3(cubeSize / 2, 0, -cubeSize / 2),
            Vector3.one * cubeSize, box.transform, newColor);  // Right Down Cube
    }
    private void SpawnHorizontalPrismDown(float cubeSize, GameObject box, List<Color> usedColors)
    {
        var newColor = GetUniqueColor(usedColors);
        usedColors.Add(newColor);
        
        // Horizontal Down Prism 
        CreateObject(cubePrefab, new Vector3(0, 0, -cubeSize / 2), 
            new Vector3(cubeSize * 2, cubeSize , cubeSize), box.transform, newColor);
        
        newColor = GetUniqueColor(usedColors);
        usedColors.Add(newColor);
        
        // Cubes
        CreateObject(cubePrefab, new Vector3(-cubeSize / 2, 0, cubeSize / 2), 
            Vector3.one * cubeSize, box.transform, newColor); // Left Up Cube
        
        newColor = GetUniqueColor(usedColors);
        usedColors.Add(newColor); 
        
        CreateObject(cubePrefab, new Vector3(cubeSize / 2, 0, cubeSize / 2),
            Vector3.one * cubeSize, box.transform, newColor);  // Right Up Cube
    }


    private void CreateTwoPrisms(GameObject box)
    {
        var cubeSize = _boxSize / 2f;
        var usedColors = new List<Color>();

        var randPos = UnityEngine.Random.Range(0,2);
        switch (randPos)
        {
            case 0:
                SpawnVerticalPrisms(cubeSize, box, usedColors);
                break;
            case 1:
                SpawnHorizontalPrisms(cubeSize, box, usedColors);
                break;
        }

    }

    private void SpawnHorizontalPrisms(float cubeSize, GameObject box, List<Color> usedColors)
    {
        var newColor = GetUniqueColor(usedColors);
        usedColors.Add(newColor); 
        
        // Horizontal Down Prism 
        CreateObject(cubePrefab, new Vector3(0, 0, -cubeSize / 2), 
            new Vector3(cubeSize * 2, cubeSize , cubeSize), box.transform, newColor);
        
        newColor = GetUniqueColor(usedColors);
        usedColors.Add(newColor); 
        
        // Horizontal Up Prism 
        CreateObject(cubePrefab, new Vector3(0, 0, cubeSize / 2), 
            new Vector3(cubeSize * 2, cubeSize , cubeSize), box.transform, newColor);
        
    }

    private void SpawnVerticalPrisms(float cubeSize, GameObject box, List<Color> usedColors)
    {
        var newColor = GetUniqueColor(usedColors);
        usedColors.Add(newColor); 
        // Left Vertical Prism 
        CreateObject(cubePrefab, new Vector3(-cubeSize / 2, 0, 0), 
            new Vector3(cubeSize, cubeSize , cubeSize * 2), box.transform, newColor);
        
        newColor = GetUniqueColor(usedColors);
        usedColors.Add(newColor); 
        
        // Right Vertical Prism 
        CreateObject(cubePrefab, new Vector3(cubeSize / 2, 0, 0), 
            new Vector3(cubeSize, cubeSize , cubeSize * 2), box.transform, newColor);
        
    }

    private void CreateOneBigCube(GameObject box)
    {
        var cubeSize = _boxSize;
        var usedColors = new List<Color>();
        var newColor = GetUniqueColor(usedColors);
        usedColors.Add(newColor); 
        
        CreateObject(cubePrefab, Vector3.zero, new Vector3(cubeSize, cubeSize / 2, cubeSize), box.transform, newColor);
    }

    private void CreateObject(GameObject prefab, Vector3 position, Vector3 size, Transform boxTransform, Color color)
    {
        var obj = Instantiate(prefab, boxTransform);
        position.y = startPos.position.y;
        obj.transform.localPosition = position;
        size.y /= 2;
        obj.transform.localScale = size;

        obj.GetComponentInChildren<Renderer>().material.color = color;

    }

    private Color GetUniqueColor(List<Color> usedColors)
    {
        Color newColor;
        do
        {
            newColor = colors[UnityEngine.Random.Range(0, colors.Length)];
        } while (usedColors.Contains(newColor));

        return newColor;
    }
    
    
    

}
