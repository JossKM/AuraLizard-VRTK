using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour, System.IEquatable<Edge>
{
    public Node source;
    public Node destination;
    public float weight;

    [SerializeField]
    Renderer renderer;

    public void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    public bool Equals(Edge other)
    {
       return (source == other.source && destination == other.destination);
    }

    public void UpdateVisual()
    {
        Vector3 toTarget = destination.transform.position - source.transform.position;
        float distance = toTarget.magnitude; //- destination.GetRadius() - source.GetRadius();
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, distance);
        transform.position = source.transform.position;
        transform.rotation = Quaternion.LookRotation(toTarget, Vector3.up);
    }

    public void UpdateVisual(float width)
    {
        if(source == destination)
        {
            Debug.Log("Self loops are not yet supported");
            transform.localScale = Vector3.zero;
            return;
        }

        Vector3 toTarget = destination.transform.position - source.transform.position;
        float distance = toTarget.magnitude; //- destination.GetRadius() - source.GetRadius();
        transform.localScale = new Vector3(width, width, distance);
        transform.position = source.transform.position;
        transform.rotation = Quaternion.LookRotation(toTarget, Vector3.up);
    }

    public void SetHighlighted(Color newColor)
    {
        renderer.material.SetColor("_EmissionColor", newColor);
    }
}

