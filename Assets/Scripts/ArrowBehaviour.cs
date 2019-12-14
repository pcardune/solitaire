using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowBehaviour : MonoBehaviour
{
    public Vector3 StartPos = Vector3.zero;
    public Vector3 EndPos = Vector3.one;
    Transform Tip;
    Transform Line;
    BoxCollider2D collider2D;
    // Start is called before the first frame update
    void Start()
    {
        Tip = transform.Find("Tip");
        Line = transform.Find("Line");
        collider2D = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float length = Vector3.Distance(StartPos, EndPos);
        collider2D.size = new Vector2(collider2D.size.x, length);
        collider2D.offset = new Vector2(collider2D.offset.x, length / 2);
        Line.position = transform.position = StartPos;
        Line.localScale = new Vector3(Line.localScale.x, length - Tip.localScale.y / 2, 1);
        Tip.position = EndPos;
        Line.rotation = Tip.rotation = transform.rotation = Quaternion.FromToRotation(Vector3.up, EndPos - StartPos);
    }
}
