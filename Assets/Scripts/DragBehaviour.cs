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

    private float _dragStartTime;

    private bool _isDragging = false;
    public bool IsDragging
    {
        get
        {
            return _isDragging;
        }
        private set
        {
            _isDragging = value;
        }
    }

    public float DragDuration
    {
        get
        {
            if (IsDragging)
            {
                return Time.time - _dragStartTime;
            }
            return 0f;
        }
    }

    public float DragDistance
    {
        get
        {
            if (IsDragging)
            {
                return dragOffset.magnitude;
            }
            return 0f;
        }
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseDown()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        initialPosition = transform.position;
        dragOffset = transform.position - mousePosition;
        originalColor = spriteRenderer.color;
        spriteRenderer.color = dragColor;
    }

    void OnMouseDrag()
    {
        if (!IsDragging)
        {
            IsDragging = true;
            _dragStartTime = Time.time;
        }
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var position = mousePosition + dragOffset;
        position.z = -1f;
        transform.position = position;
    }

    void OnMouseUp()
    {
        IsDragging = false;
        spriteRenderer.color = originalColor;
    }

    public void Reset()
    {
        transform.position = initialPosition;
    }
}
