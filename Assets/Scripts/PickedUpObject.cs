using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickedUpObject : MonoBehaviour
{

    [SerializeField] AudioClip clip_1;
    [SerializeField] AudioClip clip_2;

    AudioSource sourceHero;

    private void OnCollisionEnter(Collision collision)
    {
        GameObject collidedWith = collision.gameObject;
        if (collidedWith.tag == "Player")
        {
            int sound = Random.Range(0, 2);

            if (sound == 0)
            {
                GameObject audioSourceFinder = GameObject.Find("AudioSource");
                sourceHero = audioSourceFinder.GetComponent<AudioSource>();
                sourceHero.PlayOneShot(clip_1);

            }
            else if (sound == 1)
            {
                GameObject audioSourceFinder = GameObject.Find("AudioSource");
                sourceHero = audioSourceFinder.GetComponent<AudioSource>();
                sourceHero.PlayOneShot(clip_2);
            }

            Destroy(gameObject);

        }
    }
}
