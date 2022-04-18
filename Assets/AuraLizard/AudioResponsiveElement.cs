using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioResponsiveElement : MonoBehaviour
{
    static float FREQ_MULTIPLIER_BASE = 1.059463094359f; // Twelfth root of two, part of equation to map frequencies of notes according to https://pages.mtu.edu/~suits/NoteFreqCalcs.html
    static float PITCH_HALF_STEPS_PER_SIGNAL = 32.0f; // split signal into 
    public float frequencyOffset = 0.0f; // Can be used to indicate health

    [SerializeField]
    AudioSource audio;

    [SerializeField]
    public NodeVisualizationSettings settings;

    [SerializeField]
    Renderer renderer;

    //static float[] pingSamples = null;
    //static float[] notifSamples = null;

    //static int numSamplesPing = 0;
    //static int numSamplesNotif = 0;

    //static float pingScaleSample = 1.0f;
    //static float pingScaleBase = 0.25f;
    // static float pingSmoothing = 0.1f;

    private Vector3 baseScale = Vector3.one;

    IEnumerator pingCoroutine = null;

    private void Start()
    {
        SetBaseScale(transform.localScale);
    }

    public void SetBaseScale(Vector3 scale)
    {
        baseScale = scale;
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

    public void Ping(ClipType type, float signal, float delay)
    {
        audio.clip = settings.audioClips[(int)type];
        float frequencyMultiplier = Random.Range(0.995f, 1.005f) * PitchToRaiseByNotes(((PITCH_HALF_STEPS_PER_SIGNAL) * (1.0f - signal)) - PITCH_HALF_STEPS_PER_SIGNAL);
        pingCoroutine = AudioResponseCoroutine(type, signal, frequencyMultiplier, delay, Color.white);
        StartCoroutine(pingCoroutine);
    }

    public void Ping(ClipType type, float volume, float frequencyMultiplier, float delay, Color glowColor)
    {
        audio.clip = settings.audioClips[(int)type];
        pingCoroutine = AudioResponseCoroutine(type, volume, frequencyMultiplier, delay, glowColor);
        StartCoroutine(pingCoroutine);
    }

    void RevertToDefault()
    {
        renderer.material.SetColor("_EmissionColor", Color.black);
        transform.localScale = baseScale;
    }

    public static float PitchToRaiseByNotes(float numHalfSteps)
    {
        return Mathf.Pow(FREQ_MULTIPLIER_BASE, numHalfSteps);
    }

    private IEnumerator AudioResponseCoroutine(ClipType type, float volume, float frequencyMultiplier, float delay, Color glow)
    {
        yield return new WaitForSeconds(delay);
        audio.Stop();
        audio.volume = volume;
        audio.pitch = frequencyOffset + frequencyMultiplier;

        audio.Play();
        int sampleIdx = 0;
        while (audio.isPlaying)
        {
            float sample = settings.audioSamples[(int)type][sampleIdx];
            float progress = Mathf.Min((float)sampleIdx / (float)settings.audioSamples[(int)type].Length, 1.0f);
            float scale = (1.0f - progress) * volume;

            Color newCol = glow * scale;
            renderer.material.SetColor("_EmissionColor", newCol);
            float scalar = baseScale.x + (baseScale.x * settings.PING_SCALE_BASE * sample) + (baseScale.x * settings.PING_SCALE_SAMPLE * scale); //Mathf.Lerp(0.01f + baseScale + (baseScale * pingScaleSample * sample) + (pingScaleBase * scale), currentScale, pingSmoothing);

            if (type == ClipType.EdgeNotif || type == ClipType.EdgePing || type == ClipType.EdgeCreate || type == ClipType.EdgeDestroy)
            {
                transform.localScale = new Vector3(scalar, scalar, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(scalar, scalar, scalar);
            }

            yield return new WaitForEndOfFrame();
            sampleIdx = audio.timeSamples;
        }
    }

    //private IEnumerator AudioResponseCoroutine(ClipType type, float signal, float delay, Color glow)
    //{
    //    yield return new WaitForSeconds(delay);
    //    audio.Stop();
    //    audio.volume = signal;
    //    float frequencyMultiplier = frequencyOffset + Random.Range(0.995f, 1.005f) * PitchToRaiseByNotes(((PITCH_HALF_STEPS_PER_SIGNAL) * (1.0f - signal)) - PITCH_HALF_STEPS_PER_SIGNAL);
    //    audio.pitch = frequencyMultiplier;

    //    audio.Play();
    //    int sampleIdx = 0;
    //    while (audio.isPlaying)
    //    {
    //        float sample = settings.audioSamples[(int)type][sampleIdx];
    //        float progress = Mathf.Min((float)sampleIdx / (float)settings.audioSamples[(int)type].Length, 1.0f);
    //        float scale = (1.0f - progress) * signal;

    //        Color newCol = glow * scale;
    //        renderer.material.SetColor("_EmissionColor", newCol);
    //        float scalar = baseScale.x + (baseScale.x * settings.PING_SCALE_BASE * sample) + (baseScale.x * settings.PING_SCALE_SAMPLE * scale); //Mathf.Lerp(0.01f + baseScale + (baseScale * pingScaleSample * sample) + (pingScaleBase * scale), currentScale, pingSmoothing);

    //        if (type == ClipType.EdgeNotif || type == ClipType.EdgePing)
    //        {
    //            transform.localScale = new Vector3(scalar, scalar, transform.localScale.z);
    //        }
    //        else
    //        {
    //            transform.localScale = new Vector3(scalar, scalar, scalar);
    //        }

    //        yield return new WaitForEndOfFrame();
    //        sampleIdx = audio.timeSamples;
    //    }
    //}

    //private IEnumerator AuralizeProperties()
    //{

    //}
}
