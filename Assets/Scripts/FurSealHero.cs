using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.UI;

public class FurSealHero : MonoBehaviour
{
    [SerializeField] private GameObject gb;

    [Header("Set in Inspector")]
    public Vector3 toRot;
    public Vector3 toRotHelper;
    public Vector3 zeroVec = new Vector3(0, 360, 0);
    public float speed = 1f;
    public float smooth;

    [Header("Set Dynamically")]
    public Text scoreGT;

    void Start()
    {
        StartCoroutine(AroundHero());

        GameObject scoreGO = GameObject.Find("ScoreCounter");
        scoreGT = scoreGO.GetComponent<Text>();
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
    }
}
