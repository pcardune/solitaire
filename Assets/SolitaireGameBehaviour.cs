using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolitaireGameBehaviour : MonoBehaviour
{

    DeckBehaviour deck;
    TableauBehaviour tableau;

    // Start is called before the first frame update
    void Start()
    {
        deck = FindObjectOfType<DeckBehaviour>();
        tableau = FindObjectOfType<TableauBehaviour>();
        DealCards();
    }

    async void DealCards()
    {
        TableauPileBehaviour[] piles = tableau.GetTableauPiles();
        for (int i = 0; i < piles.Length; i++)
        {
            for (int j = 0; j < i + 1; j++)
            {
                CardBehaviour card = deck.GetTopCard();
                card.transform.parent = piles[i].transform;
                if (j == i)
                {
                    card.SetFaceUp(true);
                }
                await card.GetComponent<MoveBehaviour>().MoveTo(piles[i].cardTarget.transform.position, .1f, j);
                piles[i].cardTarget.transform.position += new Vector3(0, -0.3f);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
