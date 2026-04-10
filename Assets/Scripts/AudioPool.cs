using UnityEngine;
using System.Collections.Generic;

public class AudioPool : MonoBehaviour
{
    public static AudioPool Instance;

    [SerializeField] private int poolSize = 10;

    private List<AudioSource> sources = new List<AudioSource>();

    void Awake()
    {
        Instance = this;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = new GameObject("PooledAudio_" + i);
            go.transform.parent = transform;

            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;

            sources.Add(source);
        }
    }

    public void PlayClip(AudioClip clip, Vector3 position, float volume, float duration)
    {
        AudioSource source = GetAvailableSource();

        source.transform.position = position;
        source.clip = clip;
        source.volume = volume;
        source.time = 0f;

        // random pitch variation
        source.pitch = Random.Range(0.9f, 1.1f);

        // random start time
        float maxStart = Mathf.Max(0f, clip.length * 0.5f);
        source.time = Random.Range(0f, maxStart);

        source.gameObject.SetActive(true);
        source.Play();

        StartCoroutine(PlayWithFade(source, volume, duration));
    }

    private AudioSource GetAvailableSource()
    {
        foreach (var s in sources)
        {
            if (!s.isPlaying)
                return s;
        }

        // fallback if all are busy
        return sources[0];
    }

    private System.Collections.IEnumerator PlayWithFade(AudioSource source, float targetVolume, float duration)
    {
        float fadeInTime = 0.05f;
        float t = 0f;

        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, t / fadeInTime);
            yield return null;
        }

        source.volume = targetVolume;

        float waitTime = Mathf.Max(0f, duration - 0.1f);
        yield return new WaitForSeconds(waitTime);

        float fadeOutTime = 0.2f;
        t = 0f;

        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(targetVolume, 0f, t / fadeOutTime);
            yield return null;
        }

        source.Stop();
        source.gameObject.SetActive(false);
    }
}