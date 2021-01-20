using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardBank : MonoBehaviour
{
    static public CardBank instance;

    public GameObject[] cards;

    public Material[] cardMats;

    public AudioClip[] audioClips;

    private void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
}
