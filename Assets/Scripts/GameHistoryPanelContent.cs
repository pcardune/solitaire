using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHistoryPanelContent : MonoBehaviour
{

    public GameHistoryRow RowPrefab;

    // Start is called before the first frame update
    void Start()
    {
        List<GameReference> refs = GameHistoryManager.Instance.gameHistoryIndex;
        Debug.Log("Loading " + refs.Count + " games");
        foreach (GameReference gameRef in refs)
        {
            Debug.Log("Loading game ref " + gameRef);
            GameHistoryRow row = Instantiate(RowPrefab).GetComponent<GameHistoryRow>();
            row.gameReference = gameRef;
            row.transform.SetParent(this.transform, false);
            // row.transform.parent = this.transform;
        }
        // GameHistoryManager.Instance.
    }

    // Update is called once per frame
    void Update()
    {

    }
}
