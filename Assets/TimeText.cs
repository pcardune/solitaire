using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TimeText : MonoBehaviour
{
    Text text;
    // Start is called before the first frame update
    void Awake()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        var dt = SolitaireGameBehaviour.Instance.GameDuration;
        text.text = $"Time: {Math.Truncate(dt / 60)}:{Math.Truncate(dt % 60).ToString("00")}";
    }
}
