using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGraphEdge : MonoBehaviour
{
    public GameGraphNode fromNode;
    public GameGraphNode toNode;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (fromNode != null && toNode != null)
        {
            var line = GetComponent<LineRenderer>();
            line.SetPosition(0, fromNode.transform.position);
            line.SetPosition(1, toNode.transform.position);
        }
    }
}
