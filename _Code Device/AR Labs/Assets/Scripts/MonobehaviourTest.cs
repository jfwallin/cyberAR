using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// An arbitrary class constructed only to demonstrate testing a monobehaviour
/// </summary>
public class MonobehaviourTest : MonoBehaviour
{
    #region Variables
    private List<int> list;
    private bool decay;               //Flag to track whether the list is shortening over time
    private float timeSinceLastDecay; //Used to track when to decay the list
    #endregion //Variables

    #region Unity Methods
    void Awake()
    {
        list = new List<int>();
        decay = false;
        timeSinceLastDecay = 0f;
    }

    private void Update()
    {
        //shorten the list if the flag is set and enough time has elapsed
        if(decay && list.Count > 0)
        {
            if(timeSinceLastDecay > 1f)
            {
                RemoveItemFromList();
                timeSinceLastDecay = 0f;
            }
            else
            {
                timeSinceLastDecay += Time.deltaTime;
            }
        }
    }
    #endregion //Unity Methods

    #region Public Methods
    public int GetListLength()
    {
        return list.Count;
    }

    public int GetLastItem()
    {
        return list[list.Count - 1];
    }

    public void AddItemToList(int item)
    {
        list.Add(item);
    }

    public void RemoveItemFromList()
    {
        list.RemoveAt(list.Count - 1);
    }

    public void MakeListDecay()
    {
        decay = true;
    }

    public void StopListDecay()
    {
        decay = false;
    }
    #endregion //Public Methods
}
