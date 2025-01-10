using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public Color color;
    private LayerMask _targetLayer;
    private BoxController _parentBox;
    public float rayDistance = 0.5f;
    public CubeLocation cubeLocation;
    
    private void Start()
    {
        color = GetComponentInChildren<Renderer>().material.color;
        _targetLayer = LayerMask.GetMask("Cube");
        _parentBox = GetComponentInParent<BoxController>();
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

            if (Physics.Raycast(transform.position, direction, out hit, rayDistance, _targetLayer))
            {
                var other = hit.collider.gameObject;
                if (other && other.transform.parent != transform.parent)
                {
                    affectedBoxes.Add(other.GetComponentInParent<BoxController>());
                    
                    if (other.GetComponent<Cube>().color == color)
                    {
                        var destroyedCube = gameObject.transform;
                        var destroyedOther = other.transform;
                        _parentBox.UpdateCubes(destroyedCube);
                        destroyedOther.GetComponentInParent<BoxController>().UpdateCubes(destroyedOther);
                        // Destroy(other);
                        // Destroy(gameObject);
                        
                    }
                }
            }
            else
            {
                Debug.Log($"There is no neighbour: Direction {direction}");
            }

            Debug.DrawRay(transform.position, direction * rayDistance, Color.red, 1f);
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

            if (Physics.Raycast(cube.transform.position, direction, out hit, rayDistance, _targetLayer))
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
                        // Destroy(cube.gameObject);
                        // Destroy(other);

                    }
                }
                
            }
            else
            {
                Debug.Log($"There is no neighbour: Direction {direction}");
            }

            Debug.DrawRay(cube.transform.position, direction * rayDistance, Color.red, 1f);
        }
    }
    
}

public enum CubeLocation
{
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