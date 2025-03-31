using System.Collections;
using System.Collections.Generic;
using Live2D.Cubism.Framework;
using Live2D.Cubism.Framework.Raycasting;
using UnityEngine;


// 触摸部位
public static class TouchPart
{
    // 左手
    public const string LeftHand = "ArtMesh27";
    // 右手
    public const string RightHand = "ArtMesh29";

    // 头部
    public const string Head = "ArtMesh17";
}


public class AnimControll : MonoBehaviour
{
    static public AnimControll Instance;

    public GameObject blackCat;
    public GameObject whiteCat;
    
    public Animator anim;
    AudioSource audioSource;

    int count;
    CubismRaycaster cubismRaycaster;
    CubismRaycastHit[] hits = new CubismRaycastHit[1];

    void Start()
    {
        Instance = this;
        anim = whiteCat.GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        cubismRaycaster = whiteCat.GetComponent<CubismRaycaster>();
        count = 0;
    }

    
    void Update()
    {
        // 检测鼠标左键按下
        if (Input.GetMouseButtonDown(0))
        {
            count = cubismRaycaster.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), hits); // 执行射线检测
            if (count == 0) return;
            if (hits[0].Drawable.Id == TouchPart.Head)
            {
                audioSource.clip = Resources.Load<AudioClip>("hulu");
                audioSource.Play();
                anim.SetBool("head", true);
            }
                
        }

        if (Input.GetMouseButtonUp(0))
        {
            // 如果没有检测到命中，直接返回
            if (count == 0) return;
            SetAnim(hits[0].Drawable.Id); // 设置 Live2D 动作
        }
    }

    public void GetAnim()
    {
        if (whiteCat.activeInHierarchy)
        {
            anim = whiteCat.GetComponent<Animator>();
            cubismRaycaster = whiteCat.GetComponent<CubismRaycaster>();
        }
        else
        {
            anim = blackCat.GetComponent<Animator>();
            cubismRaycaster = blackCat.GetComponent<CubismRaycaster>();
        }
    }

    public void SpeechAnim(float len)
    {
        anim.SetBool("isSpeak", true);
        Invoke("EndSpeechAnim", len);
    }

    void EndSpeechAnim()
    {
        anim.SetBool("isSpeak", false);
    }

    void SetAnim(string hitPart)
    {
        Debug.Log(hitPart);
        switch (hitPart)
        {
            case TouchPart.Head:
                anim.SetBool("head", false);
                audioSource.Stop();
                audioSource.clip = null;
                break;
            case TouchPart.LeftHand:
                anim.SetTrigger("hand");
                audioSource.clip = Resources.Load<AudioClip>("miao");
                audioSource.Play();
                break;
            case TouchPart.RightHand:
                anim.SetTrigger("hand");
                audioSource.clip = Resources.Load<AudioClip>("miao");
                audioSource.Play();
                break;
            default:
                break;
        }
    }
}
