using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DealDamage : MonoBehaviour
{
    void Start()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject collidedWith = collision.gameObject;
        if (collidedWith.tag == "Player")
        {
            Destroy(gameObject);
        }
    }
}
