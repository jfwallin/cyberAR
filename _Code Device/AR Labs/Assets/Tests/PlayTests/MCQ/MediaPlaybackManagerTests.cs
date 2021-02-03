/*using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.TestTools;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Tests
{
    public class MediaPlaybackManagerTests
    {
        #region Vairables
        //Reference to class being tested
        MediaPlaybackManager mpManager = null;

        //References to component dependencies
        VideoPlayer videoPlayer = null;
        AudioSource audioSource = null;
        MeshRenderer meshRenderer = null;

        //References to UI dependencies
        Button dummyButton = null;
        Slider dummySlider = null;

        //Private field names, used to reflect values
        const string R_TXTR_NAME = "videoRenderTexture";
        #endregion //Variables

        #region Test Suite
        //Called before each unit test is run to ensure a clean run
        [SetUp]
        public void SetUp()
        {
            //Setup the class to test
            mpManager = new GameObject().AddComponent<MediaPlaybackManager>();

            //Setup the dependencies
            videoPlayer = mpManager.gameObject.AddComponent<VideoPlayer>();
            audioSource = mpManager.gameObject.AddComponent<AudioSource>();
            meshRenderer = mpManager.gameObject.AddComponent<MeshRenderer>();

            //Setup UI dependencies
            dummyButton = new GameObject().AddComponent<Button>();
            dummyButton.transform.SetParent(mpManager.transform);
            dummySlider = new GameObject().AddComponent<Slider>();
            dummySlider.transform.SetParent(mpManager.transform);
        }

        //Called after each unit test is run to clean up residual state
        [TearDown]
        public void TearDown()
        {
            //Reset UI dependencies
            GameObject.Destroy(dummyButton.gameObject);
            GameObject.Destroy(dummySlider.gameObject);

            //Reset the class being tested and dependencies
            GameObject.Destroy(mpManager.gameObject);

            //Reset logging
            if (LogAssert.ignoreFailingMessages)
                LogAssert.ignoreFailingMessages = false;
        }

        [Test]
        public void Initialize_FindsRef_RefNotSetInInspector(
            [NUnit.Framework.Values("videoPlayer", "audioSource", "meshRenderer", "buttons", "scrubSlider")] string refName)
        {
            //Arrange
            FieldInfo refCheckFieldInfo = mpManager.GetType().GetField(refName, BindingFlags.NonPublic | BindingFlags.Instance);
            LogAssert.ignoreFailingMessages = true;
            //Action
            mpManager.Initialize();
            //Assert
            switch(refName)
            {
                case "videoPlayer":
                    Assert.AreEqual(videoPlayer, refCheckFieldInfo.GetValue(mpManager));
                    break;
                case "audioSource":
                    Assert.AreEqual(audioSource, refCheckFieldInfo.GetValue(mpManager));
                    break;
                case "meshRenderer":
                    Assert.AreEqual(meshRenderer, refCheckFieldInfo.GetValue(mpManager));
                    break;
                case "buttons":
                    List<Button> buttonList = (List<Button>)refCheckFieldInfo.GetValue(mpManager);
                    Assert.AreEqual(dummyButton, buttonList[0]);
                    break;
                case "scrubSlider":
                    Assert.AreEqual(dummySlider, refCheckFieldInfo.GetValue(mpManager));
                    break;
            }
        }

        [Test]
        public void Initialize_LinksVideoPlayerAndAudioSource_WhenNotLinked()
        {
            //Arrange (All done in Setup)
            //Action
            mpManager.Initialize();
            //Assert
            Assert.AreEqual(audioSource, videoPlayer.GetTargetAudioSource(0));
        }

        [Test]
        public void Initialize_LinksVideoPlayerToRenderTexture_WhenNotLinked()
        {
            //Arrange
            RenderTexture dummyRTexture = new RenderTexture(1, 1, 0);
            mpManager.GetType().GetField(R_TXTR_NAME, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mpManager, dummyRTexture);
            //Action
            mpManager.Initialize();
            //Assert
            Assert.AreEqual(dummyRTexture, videoPlayer.targetTexture);
        }

        [Test]
        public void DisplayMedia_EnablesOrDisablesProperComopnents_DependingOnMediaType(
            [NUnit.Framework.Values(MediaPlaybackManager.MediaType.Video,
                                    MediaPlaybackManager.MediaType.Audio,
                                    MediaPlaybackManager.MediaType.Image,
                                    MediaPlaybackManager.MediaType.ImageAndAudio,
                                    MediaPlaybackManager.MediaType.None)] MediaPlaybackManager.MediaType mediaType,
            [NUnit.Framework.Values(true, false)] bool componentsStartEnabled)
        {
            //Arrange
            mpManager.Initialize();
            videoPlayer.enabled = componentsStartEnabled;
            audioSource.enabled = componentsStartEnabled;
            meshRenderer.enabled = componentsStartEnabled;
            VideoClip vClip = (VideoClip) new Object();
            AudioClip aClip = AudioClip.Create("dummyClip", 1, 1, 5000, false);
            Texture2D image = Texture2D.whiteTexture;
            //Action
            switch(mediaType)
            {
                case MediaPlaybackManager.MediaType.Video:
                    mpManager.DisplayMedia(mediaType, vClip);
                    break;
                case MediaPlaybackManager.MediaType.Audio:
                    mpManager.DisplayMedia(mediaType, aClip);
                    break;
                case MediaPlaybackManager.MediaType.Image:
                    mpManager.DisplayMedia(mediaType, image);
                    break;
                case MediaPlaybackManager.MediaType.ImageAndAudio:
                    mpManager.DisplayMedia(mediaType, image, aClip);
                    break;
                case MediaPlaybackManager.MediaType.None:
                    mpManager.DisplayMedia(mediaType);
                    break;
            }
            //Assert
            switch (mediaType)
            {
                case MediaPlaybackManager.MediaType.Video:
                    Assert.IsTrue(videoPlayer.enabled && audioSource.enabled && meshRenderer.enabled);
                    break;
                case MediaPlaybackManager.MediaType.Audio:
                    Assert.IsTrue(!videoPlayer.enabled && audioSource.enabled && !meshRenderer.enabled);
                    break;
                case MediaPlaybackManager.MediaType.Image:
                    Assert.IsTrue(!videoPlayer.enabled && !audioSource.enabled && meshRenderer.enabled);
                    break;
                case MediaPlaybackManager.MediaType.ImageAndAudio:
                    Assert.IsTrue(!videoPlayer.enabled && audioSource.enabled && meshRenderer.enabled);
                    break;
                case MediaPlaybackManager.MediaType.None:
                    Assert.IsTrue(!videoPlayer.enabled && !audioSource.enabled && !meshRenderer.enabled);
                    break;
            }
        }

        [Test]
        public void DisplayMedia_EnablesAndDisablesButtonsAndSlider_BasedOnMediaType(
            [NUnit.Framework.Values(MediaPlaybackManager.MediaType.Video,
                                    MediaPlaybackManager.MediaType.Audio,
                                    MediaPlaybackManager.MediaType.Image,
                                    MediaPlaybackManager.MediaType.ImageAndAudio,
                                    MediaPlaybackManager.MediaType.None)] MediaPlaybackManager.MediaType mediaType,
            [NUnit.Framework.Values(true, false)] bool uiStartsEnabled)
        {
            //Arrange
            mpManager.Initialize();
            dummyButton.gameObject.SetActive(uiStartsEnabled);
            dummySlider.gameObject.SetActive(uiStartsEnabled);
            VideoClip vClip = (VideoClip)new Object();
            AudioClip aClip = AudioClip.Create("dummyClip", 1, 1, 5000, false);
            Texture2D image = Texture2D.whiteTexture;
            //Action
            switch (mediaType)
            {
                case MediaPlaybackManager.MediaType.Video:
                    mpManager.DisplayMedia(mediaType, vClip);
                    break;
                case MediaPlaybackManager.MediaType.Audio:
                    mpManager.DisplayMedia(mediaType, aClip);
                    break;
                case MediaPlaybackManager.MediaType.Image:
                    mpManager.DisplayMedia(mediaType, image);
                    break;
                case MediaPlaybackManager.MediaType.ImageAndAudio:
                    mpManager.DisplayMedia(mediaType, image, aClip);
                    break;
                case MediaPlaybackManager.MediaType.None:
                    mpManager.DisplayMedia(mediaType);
                    break;
            }
            //Assert
            switch (mediaType)
            {
                case MediaPlaybackManager.MediaType.Video:
                case MediaPlaybackManager.MediaType.Audio:
                case MediaPlaybackManager.MediaType.ImageAndAudio:
                    Assert.IsTrue(dummyButton.gameObject.activeSelf && dummySlider.gameObject.activeSelf);
                    break;
                case MediaPlaybackManager.MediaType.Image:
                case MediaPlaybackManager.MediaType.None:
                    Assert.IsTrue(!dummyButton.gameObject.activeSelf && dummySlider.gameObject.activeSelf);
                    break;
            }
        }

        [Test]
        public void DisplayMedia_StartsPlayback_StartImmediateEqualsTrue(
                        [NUnit.Framework.Values(MediaPlaybackManager.MediaType.Video,
                                    MediaPlaybackManager.MediaType.Image,
                                    MediaPlaybackManager.MediaType.ImageAndAudio)] MediaPlaybackManager.MediaType mediaType,
            [NUnit.Framework.Values(true, false)] bool startImmediate)
        {
            //Arrange
            mpManager.Initialize();
            VideoClip vClip = (VideoClip)new Object();
            AudioClip aClip = AudioClip.Create("dummyClip", 1, 1, 5000, false);
            Texture2D image = Texture2D.whiteTexture;
            //Action
            switch (mediaType)
            {
                case MediaPlaybackManager.MediaType.Video:
                    mpManager.DisplayMedia(mediaType, vClip, startImmediate);
                    break;
                case MediaPlaybackManager.MediaType.Audio:
                    mpManager.DisplayMedia(mediaType, aClip, startImmediate);
                    break;
                case MediaPlaybackManager.MediaType.ImageAndAudio:
                    mpManager.DisplayMedia(mediaType, image, aClip, startImmediate);
                    break;
            }
            //Assert
            switch (mediaType)
            {
                case MediaPlaybackManager.MediaType.Video:
                    Assert.IsTrue(videoPlayer.isPlaying);
                    break;
                case MediaPlaybackManager.MediaType.Audio:
                case MediaPlaybackManager.MediaType.ImageAndAudio:
                    Assert.IsTrue(audioSource.isPlaying);
                    break;
            }
        }

        [Test]
        public void OnPlayClicked_StartsVideo_WhenCalled()
        {
            //Arrange
            mpManager.Initialize();
            mpManager.DisplayMedia(MediaPlaybackManager.MediaType.Video, (VideoClip)new Object());
            //Action
            mpManager.OnPlayClicked();
            //Assert
            Assert.IsTrue(videoPlayer.isPlaying);
        }
        #endregion //Test Suite
    }
}*/