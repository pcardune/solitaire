using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CardSpriteManager
{
    private static CardSpriteManager instance;
    private Dictionary<string, Sprite> cardFrontSprites = new Dictionary<string, Sprite>();
    private Dictionary<string, Texture> cardFrontTextures = new Dictionary<string, Texture>();
    private Sprite backOfCardSprite;
    public static CardSpriteManager Load()
    {
        if (CardSpriteManager.instance == null)
        {
            CardSpriteManager.instance = new CardSpriteManager();

            Dictionary<string, Suit> suits = new Dictionary<string, Suit>(){
                {"Clubs", Suit.Clubs},
                {"Diamonds", Suit.Diamonds},
                {"Hearts", Suit.Hearts},
                {"Spades", Suit.Spades},
            };
            string[] values = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
            foreach (var suit in suits)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    string path = "CardImages/card" + suit.Key + values[i];
                    Card card = new Card(suit.Value, i + 1);
                    instance.cardFrontSprites[card.ToString()] = Resources.Load<Sprite>(path);
                    instance.cardFrontTextures[card.ToString()] = Resources.Load<Texture>(path);
                }
            }
            CardSpriteManager.instance.backOfCardSprite = Resources.Load<Sprite>("CardImages/cardBack_red5");
        }
        return CardSpriteManager.instance;
    }

    public Texture GetTextureForCard(Card card)
    {
        return cardFrontTextures[card.ToString()];
    }

    public Sprite GetSpriteForCard(Card card)
    {
        return cardFrontSprites[card.ToString()];
    }

    public Sprite GetFaceDownSprite()
    {
        return backOfCardSprite;
    }
}

[RequireComponent(typeof(DragBehaviour)), RequireComponent(typeof(MoveBehaviour))]
public class CardBehaviour : MonoBehaviour
{

    public LocatedCard locatedCard;

    public Card card { get { return locatedCard.Card; } }

    public Location cardLocation { get { return locatedCard.Location; } }

    public float dragDelay = 0.5f;

    float dragStart;

    SpriteRenderer spriteRenderer;
    MeshRenderer meshRenderer;
    DragBehaviour dragBehaviour;
    MoveBehaviour moveBehaviour;

    public DragBehaviour Drag
    {
        get
        {
            return dragBehaviour;
        }
    }

    public MoveBehaviour Move
    {
        get
        {
            return moveBehaviour;
        }
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        dragBehaviour = GetComponent<DragBehaviour>();
        moveBehaviour = GetComponent<MoveBehaviour>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (spriteRenderer != null)
        {
            if (cardLocation.faceUp)
            {
                spriteRenderer.sprite = CardSpriteManager.Load().GetSpriteForCard(card);
            }
            else
            {
                spriteRenderer.sprite = CardSpriteManager.Load().GetFaceDownSprite();
            }
        }
        if (meshRenderer != null)
        {
            meshRenderer.materials[2].mainTexture = CardSpriteManager.Load().GetTextureForCard(card);
            if (cardLocation.faceUp)
            {
                transform.localRotation = Quaternion.identity;
            }
            else
            {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }

    void OnMouseUpAsButton()
    {
        SolitaireGameBehaviour.Instance.OnClickCard(this);
    }

    void OnMouseDown()
    {
        SolitaireGameBehaviour.Instance.OnMouseDownCard(this);
    }

    void OnMouseDrag()
    {
        if (dragBehaviour.DragDuration > 0.1f)
        {
            SolitaireGameBehaviour.Instance.OnMouseDragCard(this);
        }
    }

    void OnMouseUp()
    {
        SolitaireGameBehaviour.Instance.OnMouseUpCard(this);
    }

}
