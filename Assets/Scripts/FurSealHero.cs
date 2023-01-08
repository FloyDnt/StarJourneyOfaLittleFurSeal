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

    private void ParticleGen()
    {
        GameObject particleWater = GameObject.Find("P_Clear_Water");
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

        }
        else if (health == 1 && collidedWith.tag == "DangerCube")
        {
            imageHealth_02.color = new Color(255f, 255f, 255f, .5f);
            health++;
        }
        else if (health == 2 && collidedWith.tag == "DangerCube")
        {
            imageHealth_01.color = new Color(255f, 255f, 255f, .5f);
            Destroy(gameObject);

            UI_Death_Screen.SetActive(true);
            Time.timeScale = 0;
        }
    }
}
