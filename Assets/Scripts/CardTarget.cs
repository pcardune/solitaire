using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D)), RequireComponent(typeof(SpriteRenderer))]
public class CardTarget : MonoBehaviour
{

    BoxCollider2D boxCollider2D;
    SpriteRenderer spriteRenderer;
    public Location cardLocation;
    Color initialColor;

    public Color hoverColor;

    public bool IsSelected
    {
        get;
        private set;
    }
    void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialColor = spriteRenderer.color;
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Card target location: " + transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (GetComponent<Collider2D>().OverlapPoint(mousePosition))
        {
            // Debug.Log($"RED: mouse position is {mousePosition}. bounds are: {collider2D.bounds}");
            spriteRenderer.color = hoverColor;
            IsSelected = true;
        }
        else
        {
            // Debug.Log($"GREEN: mouse position is {mousePosition}. bounds are: {collider2D.bounds}");
            spriteRenderer.color = initialColor;
            IsSelected = false;
        }
    }
}
