using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct CardAudio
{
    public AudioSource audioSource;
    public float startTime;
}

public class MoveBehaviour : MonoBehaviour
{
    float timeRemaining = 0f;
    public bool IsMoving
    {
        get
        {
            return enabled;
        }
    }
    Vector3 targetPosition;
    // Start is called before the first frame update
    public List<CardAudio> cardAudios;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (timeRemaining > 0)
        {
            Vector3 delta = (targetPosition - transform.localPosition) / (timeRemaining / Time.deltaTime);
            var pos = transform.localPosition + delta;
            pos.z = -.52f;
            transform.localPosition = pos;
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                transform.localPosition = targetPosition;
                enabled = false;
                Debug.Log("Done moving " + gameObject.name);
            }
        }
    }

    public void MoveTo(Vector3 position, bool playSound = true)
    {
        Debug.Log("Moving " + gameObject.name + " to " + position);
        timeRemaining = SolitaireGameBehaviour.Instance.cardAnimationSpeed;
        targetPosition = position;
        enabled = true;
        if (playSound && cardAudios.Count > 0)
        {
            var random = new System.Random();
            var audio = cardAudios[random.Next(0, cardAudios.Count)];
            audio.audioSource.time = audio.startTime;
            audio.audioSource.Play();
        }
    }
}
