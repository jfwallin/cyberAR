using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEditor;

[CreateAssetMenu(fileName = "MediaManager", menuName = "ScriptableObjects/MediaManager", order = 1)]
public class MediaManager : ScriptableObject
{
    public List<AudioClip> audioClips = new List<AudioClip>();
    public List<VideoClip> videoClips = new List<VideoClip>();
    public List<Texture2D> images = new List<Texture2D>();

    public AudioClip GetAudioClip(string clipName)
    {
        return audioClips.Find(x => x.name == clipName);
    }

    public VideoClip GetVideoClip(string clipName)
    {
        return videoClips.Find(x => x.name == clipName);
    }

    public Texture2D GetImage(string imageName)
    {
        return images.Find(x => x.name == imageName);
    }
}
