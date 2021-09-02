using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public enum MediaType { Audio, Video, Image }

[System.Serializable]
public class MediaInfo
{
    public int resource_id;
    public string resource_url;
    public MediaType resource_type;
}
