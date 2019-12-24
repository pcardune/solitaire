using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveCountText : MonoBehaviour
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
        int moves = SolitaireGameBehaviour.Instance.solitaire.moveHistory.Count;
        text.text = $"Moves: {moves}";
    }
}
