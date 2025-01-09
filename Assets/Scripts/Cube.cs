using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public Color color;
    public Vector3 LocalPosition
    {
        get => transform.localPosition;
        set => transform.localPosition = value;
    }

    private void Start()
    {
        color = GetComponentInChildren<Renderer>().material.color;
    }
}
