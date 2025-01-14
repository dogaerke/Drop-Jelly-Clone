using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class BoxController : MonoBehaviour
{
    public Vector2 touchStartPos;
    public Vector2 touchDirection;
    public bool isActive;
    public List<Cube> cubes;
    public List<CubeLocation> pasiveCubeLocations;
    public Dictionary<CubeLocation, Cube> locationToCubeDict = new Dictionary<CubeLocation, Cube>();
    public GrowthRulesData rulesData;
    public NeighborCheckRulesData neighborRulesData;
    
    public event Action<Vector3> OnBoxDropped;

    private void Start()
    {
        OnBoxDropped += HandleBoxDropped;
        
        cubes = new List<Cube>(gameObject.GetComponentsInChildren<Cube>());
        foreach (var c in cubes)
        {
            locationToCubeDict.Add(c.cubeLocation, c);
        }

    }
    private void OnDestroy()
    {
        OnBoxDropped -= HandleBoxDropped;
    }

    
    private void Update()
    {
        TouchControl();
    }

    private Dictionary<BoxLocationForNeighbors, BoxController> DetermineBoxNeighbors()
    {
        int distance = GridManager.Instance.gridStep;
        var boxNeighbourList = new Dictionary<BoxLocationForNeighbors, BoxController>();
        
        var pos = transform.position;
        Debug.Log(pos);
        //Left
        if (pos.x - distance >= 0)
        {
            if (BoxManager.Instance.TryGetBox(new Vector2(pos.x - distance, pos.z) , out var box))
            {
                boxNeighbourList.Add(BoxLocationForNeighbors.Left, box);
                Debug.Log("LeftNeighbor");
            }
        }    
    
        //Right
        if (pos.x + distance < GridManager.Instance.width)
        {
            if (BoxManager.Instance.TryGetBox(new Vector2(pos.x + distance, pos.z), out var box))
            {
                boxNeighbourList.Add(BoxLocationForNeighbors.Right, box);                
                Debug.Log("RightNeighbor");
            }

        }
    
        //Down
        if (pos.z + distance < GridManager.Instance.height)
        {
            if (BoxManager.Instance.TryGetBox(new Vector2(pos.x, pos.z + distance), out var box))
            {
                boxNeighbourList.Add(BoxLocationForNeighbors.Top, box);
                Debug.Log("UpNeighbor");

            }
            
        }
    
        //Up
        if (pos.z - distance >= 0)
        {
            if (BoxManager.Instance.TryGetBox(new Vector2(pos.x, pos.z - distance), out var box))
            {
                boxNeighbourList.Add(BoxLocationForNeighbors.Bottom, box);
                Debug.Log("DownNeighbor");
            }
        }
        
        
        return boxNeighbourList;
    }
    
    
    
    private void TouchControl()
    {
        if (!isActive) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    break;

                case TouchPhase.Moved:
                    touchDirection = touch.position - touchStartPos;

                    if (Mathf.Abs(touchDirection.x) > 50f)
                    {
                        if (touchDirection.x > 0)
                        {
                            MoveBox(Vector3.right * GridManager.Instance.gridStep);
                        }
                        else if (touchDirection.x < 0)
                        {
                            MoveBox(Vector3.left * GridManager.Instance.gridStep);
                        }
                        
                        touchStartPos = touch.position;
                    }
                    break;

                case TouchPhase.Ended:
                    DropBox(1.5f);
                    break;
            }
        }
    }

    private void MoveBox(Vector3 direction)
    {
        var gridManager = GridManager.Instance;

        var newPosition = transform.position + direction;

        if (newPosition.x >= 0 && newPosition.x <= (gridManager.width - 1) * gridManager.gridStep)
        {
            transform.position = newPosition;
        }
    }


    public void DropBox(float duration)
    {
        StartCoroutine(DropActiveBox(duration));
    }
    
    private void HandleBoxDropped(Vector3 targetPos)
    {
        GridManager.Instance.Tiles.TryGetValue(new Vector2(targetPos.x, targetPos.z), out var tile);
        if (tile) tile.SetTileFullness(true);
        
        BoxManager.Instance.AddNewBoxToList(new Vector2(targetPos.x, targetPos.z), this);
        WaitAndControlNeighbors();
        ControlIsGameOver();
        TrySpawnNewBox();
    }

    private void ControlIsGameOver()
    {
        if (Mathf.Abs(transform.localPosition.z - GridManager.Instance.height - GridManager.Instance.gridStep) < 0.5f)
        {
            Debug.Log("GAME OVER !");
            BoxManager.Instance.gameOverScene.gameObject.SetActive(true);
        }
    }

    private static void TrySpawnNewBox()
    {
        var flag = false;
        
        foreach (var box in BoxManager.Instance.boxDict)
        {
            if (box.Value.isActive)
            {
                flag = true;  //if there is activeBox, don't spawn new box
            }

        }
        if (!flag)
        {
            BoxManager.Instance.SpawnNewBox();
        }
    }
    public void WaitAndControlNeighbors()
    {
        StartCoroutine(WaitAndDestroy());
    }
    private IEnumerator WaitAndDestroy()
    {
        yield return new WaitForSeconds(0.3f);
        ControlNeighborBoxes();
    }
    public void ControlNeighborBoxes()
    {
        var neighborList = DetermineBoxNeighbors();
        
        //Debug.Log("1");
        if (neighborList.Count == 0)return;
        //Debug.Log("2");
        int counter = 0;
        foreach (var neighborBox in neighborList)
        {
            Debug.Log("NeighborKey: " + neighborBox.Key + "NeighborValue" + neighborBox.Value.transform.position , neighborBox.Value);

            var checkData = neighborRulesData.NeighborCheckList[neighborBox.Key];
            
            foreach (var v in checkData.MainCheckList)
            {
                //Debug.Log("Locs : " + v);
                
                
                neighborBox.Value.locationToCubeDict.TryGetValue(v.Key, out var neighborCube);

                if (neighborCube)
                {
                    var locationData = v.Value;
                    var locToControlList = locationData.Loc;

                    foreach (var location in locToControlList)
                    {
                        
                        locationToCubeDict.TryGetValue(location, out var mainCube);

                        if (mainCube)
                        {
                            Debug.Log("N: " + v.Key + " mainCube: " + location);
                            if (mainCube.ControlCubesColor(neighborCube))
                                counter++;
                                //break;
                             if (counter > 1)
                                 break;

                        }
                        
                    }
                    if (counter>1)
                        break;
                    Debug.Log("foreach1");
                }
                if (counter>1)
                    break;
                Debug.Log("foreach2");

            }
            
        }
    }


    private IEnumerator DropActiveBox(float duration)
    {
        var targetPosition = FindLowestAvailablePosition();
        if (targetPosition == new Vector3(-1, -1, -1)) yield break;
        isActive = false;
        
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            var t = Mathf.SmoothStep(0, 1, elapsedTime / duration);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, t);
            yield return null;
        }
    
        transform.localPosition = targetPosition;
        if (Mathf.Abs(transform.localPosition.z - targetPosition.z) < 0.001f)
        {
            OnBoxDropped?.Invoke(targetPosition);
            
        }
        
    }
    private Vector3 FindLowestAvailablePosition()
    {
        var gridManager = GridManager.Instance;
        var startPosition = transform.localPosition;

        var initialPosition = new Vector3(startPosition.x, startPosition.y, startPosition.z);
        if (initialPosition.z - BoxManager.Instance.startPos.position.z < 0.1f)
        {
            initialPosition.z = gridManager.height - gridManager.gridStep;
        }
        
        if (IsGridOccupied(initialPosition))
        {
            //Debug.Log("Game Over!!!");
            BoxManager.Instance.gameOverScene.gameObject.SetActive(true);

            /////TODO GameManager.Instance.TriggerGameOver();
            return initialPosition;
        }

        var currentPosition = initialPosition;

        while (true)
        {
            var nextPosition = currentPosition + Vector3.back * gridManager.gridStep;
            var isFull = IsGridOccupied(nextPosition);
            if (nextPosition.z < 0 || isFull) break;

            currentPosition = nextPosition;
        }

        return currentPosition;
    }

    
    private bool IsGridOccupied(Vector3 position)
    {
        GridManager.Instance.Tiles.TryGetValue(new Vector2(position.x, position.z), out var tile);
        return !tile || tile.isFull;
    }

    public void DestroyBox()
    {
        var pos = new Vector2(transform.position.x, transform.position.z);
        
        GridManager.Instance.Tiles.TryGetValue(pos, out var tile);
        if(tile) tile.SetTileFullness(false);
        cubes.Remove(cubes[0]);
        BoxManager.Instance.boxDict.Remove(pos);
        //gameObject.SetActive(false);
        //Destroy(gameObject);
        //Debug.Log("Destroybox");
        StartCoroutine(WaitForDrop());
    }
    private IEnumerator WaitForDrop()
    {
        //Debug.Log("Waiting");
        yield return new WaitForSeconds(0.7f);
        StartCoroutine(BoxManager.Instance.TryDropBoxesAfterExplosion(gameObject));
    }
    
}

public enum BoxLocationForNeighbors
{
    Top,
    Bottom,
    Left,
    Right
}
