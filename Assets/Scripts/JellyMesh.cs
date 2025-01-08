using System;
using UnityEngine;

public class JellyMesh : MonoBehaviour
{
    public float Intensity = 1f;
    public float mass = 1f;
    public float stiffnes = 1f;
    public float damping = 0.75f;
    private Mesh _originalMesh, _meshClone;
    private MeshRenderer _renderer;
    private JellyVertex[] _jellyVertex;
    public Vector3[] vertexArr;
    
    private void Start()
    {
        _originalMesh = GetComponent<MeshFilter>().sharedMesh;
        _meshClone = Instantiate(_originalMesh);
        GetComponent<MeshFilter>().sharedMesh = _meshClone;
        _renderer = GetComponent<MeshRenderer>();
        _jellyVertex = new JellyVertex[_meshClone.vertices.Length];
        for (var i = 0; i < _meshClone.vertices.Length; i++)
        {
            _jellyVertex[i] = new JellyVertex(i, transform.TransformPoint(_meshClone.vertices[i]));
        }
    }

    private void FixedUpdate()
    {
        vertexArr = _originalMesh.vertices;
        foreach (var j in _jellyVertex)
        {
            var target = transform.TransformPoint(vertexArr[j.id]);
            var intensity = (1 - (_renderer.bounds.max.y - target.y) / _renderer.bounds.size.y) * Intensity;
            j.Shake(target, mass, stiffnes, damping);
            target = transform.InverseTransformPoint(j.position);
            vertexArr[j.id] = Vector3.Lerp(vertexArr[j.id], target, intensity);
        }

        _meshClone.vertices = vertexArr;
    }

    private class JellyVertex
    {
        public int id;
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 force;

        public JellyVertex(int _id, Vector3 pos)
        {
            id = _id;
            position = pos;
        }
        
        public void Shake(Vector3 target, float m, float s, float d)
        {
            force = (target - position) * s;
            velocity = (velocity + force / m) * d;
            position += velocity;
            if ((velocity + force + force / m).magnitude < 0.001f)
                position = target;
            

        }
    }

    
}