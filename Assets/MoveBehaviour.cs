using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MoveBehaviour : MonoBehaviour
{
    public delegate void OnMoveComplete();

    bool isMoving = false;
    float timeRemaining = 0f;
    Vector3 targetPosition;
    int targetSortingOrder;
    OnMoveComplete onMoveComplete;
    TaskCompletionSource<MoveBehaviour> moveCompletion;
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
                GetComponent<SpriteRenderer>().sortingOrder = targetSortingOrder;
                enabled = false;
                if (onMoveComplete != null)
                {
                    onMoveComplete();
                }
                moveCompletion.SetResult(this);
                Debug.Log("Done moving " + gameObject.name);
            }
        }
    }

    public Task MoveTo(Vector3 position, float aDuration, int sortingOrder, OnMoveComplete onComplete = null)
    {
        Debug.Log("Moving " + gameObject.name + " to " + position);
        timeRemaining = aDuration;
        targetPosition = position;
        targetSortingOrder = sortingOrder;
        onMoveComplete = onComplete;
        enabled = true;

        moveCompletion = new TaskCompletionSource<MoveBehaviour>();
        return moveCompletion.Task;
    }
}
