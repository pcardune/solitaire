using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableauPileBehaviour : MonoBehaviour
{
    public CardTargetBehaviour cardTarget;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}
