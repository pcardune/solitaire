using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AITester : MonoBehaviour
{
    private int randomSeed = 1;
    private float lineWidth;
    public int numGames = 100;
    public int numMovesBeforeGivingUp = 1000;
    private int maxMoves = 0;
    private int minMoves = 10000000;
    public GameObject linePrefab;
    public GameObject maxMovesLine;
    public GameObject minMovesLine;
    public new Camera camera;

    void Awake()
    {
        camera = GetComponent<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        var lr = linePrefab.GetComponent<LineRenderer>();
        lineWidth = lr.startWidth = lr.endWidth = camera.orthographicSize * 2 / numGames;
    }

    private Vector3 screenPos(Vector3 pos)
    {
        float xSize = camera.orthographicSize * 2;
        Rect rect = new Rect(-camera.orthographicSize, camera.orthographicSize, camera.orthographicSize * 2, camera.orthographicSize * 2);
        return new Vector3(
            rect.x + rect.width * pos.x / numMovesBeforeGivingUp,
            rect.y - pos.y * lineWidth,
            0
        );
    }

    private Vector3 screenPos(int x, int y)
    {
        return screenPos(new Vector3(x, y, 0));
    }

    // Update is called once per frame
    void Update()
    {
        if (randomSeed > numGames)
        {
            return;
        }
        var s = new Solitaire(randomSeed++);
        s.DealAll();
        bool success;
        s.PerformSmartMoves(new MoveSelector(1), numMovesBeforeGivingUp, out success);
        int numMoves = s.moveHistory.Count;
        Debug.Log("Finished game " + s.RandomSeed + ": " + numMoves + " moves");

        var lineObj = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        lineObj.GetComponent<LineRenderer>().SetPositions(new Vector3[]{
            screenPos(0, randomSeed),
            screenPos(numMoves < numMovesBeforeGivingUp ? numMoves : 0, randomSeed),
        });
        lineObj.SetActive(true);

        if (numMoves < numMovesBeforeGivingUp - 2)
        {
            Debug.Log("Updating maxMoves. maxMoves=" + maxMoves + " numMoves=" + numMoves + " ... new max moves will be " + Mathf.Max(maxMoves, numMoves));
            maxMoves = Mathf.Max(maxMoves, numMoves);
            Debug.Log("maxMoves is now.... maxMoves=" + maxMoves);
            minMoves = Mathf.Min(minMoves, numMoves);
        }
        maxMovesLine.transform.SetPositionAndRotation(screenPos(maxMoves, 0), Quaternion.identity);
        maxMovesLine.GetComponentInChildren<Text>().text = "" + maxMoves;
        minMovesLine.transform.SetPositionAndRotation(screenPos(minMoves, 0), Quaternion.identity);
        minMovesLine.GetComponentInChildren<Text>().text = "" + minMoves;
    }
}
