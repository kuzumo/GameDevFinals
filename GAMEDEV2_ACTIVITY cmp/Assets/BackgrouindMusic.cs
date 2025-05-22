using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic backgroundMusic;

    void Awake()
    {
        if (backgroundMusic == null)
        {
            backgroundMusic = this;
            // Removed DontDestroyOnLoad to allow it to be destroyed on scene load
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
