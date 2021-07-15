using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WriteAtEnd", menuName = "ScriptableObjects/WriteAtEnd", order = 1)]
public abstract class WriteAtEnd : ScriptableObject
{
    // Start is called before the first frame update


    public abstract void WriteToString(string ite1, string item2, string item3, string item4);
}
