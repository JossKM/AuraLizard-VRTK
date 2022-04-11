using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeAudioResponse : MonoBehaviour
{
    const float FREQ_MULTIPLIER_BASE = 1.059463094359f; // Twelfth root of two, part of equation to map frequencies of notes according to https://pages.mtu.edu/~suits/NoteFreqCalcs.html
    const float PITCH_HALF_STEPS_PER_SIGNAL = 8.0f; // split signal into 

    [SerializeField]
    AudioSource audio;

    [SerializeField]
    AudioClip ping;

    [SerializeField]
    Color newCol;

    [SerializeField]
    Renderer renderer;
    
    static float[] pingSamples = null;
    static int numSamples = 0;
    static float pingScaleSample = 1.0f;
    static float pingScaleBase = 0.25f;
   // static float pingSmoothing = 0.1f;

    private float baseScale = 0.0f;
    private float currentScale = 1.0f;

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
            pingCoroutine = null;
        }
        RevertToDefault();
    }

    public void Ping(float signal, float delay)
    {
        audio.clip = ping;
        pingCoroutine = AudioResponseCoroutine(signal, delay);
        StartCoroutine(pingCoroutine);
    }

    void RevertToDefault()
    {
        renderer.material.SetColor("_EmissionColor", Color.black);
        currentScale = baseScale;
        transform.localScale = new Vector3(currentScale, currentScale, currentScale);
    }

    private IEnumerator AudioResponseCoroutine(float signal, float delay)
    {
        yield return new WaitForSeconds(delay);
        audio.Stop();
        audio.volume = signal;
        float frequencyMultiplier = Random.Range(0.995f, 1.005f) * Mathf.Pow(FREQ_MULTIPLIER_BASE, ((PITCH_HALF_STEPS_PER_SIGNAL) * (1.0f - signal)) - PITCH_HALF_STEPS_PER_SIGNAL);
        audio.pitch = frequencyMultiplier;

        audio.Play();
        int sampleIdx = 0;
        while (audio.isPlaying)
        {
            float sample = pingSamples[sampleIdx];
            float progress = Mathf.Min((float)sampleIdx / (float)numSamples, 1.0f);
            float scale = (1.0f - progress) * signal;

            Color newCol = Color.white * scale;
            renderer.material.SetColor("_EmissionColor", newCol);
            currentScale = 0.01f + baseScale + (baseScale * pingScaleSample * sample) + (pingScaleBase * scale); //Mathf.Lerp(0.01f + baseScale + (baseScale * pingScaleSample * sample) + (pingScaleBase * scale), currentScale, pingSmoothing);
            transform.localScale = new Vector3(currentScale, currentScale, currentScale);
            yield return new WaitForEndOfFrame();
            sampleIdx = audio.timeSamples;
        }
    }
}
