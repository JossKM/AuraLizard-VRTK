using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeAudioResponse : MonoBehaviour
{
    [SerializeField]
    AudioSource audio;

    [SerializeField]
    Color newCol;

    [SerializeField]
    Renderer renderer;
    
    static float[] pingSamples = null;
    static int numSamples = 0;
    static float pingScaleSample = 2f;
    static float pingScaleBase = 2f;

    private float baseScale = 0.0f;

    IEnumerator pingCoroutine = null;

    private void Start()
    {
        if (pingSamples == null)
        {
            numSamples = audio.clip.samples;
            pingSamples = new float[audio.clip.samples * audio.clip.channels];
            audio.clip.GetData(pingSamples, 0);
        }

        UpdateBaseScale();
    }

    public void UpdateBaseScale()
    {
        baseScale = transform.localScale.x;
    }

    public void Stop()
    {
        if (pingCoroutine != null)
        {
            StopCoroutine(pingCoroutine);
        }
        RevertToDefault();
    }

    public void Ping(float signal)
    {
        Stop();
        pingCoroutine = AudioResponseCoroutine(signal);
        StartCoroutine(pingCoroutine);
    }

    void RevertToDefault()
    {
        renderer.material.SetColor("_EmissionColor", Color.black);
        transform.localScale = new Vector3(baseScale, baseScale, baseScale);
    }

    private IEnumerator AudioResponseCoroutine(float signal)
    {
        audio.Stop();
        audio.volume = signal;
        audio.pitch = 1.0f + (-0.5f * signal);
        audio.Play();
        int sampleIdx = 0;
        while (sampleIdx < numSamples)
        {
            float sample = pingSamples[sampleIdx];
            float progress = (float)sampleIdx / (float)numSamples;
            float scale = (1.0f - progress) * signal;
            Color newCol = Color.white * scale;
            renderer.material.SetColor("_EmissionColor", newCol);
            float currentScale = 0.01f + baseScale + (baseScale * pingScaleSample * sample) + (pingScaleBase * scale);
            transform.localScale = new Vector3(currentScale, currentScale, currentScale);
            yield return new WaitForEndOfFrame();
            sampleIdx = audio.timeSamples;
        }
        Stop();
    }
}
