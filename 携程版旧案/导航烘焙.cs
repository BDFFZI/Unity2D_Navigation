#define 关闭     //调试模式:仅可视化/开启/关闭
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 导航烘焙 : MonoBehaviour
{
    public Vector2 烘焙原点;
    public float 烘焙总长;
    public float 烘焙总宽;
    public int 烘焙精度;
    public int 烘焙补偿;
    public GameObject 烘焙器;

    public static 导航烘焙 Get;
    public static 导航区块[,] 烘焙数据;

    private float 烘焙器边长;

    private void Awake()
    {
        Get = this;
        float 平均长度 = 烘焙总长 / 烘焙精度;
        float 平均宽度 = 烘焙总宽 / 烘焙精度;

        烘焙器边长 = 平均长度 > 平均宽度 ? 平均宽度 : 平均长度;
        烘焙原点 = 烘焙原点 + new Vector2(烘焙器边长 / 2, 烘焙器边长 / 2);

        烘焙数据 = new 导航区块[(int)(烘焙总长 / 烘焙器边长) + 1, (int)(烘焙总宽 / 烘焙器边长) + 1];

        float 长 = 0, 宽 = 0;
        for (int y = 0; 宽 < 烘焙总宽; y++, 宽 = 烘焙器边长 * y)
        {
            for (int x = 0; 长 < 烘焙总长; x++, 长 = 烘焙器边长 * x)
            {
                烘焙数据[x, y] = Instantiate(烘焙器, 烘焙原点 + new Vector2(x * 烘焙器边长, y * 烘焙器边长), new Quaternion()).GetComponent<导航区块>();
                烘焙数据[x, y].路径数据.初始化(x, y);
                烘焙数据[x, y].transform.localScale = new Vector2(烘焙器边长, 烘焙器边长);
            }
            长 = 0;
        }
    }

    public 导航区块 空间坐标转导航区块(Vector2 坐标)
    {
        坐标 -= 烘焙原点;
        for (int i = 0; i < 9; i++)
        {
            int x = (int)Mathf.Round(坐标.x / 烘焙器边长);
            int y = (int)Mathf.Round(坐标.y / 烘焙器边长);
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

    public 路径区块[,] 导航权重绘制(导航区块 起点, 导航区块 终点)
    {
        if (终点.路径数据 == null)
        {
#if 开启
            Debug.LogWarning("导航权重绘制:无法获取导航区块数据");
#endif
            return null;
        }

        int 长 = Mathf.Abs(终点.路径数据.x - 起点.路径数据.x) + 烘焙补偿;
        int 宽 = Mathf.Abs(终点.路径数据.y - 起点.路径数据.y) + 烘焙补偿;
        int 原点x = Mathf.Min(起点.路径数据.x, 终点.路径数据.x) - 烘焙补偿 / 2;
        int 原点y = Mathf.Min(起点.路径数据.y, 终点.路径数据.y) - 烘焙补偿 / 2;

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
        int 超长 = 长 + 原点x - 烘焙数据.GetLength(0);
        if (超长 > 0)
        {
            长 -= 超长;
        }
        int 超宽 = 宽 + 原点y - 烘焙数据.GetLength(1);
        if (超宽 > 0)
        {
            宽 -= 超宽;
        }

        路径区块[,] 路径数据 = new 路径区块[长, 宽];

        for (int y = 宽 - 1; y >= 0; y--)
        {
            for (int x = 长 - 1; x >= 0; x--)
            {
                try
                {
                    路径数据[x, y] = new 路径区块(烘焙数据[x + 原点x, y + 原点y].路径数据);     //复制导航烘焙数据
                    if (!路径数据[x, y].障碍物)
                    {
#if 开启 || 仅可视化
                        if (x * y == 0 || x == 长 - 1 || y == 宽 - 1)
                            烘焙数据[x + 原点x, y + 原点y].GetComponent<SpriteRenderer>().color = Color.yellow;
                        else
                            烘焙数据[x + 原点x, y + 原点y].GetComponent<SpriteRenderer>().color = Color.white;
#endif
                        路径数据[x, y].权重初始化(1, (烘焙数据[x + 原点x, y + 原点y].transform.position - 终点.transform.position).sqrMagnitude);
                    }
                }
                catch
                {
#if  开启
                    Debug.LogWarning("脱离烘焙区域"); return null;
#endif
                }
            }
        }

        return 路径数据;
    }
}
