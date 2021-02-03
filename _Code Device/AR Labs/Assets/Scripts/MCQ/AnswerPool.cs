using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class AnswerPool
{
    [SerializeField]
    private string name;
    [SerializeField]
    private List<string> pool;

    public string Name { get => name; }

    public AnswerPool(string[] initialPool)
    {
        pool = new List<string>();
        pool.AddRange(initialPool);
    }

    public void AddToPool(string itemToAdd) => pool.Add(itemToAdd);
    public void AddToPool(string[] itemsToAdd) => pool.AddRange(itemsToAdd);

    public void RemoveFromPool(string itemToRemove) => pool.Remove(itemToRemove);
    public void RemoveFromPool(string[] itemsToRemove) => pool.RemoveAll(x => itemsToRemove.Contains(x));

    public string[] GetAllAnswers() => pool.ToArray();

    public string GetRandomAnswer => pool[Random.Range(0, pool.Count)];

    public string[] GetRandomAnswers(int howMany)
    {
        if(howMany > 0)
        {
            List<string> returnList = new List<string>();
            List<string> tmpPool = new List<string>();
            tmpPool.AddRange(pool);
            for (int i = howMany; i > 0; i--)
            {
                int index = Random.Range(0, tmpPool.Count);
                returnList.Add(tmpPool[index]);
                tmpPool.RemoveAt(index);
            }
            return returnList.ToArray();
        }
        else
        {
            return new string[] { "" };
        }
    }

    public string GetRandomAnswerExcluding(string excludedAnswer)
    {
        string returnString;
        do
        {
            returnString = pool[Random.Range(0, pool.Count)];
        } while (returnString == excludedAnswer);

        return returnString;
    }

    public string[] GetRandomAnswersExcluding(int howMany, string excludedAnswer)
    {
        if (howMany > 0)
        {
            List<string> returnList = new List<string>();
            List<string> tmpPool = new List<string>();
            tmpPool.AddRange(pool);
            tmpPool.Remove(excludedAnswer);
            for (int i = howMany; i > 0; i--)
            {
                int index = Random.Range(0, tmpPool.Count);
                returnList.Add(tmpPool[index]);
                tmpPool.RemoveAt(index);
            }
            return returnList.ToArray();
        }
        else
        {
            return new string[] { "" };
        }
    }
}
