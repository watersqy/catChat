using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class menu : MonoBehaviour
{
    static public menu Instance;
    //按钮菜单控制
    public GameObject panelMenu;
    public GameObject btn;

    //聊天面板显示控制
    public GameObject panelChat;
    bool isOpen;

    //音量面板显示控制
    bool voice;
    public GameObject panelVoice;

    //猫咪切换
    public GameObject whiteCat;
    public GameObject blackCat;
    bool cat;

    //设置面板控制
    public GameObject panelSet;

    //消息回复音量滑动条
    public Slider volumeSlider;

    //音乐音量滑动条
    public Slider songVolume;

    //唱歌
    public AudioSource songPlayer;
    AudioClip[] songs;
    int index;

    void Start()
    {
        Instance = this;
        isOpen = true;
        voice = true;
        cat = false;
        volumeSlider.value = 50;
        songVolume.value = 1;
        index = 0;
        songs = Resources.LoadAll<AudioClip>("songs/");
    }

    public void openMenu()
    {
        panelMenu.SetActive(true);
        btn.SetActive(false);
    }

    public void closeMenu()
    {
        panelMenu.SetActive(false);
        btn.SetActive(true);
    }

    
    public void openChat()
    {
        panelChat.SetActive(isOpen);
        isOpen = !isOpen;
    }

    public void openVoice()
    {
        panelVoice.SetActive(voice);
        voice = !voice;
    }

    public void ChangeCat()
    {
        whiteCat.SetActive(cat);
        cat = !cat;
        blackCat.SetActive(cat);
        AnimControll.Instance.GetAnim();
    }

    public void openSettings()
    {
        panelSet.SetActive(true);

    }

    public void closeSettings()
    {
        panelSet.SetActive(false);
    }

    public void SingSongs()
    {
        if (songPlayer.isPlaying)
        {
            AnimControll.Instance.anim.SetBool("isSpeak", false);
            songPlayer.Stop();
            index = (index + 1) % songs.Length;
        }
        else
        {
            songPlayer.clip = songs[index];
            AnimControll.Instance.anim.SetBool("isSpeak", true);
            songPlayer.Play();
        }
    }

    
    public void exitGame()
    {
#if UNITY_EDITOR  //在编辑模式退出
        UnityEditor.EditorApplication.isPlaying = false;
#else  //发布后退出
Application.Quit();
#endif
    }
}
