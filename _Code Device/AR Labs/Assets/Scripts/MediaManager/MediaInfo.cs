using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum MediaType { Audio, Video, Image }

public class MediaInfo
{
    public int labID;
    public int resourceId;
    public MediaType mediaType;
}
