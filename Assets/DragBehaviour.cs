using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragBehaviour : MonoBehaviour
{
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
        mousePosition.z = 0;
        dragOffset = transform.position - mousePosition;
        originalColor = spriteRenderer.color;
        spriteRenderer.color = dragColor;
        spriteRenderer.sortingOrder = 100;
    }

    void OnMouseDrag()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        transform.position = mousePosition + dragOffset;
        Debug.Log("OnMouseDrag");
    }

    void OnMouseUp()
    {
        spriteRenderer.color = originalColor;
    }
}
