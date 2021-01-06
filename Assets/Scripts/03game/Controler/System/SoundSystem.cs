using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundSystem : MonoBehaviour
{
    [SerializeField] private string backgroundStyle = "Classic";
    [SerializeField] private bool isMute;
    [SerializeField] private Music[] musics;

    [HideInInspector] public static SoundSystem instance;
    private AudioSource audioSource;

    private void Start()
    {
        if(instance != null) Destroy(gameObject);
        else instance = this;

        audioSource = GetComponent<AudioSource>();
        audioSource.mute = isMute;

        PlayRandomMusic(backgroundStyle);
        DontDestroyOnLoad(gameObject);
    }

    public void PlayRandomMusic(string musiqueStyle)
    {
        List<Music> temp = new List<Music>();

        foreach (Music ms in musics)
        {
            if (ms.musicName.Contains(musiqueStyle))
            {
                temp.Add(ms);
            }
        }

        if (temp.Count != 0)
            Play(temp[UnityEngine.Random.Range(0, temp.Count)]);
        else
            Debug.Log("  [INFO:Music] No music found for " + musiqueStyle);
    }

    public void Play(int index)
    {
        if(musics[index] != null)
        {
            audioSource.PlayOneShot(musics[index].audio, musics[index].volume);
        }
    }

    public void Play(Music ms)
    {
        if (ms != null)
        {
            audioSource.PlayOneShot(ms.audio, ms.volume);
        }
    }

    public void PlayOver(AudioClip audio)
    {
        GetComponent<AudioSource>().PlayOneShot(audio);
    }

    private void Update()
    {
        if (audioSource.isPlaying) return;
        PlayRandomMusic(backgroundStyle);
    }
}
