using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LabDataObject
{
    public string Lab_ID;
    public string Author;
    public string CourseName;
    public string EstimatedLength;
    public string NumModules;
    public string[] Objectives;
    public string[] ActivityModules;
    public MediaInfo[] Assets;
    public bool Transmission;
}