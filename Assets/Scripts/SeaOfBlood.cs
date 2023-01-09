using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaOfBlood : MonoBehaviour
{
    public Transform[] points;

    int i;
    bool bloodStop = false;
    bool bloodStopTime = true;

    private float waterSpeed = 1f;

    //Sound
    [SerializeField] AudioClip bloodWaveSound;
    AudioSource sourceHero;
    bool checkBloodWaveSound = true;

    private float waitTime;
    public float startWaitTime;

    private void Start()
    {
        waitTime = startWaitTime;
    }

    private void FixedUpdate()
    {
        if (bloodStop == false && waitTime >= 0)
        {
            checkBloodWaveSound = true;
            waitTime -= Time.deltaTime;
        }
        else
        {
            bloodStop = true;

            if (bloodStopTime == true)
            {
                waitTime = startWaitTime;
                bloodStopTime = false;
            }

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
            }
        }
    }


    private void Water()
    {
        transform.position = Vector3.MoveTowards(transform.position, points[i].transform.position, waterSpeed * Time.fixedDeltaTime);

        if (checkBloodWaveSound == true)
        {
            checkBloodWaveSound = false;

            GameObject audioSourceFinder = GameObject.Find("AudioSource");
            sourceHero = audioSourceFinder.GetComponent<AudioSource>();
            sourceHero.PlayOneShot(bloodWaveSound);
        }

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
}
