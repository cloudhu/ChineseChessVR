// --------------------------------------------------------------------------------------------------------------------
// <copyright file=NewBehaviourScript.cs company=League of HTC Vive Developers>
/*
11111111111111111111111111111111111111001111111111111111111111111
11111111111111111111111111111111111100011111111111111111111111111
11111111111111111111111111111111100001111111111111111111111111111
11111111111111111111111111111110000111111111111111111111111111111
11111111111111111111111111111000000111111111111111111111111111111
11111111111111111111111111100000011110001100000000000000011111111
11111111111111111100000000000000000000000000000000011111111111111
11111111111111110111000000000000000000000000000011111111111111111
11111111111111111111111000000000000000000000000000000000111111111
11111111111111111110000000000000000000000000000000111111111111111
11111111111111111100011100000000000000000000000000000111111111111
11111111111111100000110000000000011000000000000000000011111111111
11111111111111000000000000000100111100000000000001100000111111111
11111111110000000000000000001110111110000000000000111000011111111
11111111000000000000000000011111111100000000000000011110001111111
11111110000000011111111111111111111100000000000000001111100111111
11111111000001111111111111111111110000000000000000001111111111111
11111111110111111111111111111100000000000000000000000111111111111
11111111111111110000000000000000000000000000000000000111111111111
11111111111111111100000000000000000000000000001100000111111111111
11111111111111000000000000000000000000000000111100000111111111111
11111111111000000000000000000000000000000001111110000111111111111
11111111100000000000000000000000000000001111111110000111111111111
11111110000000000000000000000000000000111111111110000111111111111
11111100000000000000000001110000001111111111111110001111111111111
11111000000000000000011111111111111111111111111110011111111111111
11110000000000000001111111111111111100111111111111111111111111111
11100000000000000011111111111111111111100001111111111111111111111
11100000000001000111111111111111111111111000001111111111111111111
11000000000001100111111111111111111111111110000000111111111111111
11000000000000111011111111111100011111000011100000001111111111111
11000000000000011111111111111111000111110000000000000011111111111
11000000000000000011111111111111000000000000000000000000111111111
11001000000000000000001111111110000000000000000000000000001111111
11100110000000000001111111110000000000000000111000000000000111111
11110110000000000000000000000000000000000111111111110000000011111
11111110000000000000000000000000000000001111111111111100000001111
11111110000010000000000000000001100000000111011111111110000001111
11111111000111110000000000000111110000000000111111111110110000111
11111110001111111100010000000001111100000111111111111111110000111
11111110001111111111111110000000111111100000000111111111111000111
11111111001111111111111111111000000111111111111111111111111100011
11111111101111111111111111111110000111111111111111111111111001111
11111111111111111111111111111110001111111111111111111111100111111
11111111111111111111111111111111001111111111111111111111001111111
11111111111111111111111111111111100111111111111111111111111111111
11111111111111111111111111111111110111111111111111111111111111111
*/
//   
// </copyright>
// <summary>
//  Chinese Chess VR
// </summary>
// <author>胡良云（CloudHu）</author>
//中文注释：胡良云（CloudHu） 3/26/2017

// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// FileName: WarUI.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 负责战斗的UI操作
/// DateTime: 3/26/2017
/// </summary>
public class WarUI : MonoBehaviour {

    #region Public Variables  //公共变量区域

    [Tooltip("显示在棋子英雄头顶的文本.")]
    public string displayText;
    [Tooltip("显示文本的字体大小.")]
    public int fontSize = 14;
    [Tooltip("字体颜色.")]
    public Color fontColor = Color.black;

    [Tooltip("整个外框的大小.")]
    public Vector2 containerSize = new Vector2(0.1f, 0.03f);
    [Tooltip("字体框背景色.")]
    public Color containerColor = Color.black;

    [Tooltip("和目标之间的偏移值")]
    public Vector3 ScreenOffset = new Vector3(0f, 5f, 0f);

    #endregion


    #region Private Variables   //私有变量区域

    ChessmanController _target; //目标棋子

    float _characterControllerHeight = 0f;  //高度

    Transform _targetTransform;

    //Renderer _targetRenderer;

    Vector3 _targetPosition;

    #endregion


    #region MonoBehaviour CallBacks //回调函数区域
    // Use this for initialization
    void Start()
    {
        ResetUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (_target == null)
        {
            Destroy(this.gameObject);
            return;
        }

        if(NetworkTurn.Instance._selectedId== int.Parse(transform.parent.name))
        {
            UpdateText("Selected");
        }


    }

    #endregion

    #region Public Methods	//公共方法区域

    public void TrySelectChessman()
    {
        string tmpText = transform.FindChild("Canvas/UITextFront").GetComponent<Text>().text;
        if (transform.parent.GetComponent<ChessmanController>())    //如果是棋子
        {
            switch (tmpText)
            {
                case "Select":
                    NetworkTurn.Instance.OnSelectChessman(int.Parse(transform.parent.name), transform.parent.localPosition.x, transform.parent.localPosition.z);
                    break;
                case "Selected":
                    NetworkTurn.Instance.OnCancelSelected(int.Parse(transform.parent.name));
                    UpdateText("Select");
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (tmpText)
            {
                case "Select":
                    NetworkTurn.Instance.OnSelectChessman(-1, transform.parent.localPosition.x, transform.parent.localPosition.z);
					transform.parent.GetComponent<BoardPoint>().Occupied();
                    break;
				case "Selected":
					NetworkTurn.Instance.OnCancelSelected (-1);
					transform.parent.GetComponent<BoardPoint> ().isOccupied = false;
                    UpdateText("Select");
                    break;
                default:
                    break;
            }
        }



    }

    public void ResetUI()
    {
        SetContainer();
        SetText("UITextFront");
        SetText("UITextReverse");


    }

    public void UpdateText(string newText)
    {
        displayText = newText;
        ResetUI();
    }

    /// <summary>
    /// 指派目标.
    /// </summary>
    /// <param name="target">Target.</param>
    public void SetTarget(ChessmanController target)
    {

        if (target == null)
        {
            Debug.LogError("<Color=Red><b>Missing</b></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
            return;
        }

        // Cache references for efficiency because we are going to reuse them.
        _target = target;
        _targetTransform = _target.GetComponent<Transform>();

    }


    #endregion




    #region Private Methods //私有方法

    private void SetContainer()
    {
        transform.FindChild("Canvas").GetComponent<RectTransform>().sizeDelta = containerSize;
        var tmpContainer = transform.FindChild("Canvas/UIContainer");
        tmpContainer.GetComponent<RectTransform>().sizeDelta = containerSize;
        tmpContainer.GetComponent<Image>().color = containerColor;
    }

    private void SetText(string name)
    {
        var tmpText = transform.FindChild("Canvas/" + name).GetComponent<Text>();
        tmpText.material = Resources.Load("UIText") as Material;
        tmpText.text = displayText.Replace("\\n", "\n");
        tmpText.color = fontColor;
        tmpText.fontSize = fontSize;
    }


    #endregion

}
