using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class menu : MonoBehaviour
{
    static public menu Instance;
    //��ť�˵�����
    public GameObject panelMenu;
    public GameObject btn;

    //���������ʾ����
    public GameObject panelChat;
    bool isOpen;

    //���������ʾ����
    bool voice;
    public GameObject panelVoice;

    //è���л�
    public GameObject whiteCat;
    public GameObject blackCat;
    bool cat;

    //����������
    public GameObject panelSet;

    //��Ϣ�ظ�����������
    public Slider volumeSlider;

    //��������������
    public Slider songVolume;

    //����
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
#if UNITY_EDITOR  //�ڱ༭ģʽ�˳�
        UnityEditor.EditorApplication.isPlaying = false;
#else  //�������˳�
Application.Quit();
#endif
    }
}
