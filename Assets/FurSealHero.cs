using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class FurSealHero : MonoBehaviour
{
    Rigidbody rigid;

    [SerializeField] private GameObject gb;

    [Header("Set in Inspector")]
    public Vector3 toRot;
    public Vector3 toRotHelper;
    public Vector3 zeroVec = new Vector3(0, 360, 0);
    public int lungeImpulse = 5000;
    public float speed = 1f;

    public float smooth;

    void Start()
    {
        StartCoroutine(AroundHero());
    }

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
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
}
