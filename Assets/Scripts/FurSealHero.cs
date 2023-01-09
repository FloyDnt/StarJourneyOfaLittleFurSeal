using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.UI;
using TMPro;

public class FurSealHero : MonoBehaviour
{

    Rigidbody rb;

    public Image imageHealth_01;
    public Image imageHealth_02;
    public Image imageHealth_03;

    public GameObject UI_Death_Screen;
    public GameObject UI_Death_Blood_Screen;
    public GameObject WaterCheck;
    public GameObject BloodWaterCheck;

    public GameObject UI_Final_Screen;

    //Particle
    public ParticleSystem pClearWater = null;
    public ParticleSystem pBloodWater = null;
    public ParticleSystem pClearDamage = null;
    public ParticleSystem pBloodDamage = null;

    //Sound
    AudioSource sourceHero;
    [SerializeField] AudioClip clip_jump_1;
    [SerializeField] AudioClip clip_jump_2;
    [SerializeField] AudioClip clip_sad_1;
    [SerializeField] AudioClip clip_sad_2;
    [SerializeField] AudioClip soundOfDeathBlood;

    int health = 0;
    public static int score;
    public static int scoreOfBank;

    Color healthColor;

    [SerializeField] private GameObject gb;

    [Header("Set in Inspector")]
    public Vector3 toRot;
    public Vector3 toRotHelper;
    public Vector3 zeroVec = new Vector3(0, 360, 0);
    public float speed = 1f;
    public float smooth;
    public float jumpForce = 7f;

    [Header("Set Dynamically")]
    public TextMeshProUGUI scoreGT;
    public TextMeshProUGUI finalScore;

    bool b_final_screen;


    void Start()
    {
        rb = GetComponent<Rigidbody>();

        StartCoroutine(AroundHero());

        ScoreGen();

        HealthGen();

        b_final_screen = UI_Final_Screen.activeInHierarchy;
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
        scoreGT.text = "Score: 0";
    }

    void Update()
    {
        JumpHero();

        scoreOfBank = score;
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
            PlayParticle();

            score += 100;
            scoreGT.text = "Score: " + score;

        }
        
        if (collidedWith.tag == "DangerCube")
        {
            DamageParticle();
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
            else { 

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

    public void PlayParticle()
    {
        bool b_waterCheck = WaterCheck.activeInHierarchy;

        if (b_waterCheck == true )
        {
            pClearWater.Play();
        }
        else if (b_waterCheck == false)
        {
            pBloodWater.Play();
        }
    }

    public void DamageParticle()
    {
        bool b_waterCheckBlood = BloodWaterCheck.activeInHierarchy;

        if (b_waterCheckBlood == false)
        {
            pClearDamage.Play();
        }
        else if (b_waterCheckBlood == true)
        {
            pBloodDamage.Play();
        }
    }

    public void CheckFinalState()
    {
        GameObject finalBS = GameObject.Find("ScoreBlood");
        finalScore = finalBS.GetComponent<TextMeshProUGUI>();
        finalScore.text = "CURRENT SCORE: ";

        finalScore.text = "CURRENT SCORE: " + scoreOfBank;
    }

}
