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
            return timeRemaining > 0;
        }
    }
    Vector3 targetPosition;
    int targetSortingOrder;
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
            Vector3 delta = (targetPosition - transform.position) / (timeRemaining / Time.deltaTime);
            var pos = transform.position + delta;
            pos.z = -.52f;
            transform.position = pos;
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                transform.position = targetPosition;
                enabled = false;
                Debug.Log("Done moving " + gameObject.name);
            }
        }
    }

    public void MoveTo(Vector3 position, float aDuration, int sortingOrder, bool playSound = true)
    {
        Debug.Log("Moving " + gameObject.name + " to " + position);
        timeRemaining = aDuration;
        targetPosition = position;
        targetSortingOrder = sortingOrder;
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
