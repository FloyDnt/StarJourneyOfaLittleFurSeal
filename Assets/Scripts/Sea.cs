using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Sea : Ocean
{

    public Transform[] points;

    int i;
    int waterBlood = 0;
    bool waterBloodStop = true;
    bool checkWaveSound = true;
    bool random = true;

    private float waterSpeed = 1f;

    private float waitTime;
    public float startWaitTime;

    //Sound
    [SerializeField] AudioClip waveSound;
    AudioSource sourceHero;
    [SerializeField] GameObject mainTrackSource;
    [SerializeField] GameObject bloodTrackSource;


    // UI and BLOOD
    public GameObject WaterObj;
    public GameObject BloodWaterObj;
    public Image oldHealthImage_1;
    public Image oldHealthImage_2;
    public Image oldHealthImage_3;
    public Image oldCloseImage;
    public Sprite newHealthImage;
    public Sprite newCloseImage;

    public List<GameObject> gameObjectsGood;
    public List<GameObject> gameObjectsBad;

    public List<Transform> spawnPoints;

    GameObject randomObject;

    private void Start()
    {
        waitTime = startWaitTime;
        spawnPoints = new List<Transform>(spawnPoints);
    }


    private void FixedUpdate()
    {
        Water();

        if (Vector3.Distance(transform.position, points[i].transform.position) < 0.01f)
        {
            if (waitTime <= 0)
            {
                checkWaveSound = true;
                random = true;
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

            if (waterBlood == 7 && waterBloodStop == true)
            {
                changeWaterForBlood();
                ChangeBloodUI();
                ChangeSound();
                waterBloodStop = false;
            }
        }
    }


    private void Water()
    {
        transform.position = Vector3.MoveTowards(transform.position, points[i].transform.position, waterSpeed * Time.fixedDeltaTime);

        if (checkWaveSound == true)
        {
            checkWaveSound = false;

            GameObject audioSourceFinder = GameObject.Find("AudioSource");
            sourceHero = audioSourceFinder.GetComponent<AudioSource>();
            sourceHero.PlayOneShot(waveSound);
        }

        if (Vector3.Distance(transform.position, points[0].transform.position) < 0.01f)
        {
            waterBlood++;

            if (i > 0)
            {
                i = 0;
            }
            else
            {
                if (random == true)
                {
                    Spawn();
                    random = false;
                }
                i = 1;
            }
        }
    }

    private void changeWaterForBlood()
    {
        BloodWaterObj.SetActive(true);
        WaterObj.SetActive(false);
    }

    private void ChangeBloodUI()
    {
        oldHealthImage_1.sprite = newHealthImage;
        oldHealthImage_2.sprite = newHealthImage;
        oldHealthImage_3.sprite = newHealthImage;
        oldCloseImage.sprite = newCloseImage;
    }

    private void ChangeSound()
    {
        mainTrackSource.SetActive(false);
        bloodTrackSource.SetActive(true);
    }


    public void Spawn()
    {

        for (int i = 0; i != oceanRandomList.Count; i++)
        {
            if (oceanRandomList != null)
            {
                Destroy(oceanRandomList[i].gameObject);
            }
        }

        oceanRandomList.Clear();


        for (int i = 0; i <= 9; i++)
        {
            var randomGoodOrBadObject = Random.Range(0, 3);

            if (randomGoodOrBadObject == 0 || randomGoodOrBadObject == 1)
            {
                var objectIndex = Random.Range(0, gameObjectsGood.Count);
                var spawn = Random.Range(0, spawnPoints.Count);

                randomObject = Instantiate(gameObjectsGood[objectIndex], spawnPoints[spawn].transform.position, Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));

                oceanRandomList.Add(randomObject);

            }
            else if (randomGoodOrBadObject == 2)
            {

                var objectIndex = Random.Range(0, gameObjectsBad.Count);
                var spawn = Random.Range(0, spawnPoints.Count);


                randomObject = Instantiate(gameObjectsBad[objectIndex], spawnPoints[spawn].transform.position, Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));

                oceanRandomList.Add(randomObject);
            }
        }
    }
}
