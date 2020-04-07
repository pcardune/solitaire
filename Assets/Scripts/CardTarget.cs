using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider)), RequireComponent(typeof(SpriteRenderer))]
public class CardTarget : MonoBehaviour
{

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
        initialColor = GetComponent<SpriteRenderer>().color;
    }

    void Start()
    {
    }

    void Update()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        var collider = GetComponent<BoxCollider>();
        if (collider.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo, 1000))
        {
            spriteRenderer.color = hoverColor;
            IsSelected = true;
        }
        else
        {
            spriteRenderer.color = initialColor;
            IsSelected = false;
        }
    }
}
