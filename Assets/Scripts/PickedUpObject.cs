using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickedUpObject : MonoBehaviour
{
    [SerializeField] GameObject pClearWater;

    private void OnCollisionEnter(Collision collision)
    {
        GameObject collidedWith = collision.gameObject;
        if (collidedWith.tag == "Player")
        {
            Destroy(gameObject);
            GameObject explosion = Instantiate(pClearWater);
            Destroy(explosion, 0.75f);

        }
    }
}
