// --------------------------------------------------------------------------------------------------------------------
// <copyright file=ChessMove.cs company=League of HTC Vive Dev>
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
//  ChineseChessVR
// </summary>
// <author>胡良云（CloudHu）</author>
//中文注释：胡良云（CloudHu） 4/28/2017

// --------------------------------------------------------------------------------------------------------------------
using Lean;
using UnityEngine;

/// <summary>
/// FileName: ChessMove.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 走法生成
/// DateTime: 4/28/2017
/// </summary>
public class ChessMoveGenerator:MonoBehaviour
{

    #region Public Variables  //公共变量区域
	[Tooltip("存放CreatePossibleMove中产生的所有走法队列")]
	public static GlobalConst._chessMove[,] m_MoveList = new GlobalConst._chessMove[8, 80];
    
    #endregion


    #region Private Variables   //私有变量区域

    private static int m_nMoveCount;//记录m_MoveList中的走法数量
    private static LeanPool pointerPool; //指针对象池 
    private static ChessmanManager chessmanManager;    //棋子管家
    private static Transform cacheTransform;    //缓存Transform组件
    #endregion
    private void Start()
    {
        pointerPool = GetComponent<LeanPool>(); //获取对象池
        chessmanManager = GetComponent<ChessmanManager>();  //获取棋子管家
        cacheTransform = transform; //缓存Transform组件
    }
    #region Public Methods	//公共方法区域

    public static GlobalConst._chessMove[] GetChildMoveList(int depth)
    {
        GlobalConst._chessMove[] m_childMoveList = new GlobalConst._chessMove[80];
        if (m_nMoveCount>0)
        {
            for (int i = 0; i < 80; i++)
            {
                m_childMoveList[i] = m_MoveList[depth, i];
            }
        }

        return m_childMoveList;
    }

    /// <summary>
    /// 在m_MoveList中插入一个走法
    /// </summary>
    /// <param name="nFromX">原始位置X</param>
    /// <param name="nFromZ">原始位置Z</param>
    /// <param name="nToX">目标位置X</param>
    /// <param name="nToZ">目标位置Z</param>
    /// <param name="nPly">深度</param>
    /// <returns>m_MoveList中的走法数量</returns>
    public static int AddMove(int nFromX, int nFromZ, int nToX, int nToZ, int nPly)
    {
        if (nPly==100)  //深度为100则是玩家调用的,需要生成指针
        {
            //Debug.Log("选中的ID:"+GlobalConst.Instance.ChessBoard[nFromX,nFromZ]+"++目标X:" + nToX * 3 + "  目标Z:" + nToZ * 3);
            GameObject go = pointerPool.FastSpawn(new Vector3((float)nToX * 3, 1.6f, (float)nToZ * 3), Quaternion.identity, cacheTransform) as GameObject;
            chessmanManager.spawnedPointers.Add(go);
        }
        else  //其他深度则是AI自己调用的
        {
            m_MoveList[nPly, m_nMoveCount].From.x = (byte)nFromX;
            m_MoveList[nPly, m_nMoveCount].From.z = (byte)nFromZ;
            m_MoveList[nPly, m_nMoveCount].To.x = (byte)nToX;
            m_MoveList[nPly, m_nMoveCount].To.z = (byte)nToZ;
            // Debug.Log("x:" + nToX + "  z:" + nToZ);
            m_nMoveCount++;
        }
        return m_nMoveCount;
    }

    /// <summary>
    /// 产生给定棋盘上的所有合法的走法
    /// </summary>
    /// <param name="chessPosition">整个棋盘的棋子位置</param>
    /// <param name="nPly">深度</param>
    /// <param name="nSide">阵营(黑:0 红:1)</param>
    /// <returns>合法的走棋数量</returns>
    public static int CreatePossibleMove(byte[,] chessPosition, int nPly, int nSide)
    {
        int nChessID,x,z;
        //m_MoveList.Initialize();
        m_nMoveCount = 0;
        for (x = 0; x < 10; x++)
        {
            for (z = 0; z <9; z++)
            {
                if (chessPosition[x,z]!=GlobalConst.NOCHESS)
                {
                    nChessID = chessPosition[x, z];
                    if (nSide==0 && GlobalConst.Instance.IsRed(nChessID))
                    {
                        continue;   //如果产生黑棋走法,则跳过红子
                    }
                    if (nSide==1 && GlobalConst.Instance.IsBlack(nChessID))
                    {
                        continue;   //如果产生红棋走法,则跳过黑子
                    }
                    //Debug.Log(nChessID+"  "+ GlobalConst.Instance.chessmanType.Length);
                    switch (GlobalConst.Instance.chessmanType[nChessID])
                    {
                        case GlobalConst.TYPE.ROOK://車
                            Gen_RookMove(chessPosition, x, z, nPly);
                            break;
                        case GlobalConst.TYPE.CANNON:   //炮
                            Gen_CannonMove(chessPosition, x, z, nPly);
                            break;
                        case GlobalConst.TYPE.HORSE:   //马
                            Gen_HorseMove(chessPosition, x, z, nPly);
                            break;
                        case GlobalConst.TYPE.ELEPHANT:   //相
                            Gen_ElephantMove(chessPosition, x, z, nPly);
                            break;
                        case GlobalConst.TYPE.GUARD:   //仕
                            Gen_GuardMove(chessPosition, x, z, nPly);
                            break;
                        case GlobalConst.TYPE.KING:   //将帅
                            Gen_KingMove(chessPosition, x, z, nPly);
                            break;                 
                        case GlobalConst.TYPE.PAWN:    //兵
                            if (GlobalConst.Instance.IsBlack(nChessID))
                            {
                                Gen_BPawnMove(chessPosition, x, z, nPly);
                            }
                            else
                            {
                                Gen_RPawnMove(chessPosition, x, z, nPly);
                            }                    
                            break;
                        default:
                            Debug.Log("没有检测到棋子类型!!");
                            break;
                    }
                }
            }
        }
        return m_nMoveCount;
    }

    /// <summary>
    /// 产生将帅的合法走位
    /// </summary>
    /// <param name="chessPosition">整个棋盘的棋子位置</param>
    /// <param name="i">棋子当前所在的X轴坐标</param>
    /// <param name="j">棋子当前所在的Z轴坐标</param>
    /// <param name="nPly">插入的列表的第几层(深度)</param>
    public static void Gen_KingMove(byte[,] chessPosition, int i, int j, int nPly)
    {
        int x, z,enemyKingId=GlobalConst.R_KING;   //目标位置坐标
        x = i+1;  //插入前方的有效走法
        z = j;
        bool isRedKing = GlobalConst.Instance.IsRed(chessPosition[i, j]);
        if (isRedKing)
        {
            enemyKingId = GlobalConst.B_KING;
            if (x > 6 && x < 10 && IsValidMove(chessPosition, i, j, x, z))
            {
                AddMove(i, j, x, z, nPly);
            }
        }
        else
        {
            if (x >= 0 && x < 3 && IsValidMove(chessPosition, i, j, x, z))
            {
                AddMove(i, j, x, z, nPly);
            }
        }

        x = i-1;  //插入后方的有效走法
        if (isRedKing)
        {
            if (x > 6 && x < 10 && IsValidMove(chessPosition, i,j , x, z))
            {
                AddMove(i, j, x, z, nPly);
            }
        }
        else
        {
            if (x >= 0 && x < 3 && IsValidMove(chessPosition, i,j , x, z))
            {
                AddMove(i, j, x, z, nPly);
            }
        }

        x = i;  //插入左方的有效走法
        z = j+1;
        //Debug.Log("x:" + x + "  z:" + z );
        if (z > 2 && z < 6 && IsValidMove(chessPosition, i,j , x, z))
        {
            AddMove(i, j, x, z, nPly);
           // Debug.Log("x:" + x + "  z:" + z );
        }
        //插入右方的有效走法
        z = j-1;
        if (z > 2 && z < 6 && IsValidMove(chessPosition, i,j , x, z))
        {
            AddMove(i, j, x, z, nPly);
        }
        //能否击杀敌方将帅
        x = (int)ChessmanManager.chessman[enemyKingId]._x;
        z = (int)ChessmanManager.chessman[enemyKingId]._z;
        x = x == 0 ? 0 : x / 3;
        z = z == 0 ? 0 : z / 3;
        if ( IsValidMove(chessPosition, i,j , x, z))
        {
            AddMove(i, j, x, z, nPly);
        }
    }

    /// <summary>
    /// 产生红仕的合法走步
    /// </summary>
    /// <param name="chessPosition">整个棋盘的棋子位置</param>
    /// <param name="i">棋子当前所在的Z轴坐标</param>
    /// <param name="j">棋子当前所在的X轴坐标</param>
    /// <param name="nPly">插入的列表的第几层(深度)</param>
    public static void Gen_GuardMove(byte[,] chessPosition, int i, int j, int nPly)
    {
        int x, z,nChessId= chessPosition[i, j];   //目标位置坐标
        bool isRedChessman = GlobalConst.Instance.IsRed(nChessId);
        if (j != 4)
        {
            z = 4;
            if (isRedChessman)
            {
                x = 8;
            }
            else
            {
                x = 1;
            }
            if (chessPosition[x, z] == GlobalConst.NOCHESS || !GlobalConst.Instance.IsSameSide(nChessId, chessPosition[x, z]))    //同阵营
            {
                AddMove(i, j, x, z, nPly);
            }
        }
        else
        {
            x = i + 1;  //插入右下方的有效走法
            z = j + 1;
            if (chessPosition[x, z] == GlobalConst.NOCHESS || !GlobalConst.Instance.IsSameSide(nChessId, chessPosition[x, z]))    //同阵营
            {
                AddMove(i, j, x, z, nPly);
            }
               
            //插入右上方的有效走法
            z = j - 1;
            if (chessPosition[x, z] == GlobalConst.NOCHESS || !GlobalConst.Instance.IsSameSide(nChessId, chessPosition[x, z]))    //同阵营
            {
                AddMove(i, j, x, z, nPly);
            }
            x = i - 1;  //插入左下方的有效走法
            z = j + 1;
            if (chessPosition[x, z] == GlobalConst.NOCHESS || !GlobalConst.Instance.IsSameSide(nChessId, chessPosition[x, z]))    //同阵营
            {
                AddMove(i, j, x, z, nPly);
            }
            //插入左上方的有效走法
            z = j - 1;
            if (chessPosition[x, z] == GlobalConst.NOCHESS || !GlobalConst.Instance.IsSameSide(nChessId, chessPosition[x, z]))    //同阵营
            {
                AddMove(i, j, x, z, nPly);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="chessPosition">整个棋盘的棋子位置</param>
    /// <param name="i">棋子当前所在的Z轴坐标</param>
    /// <param name="j">棋子当前所在的X轴坐标</param>
    /// <param name="nPly">插入的列表的第几层(深度)</param>
    public static void Gen_ElephantMove(byte[,] chessPosition, int i, int j, int nPly)
    {
        int x, z;   //目标位置坐标
        x = i + 2;  //插入右下方的有效走法
        z = j + 2;
        if (x<10 && z<9 && IsValidMove(chessPosition,i,j,x,z))
        {
            AddMove(i, j, x, z, nPly);
        }
        //插入右上方的有效走法
        z = j - 2;
        if (x<10 && z>=0 && IsValidMove(chessPosition,i,j,x,z))
        {
            AddMove(i, j, x, z, nPly);
        }
        x = i - 2;  //插入左下方的有效走法
        z = j + 2;
        if (x >=0 && z < 9 && IsValidMove(chessPosition, i,j , x, z))
        {
            AddMove(i, j, x, z, nPly);
        }
        //插入左上方的有效走法
        z = j - 2;
        if (x >=0 && z >= 0 && IsValidMove(chessPosition, i,j , x, z))
        {
            AddMove(i, j, x, z, nPly);
        }
    }

    /// <summary>
    /// 产生马的合法走位
    /// </summary>
    /// <param name="chessPosition">整个棋盘的棋子位置</param>
    /// <param name="i">棋子当前所在的Z轴坐标</param>
    /// <param name="j">棋子当前所在的X轴坐标</param>
    /// <param name="nPly">插入的列表的第几层(深度)</param>
    public static void Gen_HorseMove(byte[,] chessPosition, int i, int j, int nPly)
    {
        int x, z;   //目标位置坐标
        //插入右下方的有效走法
        x = i + 2;  //右2
        z = j + 1;  //下1
        if (x < 10 && z < 9 && IsValidMove(chessPosition, i,j , x, z))
        {
            AddMove(i, j, x, z, nPly);
        }
        //插入右上方的有效走法
        z = j - 1;  //上1
        if (x < 10 && z >= 0 && IsValidMove(chessPosition, i,j , x, z))
        {
            AddMove(i, j, x, z, nPly);
        }
        //插入左下方的有效走法
        x = i - 2;  //左2
        z = j + 1;  //下1
        if (x >=0 && z < 9 && IsValidMove(chessPosition, i,j , x, z))
        {
            AddMove(i, j, x, z, nPly);
        }
        //插入左上方的有效走法
        z = j - 1;  //上1
        if (x >=0 && z >= 0 && IsValidMove(chessPosition, i,j , x, z))
        {
            AddMove(i, j, x, z, nPly);
        }
        //插入右下方的有效走法
        x = i + 1;  //右1
        z = j + 2;  //下2
        if (x < 10 && z < 9 && IsValidMove(chessPosition, i,j , x, z))
        {
            AddMove(i, j, x, z, nPly);
        }
        //插入右下方的有效走法
        z = j - 2;  //左2
        if (x < 10 && z >= 0 && IsValidMove(chessPosition, i, j, x, z))
        {
            AddMove(i, j, x, z, nPly);
        }
        //插入左下方的有效走法
        x = i - 1;  //左1
        z = j + 2;  //下2
        if (x >=0 && z < 9 && IsValidMove(chessPosition, i,j , x, z))
        {
            AddMove(i, j, x, z, nPly);
        }
        //插入左上方的有效走法
        z = j - 2;  //上2
        if (x >=0 && z >= 0 && IsValidMove(chessPosition, i,j , x, z))
        {
            AddMove(i, j, x, z, nPly);
        }
    }

    /// <summary>
    /// 产生車的合法走位
    /// </summary>
    /// <param name="chessPosition">整个棋盘的棋子位置</param>
    /// <param name="x">棋子当前所在的Z轴坐标</param>
    /// <param name="z">棋子当前所在的X轴坐标</param>
    /// <param name="nPly">插入的列表的第几层(深度)</param>
    public static void Gen_RookMove(byte[,] chessPosition, int x, int z, int nPly)
    {
        int nChessID = chessPosition[x, z]; //当前棋子的ID
        //目标位置坐标
        int _x = x;
        int _z = z+1;    //右边
        while (_z<9)
        {
            if (chessPosition[_x,_z]==GlobalConst.NOCHESS)
            {
                AddMove(x, z, _x, _z, nPly);  //将该走法插入到m_MoveList
            }
            else
            {
                if (!GlobalConst.Instance.IsSameSide(nChessID, chessPosition[_x, _z]))
                {
                    AddMove(x, z, _x, _z, nPly);  //将该走法插入到m_MoveList
                }
                break;
            }
            _z++;
        }
        _z = z - 1;    //左边
        while (_z >= 0)
        {
            if (chessPosition[_x, _z] == GlobalConst.NOCHESS)
            {
                AddMove(x, z, _x, _z, nPly);  //将该走法插入到m_MoveList
            }
            else
            {
                if (!GlobalConst.Instance.IsSameSide(nChessID, chessPosition[_x, _z]))
                {
                    AddMove(x, z, _x, _z, nPly);  //将该走法插入到m_MoveList
                }
                break;
            }
            _z--;
        }
        _x = x+1;    //下边
        _z = z;    
        while (_x < 10)
        {
            if (chessPosition[_x, _z] == GlobalConst.NOCHESS)
            {
                AddMove(x, z, _x, _z, nPly);  //将该走法插入到m_MoveList
            }
            else
            {
                if (!GlobalConst.Instance.IsSameSide(nChessID, chessPosition[_x, _z]))
                {
                    AddMove(x, z, _x, _z, nPly);  //将该走法插入到m_MoveList
                }
                break;
            }
            _x++;
        }
        _x = x - 1;  //上边  
        while (_x >= 0)
        {
            if (chessPosition[_x, _z] == GlobalConst.NOCHESS)
            {
                AddMove(x, z, _x, _z, nPly);  //将该走法插入到m_MoveList
            }
            else
            {
                if (!GlobalConst.Instance.IsSameSide(nChessID, chessPosition[_x, _z]))
                {
                    AddMove(x, z, _x, _z, nPly);  //将该走法插入到m_MoveList
                }
                break;
            }
            _x--;
        }
    }

    /// <summary>
    /// 产生炮的合法走位
    /// </summary>
    /// <param name="chessPosition">整个棋盘的棋子位置</param>
    /// <param name="i">棋子当前所在的Z轴坐标</param>
    /// <param name="j">棋子当前所在的X轴坐标</param>
    /// <param name="nPly">插入的列表的第几层(深度)</param>
    public static void Gen_CannonMove(byte[,] chessPosition, int i, int j, int nPly)
    {
        int x, z;   //目标位置坐标
        bool flag = false;
        int nChessID = chessPosition[i, j]; //当前棋子的ID
        //Debug.Log("x:" + i + "z" + j + "id:" + nChessID);       // 7,1
        x = i + 1;    //右边
        z = j;
        while (x < 10)
        {
            //Debug.Log("目标x:" + x + "目标z" + z + "目标id:" + chessPosition[x, z]);
            if (chessPosition[x, z] == GlobalConst.NOCHESS)
            {
                
                if (!flag) {    //没有隔子
                    AddMove(i, j, x, z, nPly);  //将该走法插入到m_MoveList
                }
            }
            else
            {
                if (!flag)  //发现隔子
                {
                    flag = true;
                }
                else  //有隔子,则可以炮打翻山
                {
                    if (!GlobalConst.Instance.IsSameSide(nChessID, chessPosition[x, z]))
                    {
                        AddMove(i, j, x, z, nPly);  //将该走法插入到m_MoveList
                    }
                    break;
                }

            }
            x++;
        }

        x = i - 1;    //左边
        flag = false;
        while (x >=0 )
        {
            if (chessPosition[x, z] == GlobalConst.NOCHESS)
            {
                if (!flag)
                {    //没有隔子
                    AddMove(i, j, x, z, nPly);  //将该走法插入到m_MoveList
                }
            }
            else
            {
                if (!flag)  //发现隔子
                {
                    flag = true;
                }
                else  //有隔子,则可以炮打翻山
                {
                    if (!GlobalConst.Instance.IsSameSide(nChessID, chessPosition[x, z]))
                    {
                        AddMove(i, j, x, z, nPly);  //将该走法插入到m_MoveList
                    }
                    break;
                }

            }
            x--;
        }
        x = i;    //上边
        z = j+1;
        flag = false;
        while (z < 9)
        {
            if (chessPosition[x, z] == GlobalConst.NOCHESS)
            {
                if (!flag)
                {    //没有隔子
                    AddMove(i, j, x, z, nPly);  //将该走法插入到m_MoveList
                }
            }
            else
            {
                if (!flag)  //发现隔子
                {
                    flag = true;
                }
                else  //有隔子,则可以炮打翻山
                {
                    if (!GlobalConst.Instance.IsSameSide(nChessID, chessPosition[x, z]))
                    {
                        AddMove(i, j, x, z, nPly);  //将该走法插入到m_MoveList
                    }
                    break;
                }

            }
            z++;
        }
        //下边
        z = j - 1;
        flag = false;
        while (z >= 0)
        {
            if (chessPosition[x, z] == GlobalConst.NOCHESS)
            {
                if (!flag)
                {    //没有隔子
                    AddMove(i, j, x, z, nPly);  //将该走法插入到m_MoveList
                }
            }
            else
            {
                if (!flag)  //发现隔子
                {
                    flag = true;
                }
                else  //有隔子,则可以炮打翻山
                {
                    if (!GlobalConst.Instance.IsSameSide(nChessID, chessPosition[x, z]))
                    {
                        AddMove(i, j, x, z, nPly);  //将该走法插入到m_MoveList
                    }
                    break;
                }

            }
            z--;
        }
    }

    /// <summary>
    /// 产生红卒的合法走位
    /// </summary>
    /// <param name="chessPosition">整个棋盘的棋子位置</param>
    /// <param name="i">棋子当前所在的Z轴坐标</param>
    /// <param name="j">棋子当前所在的X轴坐标</param>
    /// <param name="nPly">插入的列表的第几层(深度)</param>
    public static void Gen_RPawnMove(byte[,] chessPosition, int i, int j, int nPly)
    {
        int x, z;   //目标位置坐标
        int nChessID = chessPosition[i, j]; //当前棋子的ID
        //Debug.Log("x:" + i + "  z:" + j + "++id:" + nChessID);
        x = i - 1;
        z = j;
        //Debug.Log("目标x:" + i + "  目标z:" + j + "  目标id:" + chessPosition[x, z]);
        if (x>=0 && !GlobalConst.Instance.IsSameSide(nChessID,chessPosition[x,z]))
        {
            AddMove(i, j, x, z, nPly);  //将该走法插入到m_MoveList
        }
        if (i<5)    //过河兵
        {
            x = i;
            z = j+1;    //右边
            if (z<9 && !GlobalConst.Instance.IsSameSide(nChessID, chessPosition[x, z]))
            {
                AddMove(i, j, x, z, nPly);  //将该走法插入到m_MoveList
            }

            z = j-1;    //左边
            if (z >= 0 && !GlobalConst.Instance.IsSameSide(nChessID, chessPosition[x, z]))
            {
                AddMove(i, j, x, z, nPly);  //将该走法插入到m_MoveList
            }
        }
    }

    /// <summary>
    /// 产生黑兵的走位
    /// </summary>
    /// <param name="chessPosition">整个棋盘的棋子位置</param>
    /// <param name="i">棋子当前所在的Z轴坐标</param>
    /// <param name="j">棋子当前所在的X轴坐标</param>
    /// <param name="nPly">插入的列表的第几层(深度)</param>
    public static void Gen_BPawnMove(byte[,] chessPosition, int i, int j, int nPly)
    {
        int x, z;   //目标位置坐标
        int nChessID = chessPosition[i, j]; //当前棋子的ID
        x = i + 1;  //向前
        z = j;
        if (x < 10 && !GlobalConst.Instance.IsSameSide(nChessID, chessPosition[x, z]))
        {
            AddMove(i, j, x, z, nPly);  //将该走法插入到m_MoveList
        }
        if (i > 4)    //过河兵
        {
            x = i;
            z = j + 1;    //右边
            if (z < 9 && !GlobalConst.Instance.IsSameSide(nChessID, chessPosition[x, z]))
            {
                AddMove(i, j, x, z, nPly);  //将该走法插入到m_MoveList
            }

            z = j - 1;    //左边
            if (z >= 0 && !GlobalConst.Instance.IsSameSide(nChessID, chessPosition[x, z]))
            {
                AddMove(i, j, x, z, nPly);  //将该走法插入到m_MoveList
            }
        }
    }

    /// <summary>
    /// 检查一个走法是否合法
    /// </summary>
    /// <param name="chessPosition">整个棋盘的棋子位置</param>
    /// <param name="nFromX">原始位置X</param>
    /// <param name="nFromZ">原始位置Z</param>
    /// <param name="nToX">目标位置X</param>
    /// <param name="nToZ">目标位置Z</param>
    /// <returns>合法为true</returns>
    public static bool IsValidMove(byte[,] chessPosition, int nFromX, int nFromZ, int nToX, int nToZ)
    {
        int i, j,nMoveChessId,nTargetId;
        if (nFromX==nToX && nFromZ==nToZ)   //原地不动
        {
            return false;
        }
        //Debug.Log(nFromX + "  " + nFromZ);
        nMoveChessId = chessPosition[nFromX,nFromZ];    //获取对应位置的ID
        //Debug.Log(nToX + "  " + nToZ);
        nTargetId = chessPosition[nToX, nToZ];          //获取目标的id

        if (nTargetId != GlobalConst.NOCHESS && GlobalConst.Instance.IsSameSide(nMoveChessId,nTargetId))    //同阵营
        {
            //Debug.Log("x:" + nToX + "  z:" + nToZ);
            return false;
        }
        bool isRedChessman = GlobalConst.Instance.IsRed(nMoveChessId);
        //Debug.Log(nMoveChessId + " " + GlobalConst.Instance.chessmanType.Length);
        switch (GlobalConst.Instance.chessmanType[nMoveChessId])
        {
            case GlobalConst.TYPE.KING:    
                if (isRedChessman)
                {
                    if (nTargetId == GlobalConst.B_KING) //将帅见面
                    {
                        if (nFromZ != nToZ) //如果横向不在一条直线上
                        {
                            return false;
                        }

                        for (i = nFromX - 1; i > nToX; i--)
                        {
                            if (chessPosition[i, nFromZ] != GlobalConst.NOCHESS) //将帅之间有隔子
                            {
                                return false;
                            }
                        }
                    }
                }
                else//黑将
                {
                    if (nTargetId == GlobalConst.R_KING)  //判断是否是将帅见面
                    {
                        if (nFromZ != nToZ)   //如果横坐标不相等,则将帅不在同一列上
                        {
                            return false;
                        }
                        for (i = nFromX + 1; i < nToX; i++)
                        {
                            if (chessPosition[i, nFromZ] != GlobalConst.NOCHESS) //如果将帅之间有隔子
                            {
                                return false;
                            }
                        }
                    }
                }

                break;
            case GlobalConst.TYPE.ELEPHANT:    
                if (isRedChessman)  //红象
                {
                    if (nToX < 5) //红象不能过河
                    {
                        return false;
                    }
                }
                else //黑象
                {
                    if (nToX > 4) //相不能过河
                    {
                        return false;
                    }
                }

                if (chessPosition[(nFromX+nToX)/2,(nFromZ+nToZ)/2]!=GlobalConst.NOCHESS)    //塞象眼
                {
                    return false;
                }
                break;
            case GlobalConst.TYPE.PAWN: 
                if (isRedChessman)  //红卒
                {
                    if (nToX > nFromX)
                    {
                        return false; //卒不可退
                    }
                    if (nFromX > 4 && nFromX == nToX)
                    {
                        return false; //卒子过河前不能横着走
                    }
                }
                else   //黑兵
                {
                    if (nToX < nFromX) //兵不可退
                    {
                        return false;
                    }
                    if (nFromX < 5 && nFromX == nToX)   //小兵过河前只能直走
                    {
                        return false;
                    }
                }
                break;           
            case GlobalConst.TYPE.HORSE:   //马
                i = nFromX;
                j = nFromZ - 1;
                if (nToX - nFromX == 2)
                {
                    i = nFromX + 1;
                    j = nFromZ;
                }
                else if (nFromX - nToX == 2)
                {
                    i = nFromX - 1;
                    j = nFromZ;
                }
                else if (nToZ - nFromZ == 2)
                {
                    i = nFromX;
                    j = nFromZ+1;
                }

                if (chessPosition[i,j]!=GlobalConst.NOCHESS)
                {
                    return false;
                }
                break;
        }
        return true;    //排除了所有不合法的走法,剩下的就是合法的
    }

    #endregion

}
