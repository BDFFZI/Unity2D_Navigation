//#define 可视化
//#define 消息台
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 导航烘焙 : MonoBehaviour
{
    public float 烘焙总长;
    public float 烘焙总宽;
    public int 烘焙精度;        //每个边至少的烘焙器数量
    public int 烘焙补偿 = 10;        //复制地图数据时额外增加一圈备用地图数据，防止AI被卡位
    public GameObject 烘焙器;

    public static 导航烘焙 Get;
    public static 导航区块[,] 烘焙数据;

    private float 烘焙器边长;

    private void Awake()        //基础初始化原始烘焙数据
    {
        Get = this;
        float 平均长度 = 烘焙总长 / 烘焙精度;
        float 平均宽度 = 烘焙总宽 / 烘焙精度;

        烘焙器边长 = 平均长度 > 平均宽度 ? 平均宽度 : 平均长度;
        Vector2 烘焙原点 = (Vector2)transform.position + new Vector2(烘焙器边长 / 2, 烘焙器边长 / 2);         //以挂载物体为烘焙原点

        烘焙数据 = new 导航区块[(int)(烘焙总长 / 烘焙器边长) + 1, (int)(烘焙总宽 / 烘焙器边长) + 1];

        float 长 = 0, 宽 = 0;
        for (int y = 0; 宽 < 烘焙总宽; y++, 宽 = 烘焙器边长 * y)
        {
            for (int x = 0; 长 < 烘焙总长; x++, 长 = 烘焙器边长 * x)
            {
                烘焙数据[x, y] = Instantiate(烘焙器, 烘焙原点 + new Vector2(x * 烘焙器边长, y * 烘焙器边长), new Quaternion(), transform).GetComponent<导航区块>();
                烘焙数据[x, y].路径数据.初始化(x, y);      //初始化【路径区块】的(x,y)数据
                烘焙数据[x, y].transform.localScale = new Vector2(烘焙器边长, 烘焙器边长);
            }
            长 = 0;
        }
    }

    public 导航区块 空间坐标转导航区块(Vector2 坐标)
    {
        坐标 -= (Vector2)transform.position;
        for (int i = 0; i < 9; i++)
        {
            int x = (int)(坐标.x / 烘焙器边长);
            int y = (int)(坐标.y / 烘焙器边长);
            switch (i)
            {
                case 0: break;
                case 1: y++; break;
                case 2: x++; break;
                case 3: y--; break;
                case 4: x--; break;
                case 5: x++; y++; break;
                case 6: x++; y--; break;
                case 7: x--; y--; break;
                case 8: x--; y++; break;
            }
            try
            {
                if (!烘焙数据[x, y].路径数据.障碍物)
                    return 烘焙数据[x, y];
            }
            catch { continue; }
        }
        return null;
    }

    public 路径区块[,] 导航权重绘制(导航区块 起点, 导航区块 终点, AI导航 请求者)
    {
        if (!终点)
        {
#if 消息台
            if (请求者.调试模式) Debug.LogWarning("导航烘焙：参数错误）：尝试修复>>1.检查目标是否越界或卡于障碍物中", 请求者);
#endif
            return null;
        }

        //确定需复制的数据范围
        int 长 = Mathf.Abs(终点.路径数据.x - 起点.路径数据.x) + 烘焙补偿;
        int 宽 = Mathf.Abs(终点.路径数据.y - 起点.路径数据.y) + 烘焙补偿;
        int 原点x = Mathf.Min(起点.路径数据.x, 终点.路径数据.x) - 烘焙补偿 / 2;
        int 原点y = Mathf.Min(起点.路径数据.y, 终点.路径数据.y) - 烘焙补偿 / 2;

        //防止烘焙补偿越界
        #region 烘焙补偿优化
        if (原点x < 0)
        {
            长 += 原点x;
            原点x = 0;
        }
        if (原点y < 0)
        {
            宽 += 原点y;
            原点y = 0;
        }
        int 超长 = 长 + 原点x + 1 - 烘焙数据.GetLength(0);
        if (超长 > 0)
        {
            长 -= 超长;
        }
        int 超宽 = 宽 + 原点y + 1 - 烘焙数据.GetLength(1);
        if (超宽 > 0)
        {
            宽 -= 超宽;
        }
        #endregion

        路径区块[,] 路径数据 = new 路径区块[长, 宽];

        for (int y = 宽 - 1; y >= 0; y--)
        {
            for (int x = 长 - 1; x >= 0; x--)
            {
                路径数据[x, y] = new 路径区块(烘焙数据[x + 原点x, y + 原点y].路径数据);     //复制导航烘焙数据
                if (!路径数据[x, y].障碍物)
                {
#if 可视化
                    if (请求者.调试模式)
                        if (x * y == 0 || x == 长 - 1 || y == 宽 - 1)
                            烘焙数据[x + 原点x, y + 原点y].GetComponent<SpriteRenderer>().color = Color.yellow;
                        else
                            烘焙数据[x + 原点x, y + 原点y].GetComponent<SpriteRenderer>().color = Color.white;
#endif
                    路径数据[x, y].权重初始化(1, (烘焙数据[x + 原点x, y + 原点y].transform.position - 终点.transform.position).sqrMagnitude);      //计算【路径区块】终点距离权重
                }
            }
        }
        return 路径数据;
    }
}
