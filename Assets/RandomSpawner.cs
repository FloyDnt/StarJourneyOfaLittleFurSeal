using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public List<GameObject> gameObjectsGood;
    public List<GameObject> gameObjectsBad;

    public List<GameObject> randomObjectList;

    void Start()
    {
        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        GameObject randomObject = new GameObject();

        while (true)
        {
            for (int i = 0; i != randomObjectList.Count; i++)
            {
                if (randomObjectList != null)
                {
                    Destroy(randomObjectList[i].gameObject);
                }
            }

            for (int i = 0; i <= 9; i++)
            {
                var randomGoodOrBadObject = Random.Range(0, 3);

                if (randomGoodOrBadObject == 0 || randomGoodOrBadObject == 1)
                {
                    var objectIndex = Random.Range(0, gameObjectsGood.Count);
                    Vector3 randomPos = new Vector3(Random.Range(-10, 10), -6, Random.Range(-10, 10));

                    randomObject = Instantiate(gameObjectsGood[objectIndex], randomPos, Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));

                    randomObjectList.Add(randomObject);

                }
                else if (randomGoodOrBadObject == 2)
                {

                    var objectIndex = Random.Range(0, gameObjectsBad.Count);
                    Vector3 randomPos = new Vector3(Random.Range(-10, 10), -6, Random.Range(-10, 10));

                    randomObject = Instantiate(gameObjectsBad[objectIndex], randomPos, Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));

                    randomObjectList.Add(randomObject);
                }
            }

            yield return new WaitForSeconds(10f);
        }
    }

}
