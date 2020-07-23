using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MuteButtonBehaviour : MonoBehaviour
{
    private bool isMuted = false;
    private float oldVolume = 0;

    public Sprite MutedImage;
    public Sprite UnmutedImage;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Image>().sprite = UnmutedImage;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Toggle()
    {
        isMuted = !isMuted;
        if (isMuted)
        {
            if (AudioListener.volume > 0)
            {
                oldVolume = AudioListener.volume;
                AudioListener.volume = 0;
                GetComponent<Image>().sprite = MutedImage;
            }
        }
        else
        {
            AudioListener.volume = oldVolume;
            GetComponent<Image>().sprite = UnmutedImage;
        }
    }
}
