using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableauBehaviour : MonoBehaviour
{
    TableauPileBehaviour[] piles = new TableauPileBehaviour[7];
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            piles[i] = transform.GetChild(i).GetComponent<TableauPileBehaviour>();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public TableauPileBehaviour[] GetTableauPiles()
    {
        return piles;
    }
}
