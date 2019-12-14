using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ClickableBehaviour : MonoBehaviour
{

    public Color HoverColor;

    public delegate void OnClickCallback();

    public OnClickCallback OnClick;

    Color oldColor;
    Dictionary<SpriteRenderer, Color> spriteRendererToColor = new Dictionary<SpriteRenderer, Color>();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnMouseEnter()
    {
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
        {
            spriteRendererToColor[sr] = sr.color;
            sr.color = HoverColor;
        }
    }

    public void OnMouseExit()
    {
        foreach (var sr in spriteRendererToColor.Keys)
        {
            sr.color = spriteRendererToColor[sr];
        }
    }

    public void OnMouseUpAsButton()
    {
        if (OnClick != null)
        {
            OnClick();
        }
    }

}
