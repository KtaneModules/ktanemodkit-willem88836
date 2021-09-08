using System;
using UnityEngine;

namespace RMMondrian
{
    [Serializable]
    public class GameObjectCache
    {
        [SerializeField] private GameObject[] gameObjects;
        int pointer = 0;

        public GameObject GetNext()
        {
            GameObject obj = gameObjects[pointer];
            pointer++;
            return obj;
        }

        public void GiveBack(GameObject obj)
        {
            pointer--;
            gameObjects[pointer] = obj;
        }

        public void DisableUnused()
        {
            for (int i = pointer; i < gameObjects.Length; i++)
                gameObjects[i].SetActive(false);
        }
    }
}
