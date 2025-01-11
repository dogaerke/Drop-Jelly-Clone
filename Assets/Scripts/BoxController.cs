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
    public Dictionary<CubeLocation, Cube> locationToCubeDict = new Dictionary<CubeLocation, Cube>();
    public GrowthRulesData rulesData;
    
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
        
        DestroySameColorCubes();
        ControlIsGameOver();
        TrySpawnNewBox();
    }

    private void ControlIsGameOver()
    {
        if (Mathf.Abs(transform.localPosition.z - GridManager.Instance.height - 1) < 0.5f)
        {
            Debug.Log("GAME OVER !");
            BoxManager.Instance.GameOverScene.gameObject.SetActive(true);
        }
    }

    private static void TrySpawnNewBox()
    {
        var flag = false;
        
        foreach (var box in BoxManager.Instance.boxList)
        {
            if (box.isActive)
            {
                flag = true;  //if there is activeBox, don't spawn new box
            }

        }
        if (!flag)
        {
            BoxManager.Instance.SpawnNewBox();
        }
    }

    public void DestroySameColorCubes()
    {
        for (var i = 0; i < cubes.Count; i++)
        {
            cubes[i].CheckNeighborsAndDestroy();
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
            Debug.Log("Game Over!!!");
            BoxManager.Instance.GameOverScene.gameObject.SetActive(true);

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
        GridManager.Instance.Tiles.TryGetValue
            (new Vector2(transform.position.x, transform.position.z), out var tile);
        if(tile) tile.SetTileFullness(false);
        BoxManager.Instance.boxList.Remove(this);
        StartCoroutine(WaitForDrop());
    }
    private IEnumerator WaitForDrop()
    {
        Debug.Log("Waiting");
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(BoxManager.Instance.TryDropBoxesAfterExplosion());
        Destroy(gameObject);


    }
    public void UpdateCubes(Transform cubeToDestroy)
    {
        if (cubeToDestroy)
        {
            cubeToDestroy.gameObject.SetActive(false);
            rulesData.CheckAndApplyGrowth(ref locationToCubeDict);
        }
        
    }
}
