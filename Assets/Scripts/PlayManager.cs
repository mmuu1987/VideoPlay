using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using  System.IO;

using UnityEngine.UI;

public class PlayManager : MonoBehaviour
{

    public VideoPlayer VideoPlayer;

    public AudioSource AudioSource;

    private List<string> _videoList = new List<string>();

    public List<AudioClip> _audioClips = new List<AudioClip>();

    private int _curVideoIndex = 0;

    private int _curAudioIndex = 0;

    public KeyInput KeyInput;

    public Button LoopButton;

    public Text LoopText;

    public UDP Udp;

    /// <summary>
    /// 是主要主机还是接收命令的主机
    /// </summary>
    public bool IsMain = true;

    /// <summary>
    /// 待机视频路径
    /// </summary>
    private string _standByPath;

    private bool _isVideoPlay = true;

    private bool _isLoop = false;

    // Start is called before the first frame update
    void Start()
    {
        //加载视频路径资源
        string videoPath = Application.streamingAssetsPath + "/Videos";

        DirectoryInfo dirInfo = new DirectoryInfo(videoPath);

        foreach (DirectoryInfo info in dirInfo.GetDirectories())
        {
            if (info.GetFiles().Length > 0)
            {
                string path = info.FullName + "/" + info.GetFiles()[0].Name;
                _videoList.Add(path);
                Debug.Log("video path is " + path);
            }
        }

        Udp.ComAction = ComAction;

        if (IsMain)
        {
            LoopButton.onClick.AddListener((() =>
            {
                string str = PlayerPrefs.GetString("loop");


                if (str == "true")
                {
                    str = "false";
                }
                else
                {
                    str = "true";
                }

                PlayerPrefs.SetString("loop", str);

                SetLoop();
            }));
            SetLoop();
            Udp.port = 9001;
        }
        else
        {
            LoopButton.gameObject.SetActive(false);
            Udp.port = 9003;
        }
       
        //待机视频
        string standByPath = Application.streamingAssetsPath + "/StandBy";
        DirectoryInfo dirInfoStandBy = new DirectoryInfo(standByPath);
        foreach (FileInfo info in dirInfoStandBy.GetFiles())
        {

            string path = info.FullName;
            Debug.Log("待机视频为：" + path);
            _standByPath = path;
            break;
        }

        //待机视频一直循环播放
        if (_standByPath != null)
        {
            VideoPlayer.url = _standByPath;
            VideoPlayer.Play();
            VideoPlayer.isLooping = true;
        }
        Udp.StartMessage();
    }

    private void SetLoop()
    {
        string str = PlayerPrefs.GetString("loop");

        if (string.IsNullOrEmpty(str))//首次播放，默认循环
        {
            str = "true";
            PlayerPrefs.SetString("loop", str);
        }

        if (str == "true")
        {
            VideoPlayer.isLooping = true;
            _isLoop = true;
            Udp.loopCom = "loopTrue";
        }
        else if (str == "false")
        {
            VideoPlayer.isLooping = false;
            _isLoop = false;
            Udp.loopCom = "loopFalse";
        }

        LoopText.text = "视频循环模式为：" + str;
    }

    private IEnumerator LoadMusic(string filepath, string savepath)
    {
        var stream = File.Open(filepath, FileMode.Open);
      

        var www = new WWW("file://" + savepath);
        yield return www;
        var clip = www.GetAudioClip();
        Debug.Log(filepath);
        _audioClips.Add(clip);
    }


    private void SetWindows( bool isVideoPlay)
    {
        _isVideoPlay = isVideoPlay;

        //隐藏UI
        LoopButton.gameObject.SetActive(false);

        if (_isVideoPlay)
        {
            //KeyInput.SetWindow(true);
        }
        else
        {
            //KeyInput.SetWindow(false);
        }
    }
    private void PlayVideo()
    {
        AudioSource.Stop();

        if (_curVideoIndex > _videoList.Count) _curVideoIndex = 0;
        if (_curVideoIndex <= 0) _curVideoIndex = 0;
        VideoPlayer.frame = 0;
        VideoPlayer.url = _videoList[_curVideoIndex];
        _curVideoIndex++;

       
        VideoPlayer.Play();
        SetWindows(true);
    }

    private void PlayAudio()
    {
        VideoPlayer.Stop();
        if (_curAudioIndex > _audioClips.Count) _curAudioIndex = 0;
        if (_curAudioIndex < 0) _curAudioIndex = 0;

        AudioSource.clip = _audioClips[_curAudioIndex];
        _curAudioIndex++;
      
        AudioSource.Play();
        SetWindows(false);
    }

    private IEnumerator WaitTime(float time,Action action)
    {
        yield return new WaitForSeconds(0.15f);
        if (action != null) action();
    }
    private string _strTemp = null;

    private void ComAction(string str)
    {
        _strTemp = str;

    }

    // Update is called once per frame
    void Update()
    {
        if (_strTemp != null)
        {

            Debug.Log(_strTemp);

            switch (_strTemp)
            {
                case "P":
                    if (_isVideoPlay)
                    {
                        if (VideoPlayer.isPlaying)
                        {
                            VideoPlayer.Pause();
                        }
                        else
                        {
                            VideoPlayer.Play();
                        }
                    }

                    break;
                case "Z":
                    if (_isVideoPlay)
                    {
                        //VideoPlayer.url = _videoList[_curVideoIndex];
                        VideoPlayer.frame=0;
                        VideoPlayer.Play();
                    }
                    break;
                case "S":
                    SetWindows(false);
                    VideoPlayer.Stop();
                    AudioSource.Stop();
                    break;
                case "Up":
                   
                    VideoPlayer.GetTargetAudioSource(0).volume += 0.1f;
                    break;
                case "Down":
                   
                    VideoPlayer.GetTargetAudioSource(0).volume -= 0.1f;
                    break;
                case "BgSound":
                    AudioSource.mute = !AudioSource.mute;
                    break;
                case "BgSoundVol":
                    AudioSource.mute = false;
                    break;
                case "SongVolUp":
                    AudioSource.volume += 0.1f;
                    break;
                case "loopTrue":
                        _isLoop = true;
                    break;
                case "loopFalse":
                    _isLoop = false;
                    break;
                case "SongVolDown":
                    AudioSource.volume -= 0.1f;
                    break;
                default:
                    if (_strTemp.Contains("video"))//视频
                    {
                        try
                        {
                            int index = int.Parse(_strTemp.Substring(5, _strTemp.Length - 1 - 4))-1;
                            _curVideoIndex = index;
                            PlayVideo();
                            VideoPlayer.isLooping = _isLoop;
                        }
                        catch (Exception e)
                        {
                          
                           
                        }
                      
                        
                    }
                    else if (_strTemp.Contains("song"))//音频
                    {
                        try
                        {
                            int index = int.Parse(_strTemp.Substring(4, _strTemp.Length-1-3)) -1;

                            Debug.Log("audio index " + index);
                            _curAudioIndex = index;
                            PlayAudio();
                        }
                        catch (Exception e)
                        {


                        }
                    }
                    break;
            }



            _strTemp = null;
        }


        #region 视频事件

        if (VideoPlayer.isPlaying)
        {
            if (!VideoPlayer.isLooping)
            {
                //Debug.Log(VideoPlayer.frame +" <==> " + VideoPlayer.frameCount);
                if (VideoPlayer.frame <= 0) return;
                if ((ulong)VideoPlayer.frame >= VideoPlayer.frameCount-10)
                {
                   
                    if (_isVideoPlay)
                    {
                       // _strTemp = "S";

                    }
                }
            }
        }

        #endregion
    }

    //private void OnGUI()
    //{
    //    if (GUI.Button(new Rect(0f, 0f, 100f, 100f), "test"))
    //    {
    //        PlayAudio();
    //    }
    //}
}
