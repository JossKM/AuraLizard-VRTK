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

    //[SerializeField]
    //AudioSource audio;

    [SerializeField]
    AudioResponsiveElement audioResponse;

    //[SerializeField]
    //AudioClip edgePingSound;
    //[SerializeField]
    //AudioClip notifSound;

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
        audioResponse.SetBaseScale(transform.localScale);
    }

    public void SetHighlighted(Color newColor)
    {
        renderer.material.SetColor("_EmissionColor", newColor);
    }

    public void Ping(float signal, float delay)
    {
        if (signal > audioResponse.settings.SIGNAL_PING_THRESH)
        {
            //StartCoroutine(PingCoroutine(signal, delay));
            audioResponse.Ping(ClipType.EdgePing, signal, delay);
            destination.Ping(signal - audioResponse.settings.SIGNAL_LOSS, delay + audioResponse.settings.PING_DELAY);
        }
    }

    public void Notif(float volume, float speed, Color glowColor)
    {
        audioResponse.Ping(ClipType.EdgeNotif, volume, speed, 0.0f, glowColor);
    }

    //IEnumerator PingCoroutine(float signal, float delay)
    //{
    //    //audio.PlayDelayed(delay);
    //    yield return new WaitForSeconds(delay);
    //    AudioSource.PlayClipAtPoint(edgePingSound, Vector3.Lerp(source.transform.position, destination.transform.position, 0.5f));
    //    destination.Ping(signal - source.settings.SIGNAL_LOSS, delay + source.settings.PING_DELAY);
    //}

    //public void PlayNotifSound(float pitch)
    //{
    //    audio.pitch = pitch;
    //    audio.clip = notifSound;
    //    audio.Play();
    //}
}

