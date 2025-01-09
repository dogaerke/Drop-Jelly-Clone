using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class BoxController : MonoBehaviour
{
    public Vector2 touchStartPos;
    public Vector2 touchDirection;
    public bool isActive;
    public event Action<Vector3> OnBoxDropped;

    private void Start()
    {
        OnBoxDropped += HandleBoxDropped;
    }
    private void OnDestroy()
    {
        OnBoxDropped -= HandleBoxDropped;
        StopAllCoroutines();
    }

    
    private void Update()
    {
        if (!isActive) return;

        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    Debug.Log("Touch Phase Began");
                    touchStartPos = touch.position;
                    break;

                case TouchPhase.Moved:
                    Debug.Log("Touch Phase Moved");
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
                    Debug.Log("Touch Phase Ended");
                    DropBox();
                    break;
            }
        }
    }

    private void MoveBox(Vector3 _direction)
    {
        var gridManager = GridManager.Instance;

        var newPosition = transform.position + _direction;

        if (newPosition.x >= 0 && newPosition.x <= (gridManager.width - 1) * gridManager.gridStep)
        {
            transform.position = newPosition;
        }
    }


    private void DropBox()
    {
        StartCoroutine(DropActiveBox());
    }
    
    private void HandleBoxDropped(Vector3 targetPos)
    {
        GridManager.Instance.Tiles.TryGetValue(new Vector2(targetPos.x, targetPos.z), out var tile);
        if (tile) tile.SetTileFull(true);
        
        var flag = false;
        
        foreach (var box in BoxManager.Instance.boxList)
        {
            if (box.isActive)
            {
                flag = true;
            }

        }
        if (!flag)
        {
            BoxManager.Instance.SpawnNewBox();
        }
    }

    private IEnumerator DropActiveBox()
    {
        var targetPosition = FindLowestAvailablePosition();
        isActive = false;
        
        const float duration = 1.5f;
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            var t = Mathf.SmoothStep(0, 1, elapsedTime / duration);
            transform.position = Vector3.Lerp(transform.position, targetPosition, t);
            yield return null;
        }
    
        transform.position = targetPosition;
        if (Mathf.Abs(transform.position.z - targetPosition.z) < 0.001f)
        {
            OnBoxDropped?.Invoke(targetPosition);
            
        }
        
    }
    private Vector3 FindLowestAvailablePosition()
    {
        var gridManager = GridManager.Instance;
        var startPosition = transform.position;

        var initialPosition = new Vector3(startPosition.x, startPosition.y, gridManager.height - gridManager.gridStep);
        if (IsGridOccupied(initialPosition))
        {
            Debug.Log("Game Over");
            /////TODO GameManager.Instance.TriggerGameOver();
            return initialPosition;
        }

        var currentPosition = initialPosition;

        while (true)
        {
            var nextPosition = currentPosition + Vector3.back * gridManager.gridStep;
            var a = IsGridOccupied(nextPosition);
            if (nextPosition.z < 0 || a) break;

            currentPosition = nextPosition;
        }

        return currentPosition;
    }

    
    private bool IsGridOccupied(Vector3 position)
    {
        GridManager.Instance.Tiles.TryGetValue(new Vector2(position.x, position.z), out var tile);
        return !tile || tile.isFull;
    }
}