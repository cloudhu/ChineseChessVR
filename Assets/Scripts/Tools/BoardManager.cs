// --------------------------------------------------------------------------------------------------------------------
// <copyright file=BoardManager.cs company=League of HTC Vive Developers>
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
/// <summary>
/// FileName: BoardManager.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 
/// DateTime: 3/26/2017
/// </summary>
public class BoardManager : MonoBehaviour {

    #region Public Variables  //公共变量区域
	[Tooltip("棋盘上的90个坐标点")]
    public static BoardPoint[] points=new BoardPoint[90]; 

	[Tooltip("展示后需要隐藏的坐标点")]
	public List<BoardPoint> hidePoints = new List<BoardPoint> ();
    #endregion


    #region Private Variables   //私有变量区域


    #endregion


    #region MonoBehaviour CallBacks //回调函数区域
    // Use this for initialization
    void Start () {
        points = transform.GetComponentsInChildren<BoardPoint>();

	}
	
    #endregion

    #region Public Methods	//公共方法区域

    /// <summary>
    /// 棋子离开该位置，取消该位置的占用
    /// </summary>
    public void leavePoint(float fromX,float fromZ)
    {
        for (int i = 0; i < 90; i++)
        {
            if (points[i].isOccupied)
            {
                if (points[i].transform.localPosition.x==fromX && points[i].transform.localPosition.z==fromZ)
                {
                    points[i].isOccupied = false;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 分别展示各个棋子可行的路线
    /// </summary>
    /// <param name="id">棋子id</param>
    public void showPossibleWay(int id)
    {
        switch (ChessmanManager.chessman[id]._type)
        {
            case ChessmanManager.Chessman.TYPE.KING:
                ShowKingWay(id);
                break;
            case ChessmanManager.Chessman.TYPE.GUARD:
                ShowGuardWay(id);
                break;
            case ChessmanManager.Chessman.TYPE.ELEPHANT:
                ShowElephantWay(id);
                break;
            case ChessmanManager.Chessman.TYPE.HORSE:
                ShowHorseWay(id);
                break;
            case ChessmanManager.Chessman.TYPE.ROOK:
                ShowRookWay(id);
                break;
            case ChessmanManager.Chessman.TYPE.CANNON:
                ShowCannonWay(id);
                break;
            case ChessmanManager.Chessman.TYPE.PAWN:
                ShowPawnWay(id);
                break;
        }
    }

	/// <summary>
	/// 在选择好目标后调用,将可能的位置隐藏.
	/// </summary>
	public void hidePossibleWay(){
	
		foreach (var item in hidePoints) {
			item.HidePointer ();
		}
		hidePoints.Clear ();
	}


    #endregion

    #region Private Methods  //辅助方法


    void ShowKingWay(int id)
    {
        for (int i = 12; i < 31; i++)
        {
            if (!points[i].isOccupied)
            {
                if (PositionManager.canMoveKing(id, points[i].transform.localPosition.x, points[i].transform.localPosition.z, -1))
                {
                    points[i].ShowBeams();
					hidePoints.Add (points[i]);
                }
            }
        }
    }

    void ShowGuardWay(int id)
    {
        for (int i = 12; i < 31; i++)
        {
            if (!points[i].isOccupied)
            {
                if (PositionManager.canMoveGuard(id, points[i].transform.localPosition.x, points[i].transform.localPosition.z, -1))
                {
                    points[i].ShowBeams();
					hidePoints.Add (points[i]);
                }

            }
        }
    }

    void ShowElephantWay(int id)
    {

        for (int i = 0; i < 14; i++)
        {
            if (!points[i].isOccupied)
            {
                if (PositionManager.canMoveElephant(id, points[i].transform.localPosition.x, points[i].transform.localPosition.z, -1))
                {
                    points[i].ShowBeams();
					hidePoints.Add (points[i]);
                }

            }
        }
    }



    void ShowHorseWay(int id)
    {
        for (int i = 0; i < 90; i++)
        {
            if (!points[i].isOccupied)
            {
                float _x = points[i].transform.localPosition.x;	//节点位置x		
                float fromX = ChessmanManager.chessman[id]._x;	//棋子位置x

                if (_x > (fromX-12f) && _x < (fromX+12f))
                {
                    float _z = points[i].transform.localPosition.z;
                    float fromZ= ChessmanManager.chessman[id]._z;
                    if (_z > (fromZ-9f) && _z < (fromZ+9f))
                    {
                        if (PositionManager.canMoveHorse(id, _x, _z, -1))
                        {
                            points[i].ShowBeams();
							hidePoints.Add (points[i]);
                        }

                    }
                }
            }
        }
    }

    void ShowRookWay(int id)
    {
        for (int i = 0; i < 90; i++)
        {
            if (!points[i].isOccupied)
            {
                float _x = points[i].transform.localPosition.x;
                float _z = points[i].transform.localPosition.z;
                if (_x== ChessmanManager.chessman[id]._x || _z== ChessmanManager.chessman[id]._z)
                {
                    if (PositionManager.canMoveRook(id, _x, _z, -1))
                    {
                        points[i].ShowBeams();
						hidePoints.Add (points[i]);
                    }
                }
            }
        }
    }

    void ShowCannonWay(int id)
    {
        for (int i = 0; i < 90; i++)
        {
            if (!points[i].isOccupied)
            {
                float _x = points[i].transform.localPosition.x;
                float _z = points[i].transform.localPosition.z;
                if (_x == ChessmanManager.chessman[id]._x || _z == ChessmanManager.chessman[id]._z)
                {
                    if (PositionManager.canMoveCannon(id, _x, _z, -1))
                    {
                        points[i].ShowBeams();
						hidePoints.Add (points[i]);
                    }
                }
            }
        }
    }

    void ShowPawnWay(int id)
    {
        for (int i = 0; i < 90; i++)
        {
            if (!points[i].isOccupied)
            {
                float _x = points[i].transform.localPosition.x;
                
                float fromX = ChessmanManager.chessman[id]._x;
                if (_x<(fromX+9f) && _x > (fromX - 9f))
                {
                    float _z = points[i].transform.localPosition.z;
                    float fromZ = ChessmanManager.chessman[id]._z;
                    if (_z>(fromZ-6f) && _z<(fromZ+6f))
                    {
                        if (PositionManager.canMovePawn(id, _x, _z, -1))
                        {
                            points[i].ShowBeams();
							hidePoints.Add (points[i]);
                        }
                    }
                }

            }
        }
    }

    #endregion


}
