﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ClickableBehaviour))]
[RequireComponent(typeof(ArrowBehaviour))]
public class MoveLineBehaviour : MonoBehaviour
{
    public CardMovement Move;
    ArrowBehaviour arrow;

    public bool Highlight = false;

    // Start is called before the first frame update
    void Start()
    {
        arrow = GetComponent<ArrowBehaviour>();
        GetComponent<ClickableBehaviour>().OnClick = OnClick;
    }

    // Update is called once per frame
    void Update()
    {
        var start = SolitaireGameBehaviour.Instance.GetPositionForCardLocation(Move.Source) + new Vector3(0, 0, -1);
        var end = SolitaireGameBehaviour.Instance.GetPositionForCardLocation(Move.Destination) + new Vector3(0, 0, -1);
        arrow.StartPos = start;
        arrow.EndPos = end;

        if (Highlight)
        {
            arrow.SetColor(Color.red);
        }
        else
        {
            arrow.SetColor(Color.yellow);
        }
    }

    public void SetMove(CardMovement move)
    {
        Move = move;
    }

    public void OnClick()
    {
        SolitaireGameBehaviour.Instance.PerformAndAnimateMove(Move);
    }

}
