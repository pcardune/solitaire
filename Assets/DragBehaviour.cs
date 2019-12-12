using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragBehaviour : MonoBehaviour
{
    Vector3 initialPosition;
    int initialOrder;
    Vector3 dragOffset;
    SpriteRenderer spriteRenderer;

    Color originalColor;

    public Color dragColor = new Color(1, 1, 1, .8f);

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseDown()
    {
        Debug.Log("OnMouseDown");
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        initialPosition = transform.position;
        dragOffset = transform.position - mousePosition;
        originalColor = spriteRenderer.color;
        spriteRenderer.color = dragColor;
        initialOrder = GetComponent<CardBehaviour>().GetOrder();
        GetComponent<CardBehaviour>().SetOrder(100);
    }

    void OnMouseDrag()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mousePosition + dragOffset;
        Debug.Log("OnMouseDrag");
    }

    void OnMouseUp()
    {
        spriteRenderer.color = originalColor;
        transform.position = initialPosition;
        GetComponent<CardBehaviour>().SetOrder(initialOrder);
    }
}
