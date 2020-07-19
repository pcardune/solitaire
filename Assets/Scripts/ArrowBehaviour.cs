using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowBehaviour : MonoBehaviour
{
    public Vector3 StartPos = Vector3.zero;
    public Vector3 EndPos = Vector3.one;

    Transform Tip;
    Transform Line;
    Transform Text;
    BoxCollider2D boxCollider2D;
    // Start is called before the first frame update
    void Start()
    {
        Tip = transform.Find("Tip");
        Line = transform.Find("Line");
        Text = transform.Find("Text");
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float length = Vector3.Distance(StartPos, EndPos);
        boxCollider2D.size = new Vector2(boxCollider2D.size.x, length);
        boxCollider2D.offset = new Vector2(boxCollider2D.offset.x, length / 2);
        Line.position = transform.position = StartPos;
        Line.localScale = new Vector3(Line.localScale.x, length - Tip.localScale.y / 2, 1);
        Tip.position = EndPos;
        Line.rotation = Tip.rotation = transform.rotation = Quaternion.FromToRotation(Vector3.up, EndPos - StartPos);
        Vector3 textPos = Vector3.Lerp(StartPos, EndPos, .5f);
        textPos.z = -1.5f;
        Text.position = textPos;
    }

    public void SetColor(Color color)
    {
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
        {
            sr.color = color;
        }
    }
}
