// --------------------------------------------------------------------------------------------------------------------
// <copyright file=ChessEvaluation.cs company=League of HTC Vive Dev>
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
using UnityEngine;
/// <summary>
/// FileName: ChessEvaluation.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 估值
/// DateTime: 4/28/2017
/// </summary>
public class ChessEvaluation{

    #region Public Variables  //公共变量区域
    [Header("棋子的基本估值")]
    public const int BASEVALUE_PAWN = 100;
    public const int BASEVALUE_GUARD = 250;
    public const int BASEVALUE_ELEPHANT = 250;
    public const int BASEVALUE_ROOK = 500;
    public const int BASEVALUE_HORSE = 350;
    public const int BASEVALUE_CANNON = 350;
    public const int BASEVALUE_KING = 10000;
    [Header("棋子的弹性值")]
    public const int FLEXIBILITY_PAWN = 15;
    public const int FLEXIBILITY_GUARD = 1;
    public const int FLEXIBILITY_ELEPHANT = 1;
    public const int FLEXIBILITY_ROOK = 6;
    public const int FLEXIBILITY_HORSE = 12;
    public const int FLEXIBILITY_CANNON = 6;
    public const int FLEXIBILITY_KING = 0;
    [Header("兵卒的附加值矩阵")]
    [Tooltip("红卒")]
    public int[,] R_PAWN_VALUE = { {0,0,0,0,0,0,0,0,0 },
                          {90,90,110,120,120,120,110,90,90 },
                          {90,90,110,120,120,120,110,90,90 },
                          {70,90,110,110,110,110,110,90,70 },
                          {70,70,70,70,70,70,70,70,70 },
                          {0,0,0,0,0,0,0,0,0 },
                          {0,0,0,0,0,0,0,0,0 },
                          {0,0,0,0,0,0,0,0,0 },
                          {0,0,0,0,0,0,0,0,0 },
                          {0,0,0,0,0,0,0,0,0 },
    };
    [Tooltip("黑兵的附加值矩阵")]
    public int[,] B_PAWN_VALUE = { {0,0,0,0,0,0,0,0,0 },
                          {0,0,0,0,0,0,0,0,0 },
                          {0,0,0,0,0,0,0,0,0 },
                          {0,0,0,0,0,0,0,0,0 },
                          {0,0,0,0,0,0,0,0,0 },
                          {70,70,70,70,70,70,70,70,70 },
                          {70,90,110,110,110,110,110,90,70 },
                          {90,90,110,120,120,120,110,90,90 },
                          {90,90,110,120,120,120,110,90,90 },
                          {0,0,0,0,0,0,0,0,0 },
    };
    [Tooltip("统计调用估值函数的子节点次数")]
    public int count = 0;
    #endregion


    #region Protected Variables   //受保护变量区域
    protected int[] m_BaseValue = new int[15];  //存放棋子基本估值的数组
    protected int[] m_FlexValue = new int[15];  //存放棋子弹性值的数组
    protected short[,] m_AttackPos = new short[10,9];  //存放棋子每一个被威胁的位置信息
    protected byte[,] m_GuardPos = new byte[10, 9];  //存放棋子每一个被保护的位置信息
    protected byte[,] m_FlexibilityPos = new byte[10, 9];  //存放每一个位置上的棋子的弹性值
    protected int[,] m_chessValue = new int[10,9];  //存放每一个位置上棋子的总值
    protected int nPosCount;    //记录棋子的相关位置个数
    protected Vector2[] RelatePos = new Vector2[20]; //记录一个棋子相关位置的数组
    #endregion

    #region Public Methods	//公共方法区域

    public ChessEvaluation()
    {   // 在构造函数中初始化所有棋子的基础估值
        m_BaseValue[(int)GlobalConst.TYPE.KING] = BASEVALUE_KING;
        m_BaseValue[(int)GlobalConst.TYPE.ROOK] = BASEVALUE_ROOK;
        m_BaseValue[(int)GlobalConst.TYPE.HORSE] = BASEVALUE_HORSE;
        m_BaseValue[(int)GlobalConst.TYPE.GUARD] = BASEVALUE_GUARD;
        m_BaseValue[(int)GlobalConst.TYPE.ELEPHANT] = BASEVALUE_ELEPHANT;
        m_BaseValue[(int)GlobalConst.TYPE.CANNON] = BASEVALUE_CANNON;
        m_BaseValue[(int)GlobalConst.TYPE.PAWN] = BASEVALUE_PAWN;
        //初始化弹性值数组
        m_FlexValue[(int)GlobalConst.TYPE.KING] = FLEXIBILITY_KING;
        m_FlexValue[(int)GlobalConst.TYPE.ROOK] = FLEXIBILITY_ROOK;
        m_FlexValue[(int)GlobalConst.TYPE.HORSE] = FLEXIBILITY_HORSE;
        m_FlexValue[(int)GlobalConst.TYPE.GUARD] = FLEXIBILITY_GUARD;
        m_FlexValue[(int)GlobalConst.TYPE.ELEPHANT] = FLEXIBILITY_ELEPHANT;
        m_FlexValue[(int)GlobalConst.TYPE.CANNON] = FLEXIBILITY_CANNON;
        m_FlexValue[(int)GlobalConst.TYPE.PAWN] = FLEXIBILITY_PAWN;
    }

    /// <summary>
    /// 获取兵的附加值
    /// </summary>
    /// <param name="x">X轴坐标</param>
    /// <param name="z">Z轴坐标</param>
    /// <param name="curSituation">棋局</param>
    /// <returns>不是兵返回0</returns>
    public int GetPawnValue(int x,int z,byte[,] curSituation)
    {
        int nPawnId = curSituation[x, z];
        if (nPawnId == GlobalConst.NOCHESS)
        {
            return 0;
        }
        //Debug.Log(nPawnId +" : "+ (int)GlobalConst.Instance.chessmanType.Length);
        if (GlobalConst.Instance.chessmanType[nPawnId]==GlobalConst.TYPE.PAWN)
        {
            bool bIsRedPawn = GlobalConst.Instance.IsRed(nPawnId);
            if (bIsRedPawn)//如果是红卒返回其位置的附加值
            {
                return R_PAWN_VALUE[x, z];
            }
            else   //如果是黑兵返回其位置的附加值
            {
                return B_PAWN_VALUE[x, z];
            }
        }
        return 0;   //不是兵卒返回0
    }

    /// <summary>
    /// 估值函数,对传入的棋局打分
    /// </summary>
    /// <param name="chessPosition">棋局</param>
    /// <param name="nIsRedTurn">是否是红方回合(1:红方;0:黑方)</param>
    /// <returns></returns>
	public int Evaluate(byte[,] chessPosition,int nIsRedTurn)
    {
        int i, j, k, nChessType, nTargetType;
        count++;//统计估值函数的子节点的调用次数
        //初始化数组
        m_chessValue.Initialize();       //Array.Clear(m_chessValue, 0, 360);
        m_AttackPos.Initialize();       //Array.Clear(m_AttackPos, 0, 180);
        m_GuardPos.Initialize();        // Array.Clear(m_GuardPos, 0, 90);
        m_FlexibilityPos.Initialize(); //Array.Clear(m_FlexibilityPos, 0, 90);
        //遍历棋盘,找出所有棋子,其威胁/保护的棋子,还有其弹性值
        for (i = 0; i< 10; i++)
        {
            for (j = 0; j < 9; j++)
            {
                if (chessPosition[i,j]!=GlobalConst.NOCHESS)    //如果不是空白
                {
                    nChessType = chessPosition[i, j];   //获取棋子类型
                    GetRelatePiece(chessPosition, i, j);    //找出关联位置
                    for (k = 0; k < nPosCount; k++) //遍历所有相关位置
                    {   //获取目标位置的棋子类型
                        nTargetType = chessPosition[(int)RelatePos[k].y, (int)RelatePos[k].x];
                        if (nTargetType==GlobalConst.NOCHESS)   //是空位
                        {
                            m_FlexibilityPos[i, j]++;   //灵活性增加
                        }
                        else
                        {   //是棋子
                            if (GlobalConst.Instance.IsSameSide(nChessType,nTargetType))
                            {   //如果是己方棋子,则受其保护
                                m_GuardPos[(int)RelatePos[k].y, (int)RelatePos[k].x]++;
                            }
                            else  //如果目标是敌方棋子,则受到这个棋子的威胁
                            {
                                m_AttackPos[(int)RelatePos[k].y, (int)RelatePos[k].x]++;
                                m_FlexibilityPos[i, j]++;   //灵活性增加
                                if (nTargetType==GlobalConst.R_KING)
                                {
                                    if (nIsRedTurn == 0)  //如果是黑棋回合
                                    {
                                        return 18888; //返回失败极值
                                    }
                                }
                                else if (nTargetType == GlobalConst.B_KING)
                                {
                                    if (nIsRedTurn == 1)  //如果是红棋回合
                                    {
                                        return 18888; //返回失败极值
                                    }
                                }
                                else
                                {   //不是将帅的棋子则根据威胁的棋子加分
                                    m_AttackPos[(int)RelatePos[k].y, (int)RelatePos[k].x] += (short)((30 + (m_BaseValue[(int)GlobalConst.Instance.chessmanType[nTargetType]] - m_BaseValue[(int)GlobalConst.Instance.chessmanType[nChessType]]) / 10) / 10);
                                }
                            }
                        }
                    }
                }
            }
        }
        //遍历完棋盘后,循环统计数据
        for (i = 0; i < 10; i++)
        {
            for (j = 0; j < 9; j++)
            {
                if (chessPosition[i,j]!=GlobalConst.NOCHESS)
                {
                    nChessType = chessPosition[i, j];   //棋子类型
                    m_chessValue[i, j]++;   //如果棋子存在,其价值不为0
                    //把每一个棋子的灵活性价值加进棋子的价值
                    m_chessValue[i, j] += m_FlexValue[(int)GlobalConst.Instance.chessmanType[nChessType]] * m_FlexibilityPos[i, j];
                    //再加上兵的位置附加值
                    m_chessValue[i, j] += GetPawnValue(i, j, chessPosition);
                }
            }
        }
        //循环计算每个棋子的总估值
        int nHalfValue; //棋子威胁/保护增量
        for (i = 0; i < 10; i++)
        {
            for (j = 0; j < 9; j++)
            {
                if (chessPosition[i, j] != GlobalConst.NOCHESS) //如果不是空白
                {
                    nChessType = chessPosition[i, j];   //棋子类型
                    //棋子基本估值的1/16作为威胁/保护增量
                    nHalfValue = m_BaseValue[(int)GlobalConst.Instance.chessmanType[nChessType]] / 16;
                    //将所有棋子的基本价值加入总价
                    m_chessValue[i, j] += m_BaseValue[(int)GlobalConst.Instance.chessmanType[nChessType]];
                    if (GlobalConst.Instance.IsRed(nChessType)) //如果是红棋
                    {
                        if (m_AttackPos[i,j]>0)   //当前红棋收到威胁
                        {
                            if (nIsRedTurn==1)  //红方回合
                            {
                                if (nChessType==GlobalConst.R_KING)
                                {   //如果是红帅,价值降低20
                                    m_chessValue[i, j] -= 20;
                                }
                                else
                                {   //价值减去2倍威胁增量
                                    m_chessValue[i, j] -= nHalfValue * 2;
                                    if (m_GuardPos[i,j]>0)
                                    {   //如果受到己方棋子保护,则增加估值
                                        m_chessValue[i, j] += nHalfValue;
                                    }
                                }
                            }
                            else //如果是黑方回合
                            {
                                if (nChessType==GlobalConst.R_KING)
                                {   //如果是红帅,则返回失败极值
                                    return 18888;
                                }
                                //其他棋子则减去10倍威胁增量
                                m_chessValue[i, j] -= nHalfValue * 10;
                                if (m_GuardPos[i,j]>0)
                                {   //如果受到己方棋子保护,则加上9倍保护增量
                                    m_chessValue[i, j] += nHalfValue * 9;
                                }
                            }
                            //最后加上威胁差,防止一个兵威胁一个被保护的車,而估值函数没有反映之类的问题
                            m_chessValue[i, j] -= m_AttackPos[i, j];
                        }
                        else
                        {   //不受威胁的情况
                            if (m_GuardPos[i, j] > 0)
                            {   //如果受到己方棋子保护,则加上一点优势分,因为受到威胁时才能体现保护的价值
                                m_chessValue[i, j] += 5;
                            }
                        }
                    }
                    else
                    {   //黑棋
                        if (m_AttackPos[i, j] > 0)   //当前黑棋受到威胁
                        {
                            if (nIsRedTurn == 0)  //黑方回合
                            {
                                if (nChessType == GlobalConst.B_KING)
                                {   //如果是黑将,价值降低20
                                    m_chessValue[i, j] -= 20;
                                }
                                else
                                {   //价值减去2倍威胁增量
                                    m_chessValue[i, j] -= nHalfValue * 2;
                                    if (m_GuardPos[i, j] > 0)
                                    {   //如果受到己方棋子保护,则增加估值
                                        m_chessValue[i, j] += nHalfValue;
                                    }
                                }
                            }
                            else //红方回合
                            {
                                if (nChessType == GlobalConst.B_KING)
                                {   //如果是黑将,则返回失败极值
                                    return 18888;
                                }
                                //其他棋子则减去10倍威胁增量
                                m_chessValue[i, j] -= nHalfValue * 10;
                                if (m_GuardPos[i, j] > 0)
                                {   //如果受到己方棋子保护,则加上9倍保护增量
                                    m_chessValue[i, j] += nHalfValue * 9;
                                }
                            }
                            //最后加上威胁差,防止一个兵威胁一个被保护的車,而估值函数没有反映之类的问题
                            m_chessValue[i, j] -= m_AttackPos[i, j];
                        }
                        else
                        {   //不受威胁的情况
                            if (m_GuardPos[i, j] > 0)
                            {   //如果受到己方棋子保护,则加上一点优势分,因为受到威胁时才能体现保护的价值
                                m_chessValue[i, j] += 5;
                            }
                        }
                    }
                }
            }
        }
        //下面统计红黑双方总分
        int nRedValue = 0, nBlackValue = 0;
        for (i = 0; i < 10; i++)
        {
            for (j = 0; j < 9; j++)
            {
                if (chessPosition[i, j] != GlobalConst.NOCHESS)
                {
                    nChessType = chessPosition[i, j];
                    if (GlobalConst.Instance.IsRed(nChessType))
                    {
                        nRedValue += m_chessValue[i, j];    //累加红棋的总值
                    }
                    else
                    {
                        nBlackValue += m_chessValue[i, j];    //累加黑棋的总值
                    }
                }
            }
        }

        if (nIsRedTurn==1)
        {
            return nRedValue - nBlackValue; //如果轮的红方回合则返回红方的估值
        }
        else
        {
            return  nBlackValue- nRedValue; //如果轮的黑方回合则返回黑方的估值
        }
        
    }
    #endregion

    #region Protected Methods
    /// <summary>
    /// 获取与指定位置棋子相关的所有位置
    /// </summary>
    /// <param name="chessPosition">棋局</param>
    /// <param name="j">指定棋子的X坐标</param>
    /// <param name="i">指定棋子的Z坐标</param>
    /// <returns>棋子ID</returns>
    protected int GetRelatePiece(byte[,] chessPosition,int i,int j)
    {
        nPosCount = 0;
        byte nChessID;
        bool flag=false;
        int x, z;
        nChessID = chessPosition[i, j];
        switch (GlobalConst.Instance.chessmanType[nChessID])
        {
            case GlobalConst.TYPE.KING:
                int enemyKingId = GlobalConst.R_KING;   //目标位置坐标
                x = i + 1;  //插入前方的有效走法
                z = j;
                bool isRedKing = GlobalConst.Instance.IsRed(nChessID);
                if (isRedKing)
                {
                    enemyKingId = GlobalConst.B_KING;
                    if (x > 6 && x < 10 && CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                    {
                        AddPoint(x, z);  //将该走法插入到RelatePos
                    }
                }
                else
                {
                    if (x >= 0 && x < 3 && CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                    {
                        AddPoint(x, z);  //将该走法插入到RelatePos
                    }
                }

                x = i - 1;  //插入后方的有效走法
                z = j;
                if (isRedKing)
                {
                    if (x > 6 && x < 10 && CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                    {
                        AddPoint(x, z);  //将该走法插入到RelatePos
                    }
                }
                else
                {
                    if (x >= 0 && x < 3 && CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                    {
                        AddPoint(x, z);  //将该走法插入到RelatePos
                    }
                }

                x = i;  //插入左方的有效走法
                z = j + 1;
                //Debug.Log("x:" + x + "  z:" + z );
                if (z > 2 && z < 6 && CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                {
                    AddPoint(x, z);  //将该走法插入到RelatePos
                    // Debug.Log("x:" + x + "  z:" + z );
                }
                x = i;  //插入右方的有效走法
                z = j - 1;
                if (z > 2 && z < 6 && CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                {
                    AddPoint(x, z);  //将该走法插入到RelatePos
                }

                x = (int)ChessmanManager.chessman[enemyKingId]._x;
                z = (int)ChessmanManager.chessman[enemyKingId]._z;
                x = x == 0 ? 0 : x / 3;
                z = z == 0 ? 0 : z / 3;
                if (CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                {
                    AddPoint(x, z);  //将该走法插入到RelatePos
                }
                break;
            case GlobalConst.TYPE.GUARD:
                bool bIsRedChessman = GlobalConst.Instance.IsRed(chessPosition[i, j]);
                if (j != 4)
                {
                    z = 4;
                    if (bIsRedChessman)
                    {
                        x = 8;
                    }
                    else
                    {
                        x = 1;
                    }

                    AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                    
                }
                else
                {
                    if (bIsRedChessman)
                    {
                        AddPoint(9, 3);
                        AddPoint(7, 3);
                        AddPoint(9, 5);
                        AddPoint(7, 5);
                    }
                    else
                    {
                        AddPoint(0, 3);
                        AddPoint(2, 3);
                        AddPoint(0, 5);
                        AddPoint(2, 5);
                    }
                }
                break;
            case GlobalConst.TYPE.ELEPHANT:
                x = i + 2;  //插入右下方的有效走法
                z = j + 2;
                if (x < 10 && z < 9 && CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                {
                    AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                }
                x = i + 2;  //插入右上方的有效走法
                z = j - 2;
                if (x < 10 && z >= 0 && CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                {
                    AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                }
                x = i - 2;  //插入左下方的有效走法
                z = j + 2;
                if (x >= 0 && z < 9 && CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                {
                    AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                }
                x = i - 2;  //插入左上方的有效走法
                z = j - 2;
                if (x >= 0 && z >= 0 && CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                {
                    AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                }
                break;
            case GlobalConst.TYPE.HORSE:
                //插入右下方的有效走法
                x = i + 2;  //右2
                z = j + 1;  //下1
                if (x < 10 && z < 9 && CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                {
                    AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                }
                //插入右上方的有效走法
                x = i + 2;  //右2
                z = j - 1;  //上1
                if (x < 10 && z >= 0 && CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                {
                    AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                }
                //插入左下方的有效走法
                x = i - 2;  //左2
                z = j + 1;  //下1
                if (x >= 0 && z < 9 && CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                {
                    AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                }
                //插入左上方的有效走法
                x = i - 2;  //左2
                z = j - 1;  //上1
                if (x >= 0 && z >= 0 && CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                {
                    AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                }
                //插入右下方的有效走法
                x = i + 1;  //右1
                z = j + 2;  //下2
                if (x < 10 && z < 9 && CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                {
                    AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                }
                //插入左下方的有效走法
                x = i - 1;  //左1
                z = j + 2;  //下2
                if (x >= 0 && z < 9 && CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                {
                    AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                }
                //插入右下方的有效走法
                x = i + 1;  //右1
                z = j - 2;  //左2
                if (x < 10 && z >= 0 && CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                {
                    AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                }
                //插入左上方的有效走法
                x = i - 1;  //左1
                z = j - 2;  //上2
                if (x >= 0 && z >= 0 && CanTouch(chessPosition, i, j, x, z)) //走步是否合法
                {
                    AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                }
                break;
            case GlobalConst.TYPE.ROOK:
                x = i;
                z = j + 1;    //右边
                while (z < 9)
                {
                    AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                    if (chessPosition[x, z] != GlobalConst.NOCHESS)
                    {
                        break;
                    }
                    z++;
                }
                x = i;
                z = j - 1;    //左边
                while (z >= 0)
                {
                    AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                    //Debug.Log("x:" + x + "  z:" + z);
                    if (chessPosition[x, z] != GlobalConst.NOCHESS)
                    {
                        break;
                    }
                    z--;
                }
                x = i + 1;    //下边
                z = j;
                while (x < 10)
                {
                    AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                    if (chessPosition[x, z] != GlobalConst.NOCHESS)
                    {
                        break;
                    }
                    x++;
                }
                x = i - 1;  //上边
                z = j;
                while (x >= 0)
                {
                    AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                    if (chessPosition[x, z] != GlobalConst.NOCHESS)
                    {
                        break;
                    }
                    x--;
                }
                break;
            case GlobalConst.TYPE.PAWN:
                if (GlobalConst.Instance.IsRed(nChessID))
                {
                    x = i - 1;  //向前
                    z = j;
                    if (x >= 0)
                    {
                        AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                    }
                    if (i < 5)    //过河兵
                    {
                        x = i;
                        z = j + 1;    //右边
                        if (z < 9)
                        {
                            AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                        }

                        z = j - 1;    //左边
                        if (z >= 0)
                        {
                            AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                        }
                    }
                }
                else
                {
                    x = i + 1;
                    z = j;
                    if (x < 10)
                    {
                        AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                    }
                    if (i > 4)    //过河兵
                    {
                        x = i;
                        z = j + 1;    //右边
                        if (z < 9)
                        {
                            AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                        }

                        z = j - 1;    //左边
                        if (z >= 0)
                        {
                            AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                        }
                    }
                }                                
                break;
            case GlobalConst.TYPE.CANNON:
                x = i + 1;    //右边
                z = j;
                while (x < 10)
                {
                    if (chessPosition[x, z] == GlobalConst.NOCHESS)
                    {
                        if (!flag)
                        {    //没有隔子
                            AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
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
                            AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                            break;
                        }

                    }
                    x++;
                }

                x = i - 1;    //左边
                flag = false;
                while (x >= 0)
                {
                    if (chessPosition[x, z] == GlobalConst.NOCHESS)
                    {
                        if (!flag)
                        {    //没有隔子
                            AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
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
                            AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                            break;
                        }

                    }
                    x--;
                }
                x = i;    //上边
                z = j + 1;
                flag = false;
                while (z < 9)
                {
                    if (chessPosition[x, z] == GlobalConst.NOCHESS)
                    {
                        if (!flag)
                        {    //没有隔子
                            AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
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
                            AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
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
                            AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
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
                            AddPoint(x, z);  //将该坐标位置插入到RelatePos数组
                            break;
                        }

                    }
                    z--;
                }
                break;
        }
        return nPosCount;
    }

    /// <summary>
    /// 判断位置From的棋子能否走到位置To
    /// </summary>
    /// <param name="chessPossition">棋局</param>
    /// <param name="nFromX">初始位置X轴坐标</param>
    /// <param name="nFromZ">初始位置Z轴坐标</param>
    /// <param name="nToX">目标位置X轴坐标</param>
    /// <param name="nToZ">目标位置Z轴坐标</param>
    /// <returns>可以到达为true</returns>
    protected bool CanTouch(byte[,] chessPosition,int nFromX,int nFromZ,int nToX,int nToZ)
    {
        int i, j, nMoveChessId, nTargetId;
        if (nFromX == nToX && nFromZ == nToZ)   //原地不动
        {
            return false;
        }
        nMoveChessId = chessPosition[nFromX, nFromZ];    //获取对应位置的ID
        //Debug.Log(nMoveChessId+"  x:" + nToX + "  z:" + nToZ);
        nTargetId = chessPosition[nToX, nToZ];          //获取目标的id
        bool bIsRedChessman = GlobalConst.Instance.IsRed(nMoveChessId);
        switch (GlobalConst.Instance.chessmanType[nMoveChessId])
        {
            case GlobalConst.TYPE.KING: 
                if (bIsRedChessman) //红帅
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
                else   //黑将
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
                if (bIsRedChessman)   //红象
                {
                    if (nToX < 5) //红象不能过河
                    {
                        return false;
                    }
                    if (chessPosition[(nFromX + nToX) / 2, (nFromZ + nToZ) / 2] != GlobalConst.NOCHESS)    //塞象眼
                    {
                        return false;
                    }
                }
                else    //黑象
                {
                    if (nToX > 4) //相不能过河
                    {
                        return false;
                    }
                    if (chessPosition[(nFromX + nToX) / 2, (nFromZ + nToZ) / 2] != GlobalConst.NOCHESS)    //塞象眼
                    {
                        return false;
                    }
                }
                break;         
            case GlobalConst.TYPE.HORSE:
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
                    j = nFromZ + 1;
                }

                if (chessPosition[i, j] != GlobalConst.NOCHESS)
                {
                    return false;
                }
                break;           
        }
        return true;    //排除了所有不合法的走法,剩下的就是合法的
    }

    /// <summary>
    /// 将一个点加入到相关位置RelatePos的队列
    /// </summary>
    /// <param name="x">X轴坐标</param>
    /// <param name="z">Z轴坐标</param>
    protected void AddPoint(int x,int z)
    {
        RelatePos[nPosCount].x = z;
        RelatePos[nPosCount].y = x;
        nPosCount++;
    }

    #endregion
}
