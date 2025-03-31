using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 文本输入框
//消息面板

public class UItext : MonoBehaviour
{
    //全局唯一单例
    public static UItext Instance;
    // 文本输入框
    public InputField inputField;
    //滚动条 判断滚动条是否处于最底部
    public Scrollbar scrollbar;

    //发送消息时，滚动条自动向下滚动
    public ScrollRect scrollRect;

    //添加面板中的消息记录
    public GameObject contentNode;

    //文本
    public Text defaultText;

    void Start()
    {
        Instance = this;
    }

    private void Update()
    {
        //每次更新时
        if(contentNode != null && contentNode.transform.childCount > 10)
        {
            //判断消息是否大于10条
            Destroy(contentNode.transform.GetChild(1).gameObject);
            //0不能删，是文本的模板
        }
    }

    //创建新的文本。文本输入框提交时调用这个函数，提交的值是value
    public void CreateQuestion(string value)
    {
        //克隆文本组件
        Text clonedText = Instantiate(defaultText, contentNode.transform);
        //设置该文本组件的内容
        clonedText.text = "用户：\n"+value;
        //设置文本显示
        clonedText.gameObject.SetActive(true);
        //设置是否滚动到底部
        if (scrollbar.value < 0.01f)
        {
            //滚动条在底部，添加信息时滚动条自动滚动；如果滚动条不在底部，说明用户正在查看聊天记录，滚动条不会自动滚动
            Canvas.ForceUpdateCanvases();
            scrollRect.content.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputHorizontal();
            scrollRect.content.GetComponent<ContentSizeFitter>().SetLayoutVertical();
            scrollRect.verticalNormalizedPosition = 0;
        }
        inputField.text = "";
    }

    public void CreateAnswer(string value)
    {
        //克隆文本组件
        Text clonedText = Instantiate(defaultText, contentNode.transform);
        //设置该文本组件的内容
        clonedText.text += "猫：\n" + value;
        //设置文本显示
        clonedText.gameObject.SetActive(true);
        //设置是否滚动到底部
        if (scrollbar.value < 0.01f)
        {
            //滚动条在底部，添加信息时滚动条自动滚动；如果滚动条不在底部，说明用户正在查看聊天记录，滚动条不会自动滚动
            Canvas.ForceUpdateCanvases();
            scrollRect.content.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputHorizontal();
            scrollRect.content.GetComponent<ContentSizeFitter>().SetLayoutVertical();
            scrollRect.verticalNormalizedPosition = 0;
        }
    }
}
