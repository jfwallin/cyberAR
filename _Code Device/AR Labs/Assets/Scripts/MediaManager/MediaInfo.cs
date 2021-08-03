using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public enum MediaType { Audio, Video, Image }

[System.Serializable]
public class MediaInfo
{
    public int labID;
    public int resourceId;
    public MediaType mediaType;
}
