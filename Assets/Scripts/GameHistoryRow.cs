using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHistoryRow : MonoBehaviour
{

    public Text DateText;
    public Text MoveText;
    public Text DurationText;
    public Text WinText;
    public Button LoadButton;

    private GameReference _gameRef;
    public GameReference gameReference;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"Loading game history row {gameReference.startTime} {gameReference.moveCount} {gameReference.duration}");
        DateText.text = gameReference.startTime.ToShortDateString();
        MoveText.text = $"Moves: {gameReference.moveCount}";
        DurationText.text = TimeText.FormatDuration(gameReference.duration);
        WinText.text = gameReference.isWon ? "Win!" : "";
    }

    // Update is called once per frame
    void Update()
    {

    }
}
