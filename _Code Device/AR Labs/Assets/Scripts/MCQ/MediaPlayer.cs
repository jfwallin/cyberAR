using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MediaPlayer : MonoBehaviour
{
    public void PlayMedia(string mediaName, System.Action callback)
    {
        Debug.Log($"Play Media called with media name: {mediaName}\nExecuting callback\n\n\n");
        callback.Invoke();
    }
}
