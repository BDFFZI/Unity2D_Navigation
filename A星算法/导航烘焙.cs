using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 导航烘焙 : MonoBehaviour
{
    public Vector2 烘焙原点;
    public int 烘焙长度;
    public int 烘焙宽度;
    public int 烘焙精细度;
    public GameObject 烘焙器;
    public static 导航区块[,] 数据;

    private void Awake()
    {
        数据 = new 导航区块[烘焙长度, 烘焙宽度];
    }

    void Start()
    {
        for (int y = 0; y < 烘焙宽度; y++)
        {
            for (int x = 0; x < 烘焙长度; x++)
            {
                数据[x, y] = Instantiate(烘焙器, 烘焙原点 + new Vector2(x, y), new Quaternion()).GetComponent<导航区块>();
                数据[x, y].初始化(x, y, Mathf.Abs(导航系统.寻路.起点x - x) + Mathf.Abs(导航系统.寻路.起点y - y), Mathf.Abs(导航系统.寻路.终点x - x) + Mathf.Abs(导航系统.寻路.终点y - y));
            }
        }

        数据[导航系统.寻路.起点x, 导航系统.寻路.起点y].GetComponent<SpriteRenderer>().color = Color.red;
        数据[导航系统.寻路.终点x, 导航系统.寻路.终点y].GetComponent<SpriteRenderer>().color = Color.green;
    }
}
