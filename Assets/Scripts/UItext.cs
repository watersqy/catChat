using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// �ı������
//��Ϣ���

public class UItext : MonoBehaviour
{
    //ȫ��Ψһ����
    public static UItext Instance;
    // �ı������
    public InputField inputField;
    //������ �жϹ������Ƿ�����ײ�
    public Scrollbar scrollbar;

    //������Ϣʱ���������Զ����¹���
    public ScrollRect scrollRect;

    //�������е���Ϣ��¼
    public GameObject contentNode;

    //�ı�
    public Text defaultText;

    void Start()
    {
        Instance = this;
    }

    private void Update()
    {
        //ÿ�θ���ʱ
        if(contentNode != null && contentNode.transform.childCount > 10)
        {
            //�ж���Ϣ�Ƿ����10��
            Destroy(contentNode.transform.GetChild(1).gameObject);
            //0����ɾ�����ı���ģ��
        }
    }

    //�����µ��ı����ı�������ύʱ��������������ύ��ֵ��value
    public void CreateQuestion(string value)
    {
        //��¡�ı����
        Text clonedText = Instantiate(defaultText, contentNode.transform);
        //���ø��ı����������
        clonedText.text = "�û���\n"+value;
        //�����ı���ʾ
        clonedText.gameObject.SetActive(true);
        //�����Ƿ�������ײ�
        if (scrollbar.value < 0.01f)
        {
            //�������ڵײ��������Ϣʱ�������Զ�������������������ڵײ���˵���û����ڲ鿴�����¼�������������Զ�����
            Canvas.ForceUpdateCanvases();
            scrollRect.content.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputHorizontal();
            scrollRect.content.GetComponent<ContentSizeFitter>().SetLayoutVertical();
            scrollRect.verticalNormalizedPosition = 0;
        }
        inputField.text = "";
    }

    public void CreateAnswer(string value)
    {
        //��¡�ı����
        Text clonedText = Instantiate(defaultText, contentNode.transform);
        //���ø��ı����������
        clonedText.text += "è��\n" + value;
        //�����ı���ʾ
        clonedText.gameObject.SetActive(true);
        //�����Ƿ�������ײ�
        if (scrollbar.value < 0.01f)
        {
            //�������ڵײ��������Ϣʱ�������Զ�������������������ڵײ���˵���û����ڲ鿴�����¼�������������Զ�����
            Canvas.ForceUpdateCanvases();
            scrollRect.content.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputHorizontal();
            scrollRect.content.GetComponent<ContentSizeFitter>().SetLayoutVertical();
            scrollRect.verticalNormalizedPosition = 0;
        }
    }
}
