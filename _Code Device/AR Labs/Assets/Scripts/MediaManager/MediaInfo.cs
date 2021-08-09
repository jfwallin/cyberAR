using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum MediaType { Audio, Video, Image }

[System.Serializable]
public class MediaInfo
{
    public string id;
    public MediaType mediaType;
}
