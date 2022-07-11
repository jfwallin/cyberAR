using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public enum MediaType { Audio, Video, Image, Zip }

[System.Serializable]
public class MediaInfo
{
    public string resource_name;
    public MediaType resource_type;
}
