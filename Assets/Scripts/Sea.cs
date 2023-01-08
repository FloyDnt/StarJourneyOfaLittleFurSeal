using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Sea : MonoBehaviour
{
    public Transform[] points;

    int i;
    int waterBlood = 0;
    bool waterBloodStop = true;

    private float waterSpeed = 1f;

    private float waitTime;
    public float startWaitTime;


    // UI and BLOOD
    public GameObject WaterObj;
    public GameObject BloodWaterObj;

    private void Start()
    {
        waitTime = startWaitTime;
        
    }

    private void FixedUpdate()
    {
        Water();

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

            if (waterBlood == 2 && waterBloodStop == true)
            {
                changeWaterForBlood();
                waterBloodStop = false;
            }
        }
    }


    private void Water()
    {
        transform.position = Vector3.MoveTowards(transform.position, points[i].transform.position, waterSpeed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, points[0].transform.position) < 0.01f)
        {
            waterBlood++;

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

    private void changeWaterForBlood()
    {
        BloodWaterObj.SetActive(true);
        WaterObj.SetActive(false);
    }
}
