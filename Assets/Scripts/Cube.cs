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
    //private const float RayDistance = 1f;

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

    private Coroutine _growthRoutine;
    
    public void AnimateGrowing(Vector3 targetScale, Vector3 targetPosition)
    {
        if (_growthRoutine != null)
        {
            StopCoroutine(_growthRoutine);
        }
        _growthRoutine = StartCoroutine(AnimateGrowth(targetScale, targetPosition));
        
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

    public void CloseCube()
    {
        gameObject.SetActive(false);
        if (parentBox.cubes.Count == 1)
        {
            Debug.Log("DestroyCubeif" + cubeLocation + color);
            parentBox.DestroyBox();
        }
        
    }

    public void DestroyCube()
    {
        parentBox.cubes.Remove(this);
        parentBox.locationToCubeDict.Remove(cubeLocation);
        Destroy(gameObject);

    }

    public bool ControlCubesColor(Cube otherCube)
    {
        if (color == otherCube.color)
        {
            UpdateCube();
            otherCube.UpdateCube();
            return true;
        }

        return false;
    }
    private void UpdateCube()
    {
        CloseCube();
        parentBox.rulesData.CheckAndApplyGrowth(ref parentBox.locationToCubeDict, cubeLocation);
        
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