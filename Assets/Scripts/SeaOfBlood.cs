using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaOfBlood : Ocean
{
    FurSealHero Hero = new FurSealHero();


    public Transform[] points;

    int i;
    bool bloodStop = false;
    bool bloodStopTime = true;
    bool randomBlood = true;
    bool organ = true;
    int endGAME ;

    private float waterSpeed = 1f;

    public GameObject UI_Final_Screen;
    public GameObject UI_Score_Panel;
    public GameObject HeroFur;

    //Sound
    [SerializeField] AudioClip bloodWaveSound;
    [SerializeField] AudioClip bloodOrgan;
    [SerializeField] AudioClip endblood;
    AudioSource sourceHero;
    bool checkBloodWaveSound = true;

    private float waitTime;
    public float startWaitTime;

    public List<GameObject> gameObjectsGood;
    public List<GameObject> gameObjectsBad;


    public List<Transform> spawnPoints;

    GameObject randomObject;

    private void Start()
    {
        waitTime = startWaitTime;

        spawnPoints = new List<Transform>(spawnPoints);
    }

    private void SoundOfOrgan()
    {
        GameObject audioSourceFinder = GameObject.Find("AudioSource");
        sourceHero = audioSourceFinder.GetComponent<AudioSource>();
        sourceHero.PlayOneShot(bloodOrgan);
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

            if (organ == true)
            {
                organ = false;
                SoundOfOrgan();
            }

            if (Vector3.Distance(transform.position, points[i].transform.position) < 0.01f)
            {
                if (waitTime <= 0)
                {
                    waitTime = startWaitTime;
                    randomBlood = true;
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
                    if (endGAME == 3 && (waitTime <= 0))
                    {
                        FinishGame();
                    }
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
            endGAME++;

            if (i > 0)
            {
                i = 0;
            }
            else
            {
                if (randomBlood == true)
                {
                    SpawnBlood();
                    randomBlood = false;
                }
                i = 1;
            }
        }
    }

    public void SpawnBlood()
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

            if (randomGoodOrBadObject == 0)
            {
                var objectIndex = Random.Range(0, gameObjectsGood.Count);
                var spawn = Random.Range(0, spawnPoints.Count);

                randomObject = Instantiate(gameObjectsGood[objectIndex], spawnPoints[spawn].transform.position, Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));

                oceanRandomList.Add(randomObject);

            }
            else if (randomGoodOrBadObject == 2 || randomGoodOrBadObject == 1)
            {

                var objectIndex = Random.Range(0, gameObjectsBad.Count);
                var spawn = Random.Range(0, spawnPoints.Count);


                randomObject = Instantiate(gameObjectsBad[objectIndex], spawnPoints[spawn].transform.position, Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));

                oceanRandomList.Add(randomObject);
            }
        }
    }

    private void FinishGame()
    {
        GameObject audioSourceFinder = GameObject.Find("AudioSource");
        sourceHero = audioSourceFinder.GetComponent<AudioSource>();
        sourceHero.PlayOneShot(endblood);


        UI_Score_Panel.SetActive(false);
        UI_Final_Screen.SetActive(true);

        Hero.CheckFinalState();

        Time.timeScale = 0;
    }
}
