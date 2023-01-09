using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.UI;
using TMPro;

public class FurSealHero : MonoBehaviour
{
    public Image imageHealth_01;
    public Image imageHealth_02;
    public Image imageHealth_03;

    public GameObject UI_Death_Screen;
    public GameObject UI_Death_Blood_Screen;
    public GameObject WaterCheck;
    public GameObject BloodWaterCheck;

    //Sound
    AudioSource sourceHero;
    [SerializeField] AudioClip clip_jump_1;
    [SerializeField] AudioClip clip_jump_2;
    [SerializeField] AudioClip clip_sad_1;
    [SerializeField] AudioClip clip_sad_2;
    [SerializeField] AudioClip soundOfDeathBlood;

    int health = 0;

    Color healthColor;

    [SerializeField] private GameObject gb;

    [Header("Set in Inspector")]
    public Vector3 toRot;
    public Vector3 toRotHelper;
    public Vector3 zeroVec = new Vector3(0, 360, 0);
    public float speed = 1f;
    public float smooth;

    [Header("Set Dynamically")]
    public TextMeshProUGUI scoreGT;

    void Start()
    {
        StartCoroutine(AroundHero());

        ScoreGen();

        HealthGen();
    }

    private void HealthGen()
    {
        healthColor = new Color(255f, 255f, 255f);

        GameObject imageFindHealth_01 = GameObject.Find("Health_01");
        imageHealth_01 = imageFindHealth_01.GetComponent<Image>();
        imageHealth_01.color = healthColor;

        GameObject imageFindHealth_02 = GameObject.Find("Health_02");
        imageHealth_02 = imageFindHealth_02.GetComponent<Image>();
        imageHealth_02.color = healthColor;

        GameObject imageFindHealth_03 = GameObject.Find("Health_03");
        imageHealth_03 = imageFindHealth_03.GetComponent<Image>();
        imageHealth_03.color = healthColor;
    }

    private void ScoreGen()
    {
        GameObject scoreGO = GameObject.Find("Score_Text_Box");
        scoreGT = scoreGO.GetComponent<TextMeshProUGUI>();
        scoreGT.text = "0";
    }

    void Update()
    {
        JumpHero();
    }

    public bool vectorEquals(Vector3 v1, Vector3 v2)
    {
        if (v1.Equals(v2))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private IEnumerator AroundHero()
    {
        while (true)
        {
            if (!vectorEquals(zeroVec, toRotHelper))
            {
                toRotHelper = toRotHelper + toRot;
            }
            else
            {
                toRotHelper = toRot;
            }

            this.transform.rotation = Quaternion.Euler(toRotHelper);

            yield return new WaitForSeconds(smooth);
        }
    }

    private void JumpHero()
    {
        if (Input.GetMouseButtonDown(0))
        {
            transform.position = Vector3.MoveTowards(transform.position, gb.transform.position, speed);

            int sound = Random.Range(0, 2);

            if (sound == 0)
            {
                GameObject audioSourceFinder = GameObject.Find("AudioSource");
                sourceHero = audioSourceFinder.GetComponent<AudioSource>();
                sourceHero.PlayOneShot(clip_jump_1);

            }
            else if (sound == 1)
            {
                GameObject audioSourceFinder = GameObject.Find("AudioSource");
                sourceHero = audioSourceFinder.GetComponent<AudioSource>();
                sourceHero.PlayOneShot(clip_jump_2);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject collidedWith = collision.gameObject;
        if (collidedWith.tag == "Cube")
        {
            int score = int.Parse(scoreGT.text);
            score += 100;
            scoreGT.text = score.ToString();

            if (score > HighScoreResult.scoreResult)
            {
                HighScoreResult.scoreResult = score;
            }
        }

        if (health == 0 && collidedWith.tag == "DangerCube")
        {
            imageHealth_03.color = new Color(255f, 255f, 255f, .5f);
            health++;

            int sound = Random.Range(0, 2);

            if (sound == 0)
            {
                GameObject audioSourceFinder = GameObject.Find("AudioSource");
                sourceHero = audioSourceFinder.GetComponent<AudioSource>();
                sourceHero.PlayOneShot(clip_sad_1);

            }
            else if (sound == 1)
            {
                GameObject audioSourceFinder = GameObject.Find("AudioSource");
                sourceHero = audioSourceFinder.GetComponent<AudioSource>();
                sourceHero.PlayOneShot(clip_sad_2);
            }

        }
        else if (health == 1 && collidedWith.tag == "DangerCube")
        {
            imageHealth_02.color = new Color(255f, 255f, 255f, .5f);
            health++;

            int sound = Random.Range(0, 2);

            if (sound == 0)
            {
                GameObject audioSourceFinder = GameObject.Find("AudioSource");
                sourceHero = audioSourceFinder.GetComponent<AudioSource>();
                sourceHero.PlayOneShot(clip_sad_1);

            }
            else if (sound == 1)
            {
                GameObject audioSourceFinder = GameObject.Find("AudioSource");
                sourceHero = audioSourceFinder.GetComponent<AudioSource>();
                sourceHero.PlayOneShot(clip_sad_2);
            }
        }
        else if (health == 2 && collidedWith.tag == "DangerCube")
        {
            int sound = Random.Range(0, 2);

            if (sound == 0)
            {
                GameObject audioSourceFinder = GameObject.Find("AudioSource");
                sourceHero = audioSourceFinder.GetComponent<AudioSource>();
                sourceHero.PlayOneShot(clip_sad_1);

            }
            else if (sound == 1)
            {
                GameObject audioSourceFinder = GameObject.Find("AudioSource");
                sourceHero = audioSourceFinder.GetComponent<AudioSource>();
                sourceHero.PlayOneShot(clip_sad_2);
            }

            imageHealth_01.color = new Color(255f, 255f, 255f, .5f);
            Destroy(gameObject);

            bool b_waterCheck = WaterCheck.activeInHierarchy; 
            bool b_bloodWaterCheck = BloodWaterCheck.activeInHierarchy;

            if (b_waterCheck == true)
            {
                UI_Death_Screen.SetActive(true);

            }
            else if (b_bloodWaterCheck == true)
            {
                UI_Death_Blood_Screen.SetActive(true);
                SoundOfDeathBlood();
            }

            Time.timeScale = 0;
        }
    }

    private void SoundOfDeathBlood()
    {
        GameObject audioSourceFinder = GameObject.Find("AudioSource");
        sourceHero = audioSourceFinder.GetComponent<AudioSource>();
        sourceHero.PlayOneShot(soundOfDeathBlood);
    }
}
