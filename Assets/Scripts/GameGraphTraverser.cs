using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameGraphTraverser : MonoBehaviour
{
    public Text mainText;
    public int randomSeed = 2;

    public GameObject graphNodePrefab;
    public GameObject graphEdgePrefab;
    private GameObject startNode;
    private int depth = 1;

    private List<GameGraphNode> nextGames = new List<GameGraphNode>();

    // Start is called before the first frame update
    void Start()
    {
        mainText.text = "Hello World";

        var s = new Solitaire(randomSeed);
        s.DealAll();
        nextGames.Add(InstantiateGraphNode(s, Vector3.zero));
    }

    private GameGraphNode InstantiateGraphNode(Solitaire s, Vector3 position)
    {
        startNode = Instantiate(graphNodePrefab, position, graphNodePrefab.transform.rotation);
        var ggNode = startNode.GetComponent<GameGraphNode>();
        ggNode.gameBytes = s.ToBytes();
        ggNode.depth = depth;
        return ggNode;
    }

    private GameGraphEdge InstantiateGraphEdge(GameGraphNode fromNode, GameGraphNode toNode)
    {
        var edge = Instantiate(graphEdgePrefab, Vector3.zero, Quaternion.identity).GetComponent<GameGraphEdge>();
        edge.fromNode = fromNode;
        edge.toNode = toNode;
        edge.toNode.GetComponent<SpringJoint>().connectedBody = fromNode.GetComponent<Rigidbody>();
        return edge;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var newNextGames = new List<GameGraphNode>();
            depth++;
            int nodeIndex = 0;
            foreach (var ggNode in nextGames)
            {
                var solitaire = Solitaire.FromBytes(ggNode.gameBytes);
                int moveIndex = 0;
                foreach (var move in solitaire.GetScoredMoves())
                {
                    var s = Solitaire.FromBytes(ggNode.gameBytes);
                    s.PerformMove(move.Move);
                    var nextNode = InstantiateGraphNode(s, ggNode.gameObject.transform.position + Vector3.right + Vector3.up * (moveIndex + 1));
                    InstantiateGraphEdge(ggNode, nextNode);
                    newNextGames.Add(nextNode);
                    moveIndex++;
                }
                nodeIndex++;
            }
            nextGames = newNextGames;
        }
    }
}
