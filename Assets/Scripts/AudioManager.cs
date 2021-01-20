using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private GameObject audioObject = null;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip, bool looping)
    {
        if (looping)
        {
            audioSource.clip = clip;
            audioSource.loop = looping;
            audioSource.Play();
        }
        else
        {
            GameObject audio = Instantiate(audioObject, transform.position, Quaternion.identity);
            audio.GetComponent<AudioBehaviour>().InitializeSound(clip);
        }
    }

    public void StopSound()
    {
        audioSource.Stop();
    }
}
