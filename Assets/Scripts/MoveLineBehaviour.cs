using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ClickableBehaviour))]
[RequireComponent(typeof(ArrowBehaviour))]
public class MoveLineBehaviour : MonoBehaviour
{
    public ScoredMove ScoredMove;
    public bool Highlight;
    ArrowBehaviour arrow;

    // Start is called before the first frame update
    void Start()
    {
        arrow = GetComponent<ArrowBehaviour>();
        GetComponent<ClickableBehaviour>().OnClick = OnClick;
    }

    // Update is called once per frame
    void Update()
    {
        var start = SolitaireGameBehaviour.Instance.GetPositionForCardLocation(ScoredMove.Move.Source) + new Vector3(0, 0, -1);
        var end = SolitaireGameBehaviour.Instance.GetPositionForCardLocation(ScoredMove.Move.Destination) + new Vector3(0, 0, -1);
        arrow.StartPos = start;
        arrow.EndPos = end;
        if (Highlight)
        {
            arrow.SetColor(new Color(1, 0, 0));
        }
        else
        {
            arrow.SetColor(new Color(0, 1, 1));
        }
    }

    public void SetScoredMove(ScoredMove scoredMove)
    {
        ScoredMove = scoredMove;
        var text = GetComponentInChildren<TMPro.TextMeshPro>();
        text.text = "" + scoredMove.Score;
    }

    public void OnClick()
    {
        SolitaireGameBehaviour.Instance.PerformAndAnimateMove(ScoredMove.Move);
    }

}
