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
    private Plane _plane;

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
        _plane = new Plane(transform.TransformPoint(Vector3.left), transform.TransformPoint(Vector3.right), transform.TransformPoint(Vector3.up));
        initialPosition = transform.position;
        dragOffset = initialPosition - getMousePosition(_plane);
        originalColor = spriteRenderer.color;
        spriteRenderer.color = dragColor;
    }

    static Vector3 getMousePosition(Plane plane)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    void OnMouseDrag()
    {
        if (!IsDragging)
        {
            IsDragging = true;
            _dragStartTime = Time.time;
        }
        transform.position = getMousePosition(_plane) + dragOffset;
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
