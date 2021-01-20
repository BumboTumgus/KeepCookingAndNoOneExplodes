using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioBehaviour : MonoBehaviour
{
    private float currentTimer = 0f;
    private float targetTimer = 1f;
    private bool initialized = false;
    private AudioSource audioSource;
    
    // Update is called once per frame
    void Update()
    {
        if(initialized)
        {
            currentTimer += Time.deltaTime;
            if (currentTimer >= targetTimer)
                Destroy(gameObject);
        }
    }

    // used to set up the audiosource of this object
    public void InitializeSound(AudioClip clip)
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();
        targetTimer = clip.length + 0.1f;
        initialized = true;
    }
}
