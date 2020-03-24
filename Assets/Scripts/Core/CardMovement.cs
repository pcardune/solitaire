using System;

[Serializable]
public class CardMovement : IEquatable<CardMovement>
{
    public Card card;
    public Location source;
    public Location destination;

    public MoveType type;

    public CardMovement(Card card, Location source, Location destination, MoveType type = MoveType.SingleCard)
    {
        this.card = card;
        this.source = source;
        this.destination = destination;
        this.type = type;
    }

    public CardMovement(LocatedCard locatedCard, Location destination, MoveType type = MoveType.SingleCard)
    {
        card = locatedCard.Card;
        source = locatedCard.Location;
        this.destination = destination;
        this.type = type;
    }

    public override bool Equals(object obj)
    {
        return obj is CardMovement && Equals((CardMovement)obj);
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public bool Equals(CardMovement other)
    {
        return card.Equals(other.card) && source.Equals(other.source) && destination.Equals(other.destination) && type.Equals(other.type);
    }

    override public string ToString()
    {
        return "CardMovement(" + card.ToString() + ", " + source.ToString() + ", " + destination.ToString() + ")";
    }
}