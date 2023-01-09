using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public List<GameObject> gameObjectsGood;
    public List<GameObject> gameObjectsBad;

    public List<GameObject> randomObjectList;

    public List<Transform> spawnPoints;

    GameObject randomObject;

    void Start()
    {
        spawnPoints = new List<Transform>(spawnPoints);
    }

    public void Spawn()
    {
        if (randomObjectList != null)
        {
            for (int i = 0; i != randomObjectList.Count; i++)
            {
                Destroy(randomObjectList[i].gameObject);
            }

            randomObjectList.Clear();
        }


        for (int i = 0; i <= 9; i++)
        {
            var randomGoodOrBadObject = Random.Range(0, 3);

            if (randomGoodOrBadObject == 0 || randomGoodOrBadObject == 1)
            {
                var objectIndex = Random.Range(0, gameObjectsGood.Count);
                var spawn = Random.Range(0, spawnPoints.Count);

                randomObject = Instantiate(gameObjectsGood[objectIndex], spawnPoints[spawn].transform.position, Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));

                randomObjectList.Add(randomObject);

            }
            else if (randomGoodOrBadObject == 2)
            {

                var objectIndex = Random.Range(0, gameObjectsBad.Count);
                var spawn = Random.Range(0, spawnPoints.Count);


                randomObject = Instantiate(gameObjectsBad[objectIndex], spawnPoints[spawn].transform.position, Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));

                randomObjectList.Add(randomObject);
            }
        }
    }

}
