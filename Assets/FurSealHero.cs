using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class FurSealHero : MonoBehaviour
{
    Rigidbody rigid;

    [Header("Set in Inspector")]
    public Vector3 toRot;
    public Vector3 toRotHelper;
    public Vector3 zeroVec = new Vector3(0, 360, 0);
    public int lungeImpulse = 5000;
    public float speed = 5f;

    public float smooth;

    void Start()
    {

    }

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
                JumpHero();

        AroundHero();

        // Debug.Log(toRotHelper);
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

    private void AroundHero()
    {
        Quaternion var = transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(toRotHelper), smooth);
        var = transform.rotation;

        if (vectorEquals(zeroVec, toRotHelper))
        {
            toRotHelper = toRot;
        }
        else
        {
            toRotHelper = toRotHelper + toRot;
        }
    }

    private void JumpHero()
    {
        if (Input.GetMouseButtonDown(0))
        {
            for (int i = 45; i <= 360; i = i + 45)
            {
                if (vectorEquals(toRotHelper, new Vector3(0, i, 0)))
                {
                    switch(i)
                    {
                        case 45:
                            transform.position += new Vector3(1, 0, 1);
                            break;

                        case 90:
                            transform.position += new Vector3(0, 0, 1);
                            break;

                        case 135:
                            transform.position += new Vector3(-1, 0, 1);
                            break;

                        case 180:
                            transform.position += new Vector3(-1, 0, 0);
                            break;

                        case 225:
                            transform.position += new Vector3(-1, 0, -1);
                            break;

                        case 270:
                            transform.position += new Vector3(0, 0, -1);
                            break;

                        case 315:
                            transform.position += new Vector3(0, 0, -1);
                            break;

                        case 360:
                            transform.position += new Vector3(1, 0, 0);
                            break;
                    }    
                }
            }
        }
    }
}
