using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Sea : MonoBehaviour
{
    public Transform[] points;

    int i;

    private float waterSpeed = 1f;

    private float waitTime;
    public float startWaitTime;

    private void Start()
    {
        waitTime = startWaitTime;
    }

    private void FixedUpdate()
    {
        Test();

        if (Vector3.Distance(transform.position, points[i].transform.position) < 0.01f)
        {
            if (waitTime <= 0)
            {
                waitTime = startWaitTime;
                if (i > 0)
                {
                    i = 0;
                }
                else
                {
                    i = 1;
                }
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }
    }


    private void Test()
    {
        transform.position = Vector3.MoveTowards(transform.position, points[i].transform.position, waterSpeed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, points[0].transform.position) < 0.01f)
        {
            if (i > 0)
            {
                i = 0;
            }
            else
            {
                i = 1;
            }
        }

    }

    private IEnumerator HighTide()
    {
        yield return new WaitForSeconds(5f);

        transform.position = Vector3.MoveTowards(transform.position, points[i].transform.position, waterSpeed * Time.fixedDeltaTime);
    }

    private void LowTide()
    {
        transform.position = Vector3.MoveTowards(transform.position, points[i].transform.position, waterSpeed * Time.fixedDeltaTime);
    }

    private IEnumerator Pause()
    {
        yield return new WaitForSeconds(30f);
    }
}
