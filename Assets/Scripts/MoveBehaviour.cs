using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MoveBehaviour : MonoBehaviour
{
    float timeRemaining = 0f;
    public bool IsMoving
    {
        get
        {
            return timeRemaining > 0;
        }
    }
    Vector3 targetPosition;
    int targetSortingOrder;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (timeRemaining > 0)
        {
            Vector3 delta = (targetPosition - transform.position) / (timeRemaining / Time.deltaTime);
            transform.position += delta;
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                transform.position = targetPosition;
                enabled = false;
                Debug.Log("Done moving " + gameObject.name);
            }
        }
    }

    public void MoveTo(Vector3 position, float aDuration, int sortingOrder)
    {
        Debug.Log("Moving " + gameObject.name + " to " + position);
        timeRemaining = aDuration;
        targetPosition = position;
        targetSortingOrder = sortingOrder;
        enabled = true;
    }
}
