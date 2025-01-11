using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Cube : MonoBehaviour
{
    public Color color;
    public BoxController parentBox;
    public CubeLocation cubeLocation;
    
    private LayerMask _targetLayer;
    private const float RayDistance = 1f;

    private void Start()
    {
        color = GetComponentInChildren<Renderer>().material.color;
        _targetLayer = LayerMask.GetMask("Cube");
        parentBox = GetComponentInParent<BoxController>();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();

    }

    public void CheckNeighborsAndDestroy()
    {
        var affectedBoxes = new HashSet<BoxController>();
        Vector3[] directions =
        {
            Vector3.right, 
            Vector3.left, 
            Vector3.forward,
            Vector3.back
        };

        RayForMainCube(directions, affectedBoxes);
        RayForAffectedCubes(directions, affectedBoxes);
    }

    private void RayForMainCube(IEnumerable<Vector3> directions, ISet<BoxController> affectedBoxes)
    {
        foreach (var direction in directions)
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, direction, out hit, RayDistance, _targetLayer))
            {
                var other = hit.collider.gameObject;
                if (other && other.transform.parent != transform.parent)
                {
                    affectedBoxes.Add(other.GetComponentInParent<BoxController>());
                    
                    if (other.GetComponent<Cube>().color == color)
                    {
                        var destroyedCube = gameObject.transform;
                        var destroyedOther = other.transform;
                        parentBox.UpdateCubes(destroyedCube);
                        destroyedOther.GetComponentInParent<BoxController>().UpdateCubes(destroyedOther);
                        
                    }
                }
            }

            Debug.DrawRay(transform.position, direction * RayDistance, Color.red, 1f);
        }
    }
    
    private void RayForAffectedCubes(IEnumerable<Vector3> directions, ISet<BoxController> affectedBoxes)
    {
        foreach (var box in affectedBoxes)
        {
            var cubes = box.GetComponentsInChildren<Cube>();
        
            foreach (var cube in cubes)
            {
                CheckNeighborsForCube(cube, directions); 
            }
        }
    }

    private void CheckNeighborsForCube(Cube cube, IEnumerable<Vector3> directions)
    {
        foreach (var direction in directions)
        {
            RaycastHit hit;

            if (Physics.Raycast(cube.transform.position, direction, out hit, RayDistance, _targetLayer))
            {
                var other = hit.collider.gameObject;

                if (other && other.transform.parent != cube.transform.parent)
                {
                    var otherCube = other.GetComponent<Cube>();
                    if (otherCube && otherCube.color == cube.color)
                    {
                        var destroyedCube = cube.gameObject.transform;
                        var destroyedOther = other.transform;
                        destroyedCube.GetComponentInParent<BoxController>().UpdateCubes(destroyedCube);
                        destroyedOther.GetComponentInParent<BoxController>().UpdateCubes(destroyedOther);
                    }
                }
                
            }

            Debug.DrawRay(cube.transform.position, direction * RayDistance, Color.red, 1f);
        }
    }

    public void WaitAndUpdateCubes()
    {
        StartCoroutine(WaitAndDestroy());
    }
    private IEnumerator WaitAndDestroy()
    {
        yield return new WaitForSeconds(0.5f);
        CheckNeighborsAndDestroy();
    }
    public void AnimateGrowing(Vector3 targetScale, Vector3 targetPosition)
    {
        StartCoroutine(AnimateGrowth(targetScale, targetPosition));
        
    }
    
    private IEnumerator AnimateGrowth(Vector3 targetScale, Vector3 targetPosition)
    {
        var initialScale = transform.localScale;
        var elapsedTime = 0f;
        var duration = 0.2f;
        var originalPosition = transform.localPosition;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / duration;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            transform.localPosition = Vector3.Lerp(originalPosition, targetPosition, t);
            yield return null;
        }
    
        transform.localScale = targetScale;
        transform.localPosition = targetPosition;

    }

    public void DestroyCube()
    {
        parentBox.cubes.Remove(this);
        parentBox.locationToCubeDict.Remove(cubeLocation);
        Destroy(gameObject);
        if (parentBox.cubes.Count == 0)
        {
            parentBox.DestroyBox();
        }
        
    }
}

public enum CubeLocation
{
    Default,
    Null,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Top,
    Bottom,
    Left,
    Right,
    Middle
}