using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class AI导航 : MonoBehaviour
{
    public bool 调试模式;
    [Range(0f, 0.2f)]
    public float 移动速度 = 0.1f;
    public float 刹车距离 = 0.1f;
    public float 移动间隔 = 0.01f;
    [Range(0f, 1f)]
    public float 更新延迟 = 1;

    [System.NonSerialized] public Transform 目标;
    [System.NonSerialized] public float 平方距离;
    [System.NonSerialized] public bool 寻路中;
    private Stack<路径区块> 路径;
    private float 更新冷却;

    private void Awake()
    {
        刹车距离 = 刹车距离 * 刹车距离;
    }

    private void Update()
    {
        if (目标)
        {
            if (((Vector2)transform.position - (Vector2)目标.transform.position).sqrMagnitude < 刹车距离)
            {
                停止寻路();
                return;
            }
            平方距离 = ((Vector2)transform.position - (Vector2)目标.transform.position).sqrMagnitude;

            if (更新冷却 > Mathf.Max(移动间隔, 平方距离 * 0.01f * 更新延迟))      //根据距离更变路径更新速度
            {
                更新冷却 = 0;
                路径 = 导航系统.Get.导航(
                    导航烘焙.Get.空间坐标转导航区块(transform.position).路径数据,
                    导航烘焙.Get.导航权重绘制(
                            导航烘焙.Get.空间坐标转导航区块(transform.position),
                            导航烘焙.Get.空间坐标转导航区块(目标.position),
                            this),
                    this);
                if (路径 != null)
                {
                    StopCoroutine("移动");
                    StartCoroutine("移动");
                }              
            }
            更新冷却 += Time.deltaTime;
        }
    }

    public void 开始寻路(Transform 目标)
    {
        if (this.目标 != 目标)
        {
            更新冷却 = float.MaxValue;
            this.目标 = 目标;
            寻路中 = true;
        }
    }

    public void 停止寻路()
    {
        StopAllCoroutines();
        目标 = null;
        寻路中 = false;
    }

    IEnumerator 移动()
    {
        WaitForSeconds s = new WaitForSeconds(移动间隔);
        Stack<路径区块> 临时路径 = 路径;
        while (临时路径.Count > 0)
        {
            路径区块 i = 临时路径.Pop();
            Vector2 路径点 = 导航烘焙.烘焙数据[i.x, i.y].transform.position;
            while (((Vector2)transform.position - 路径点).sqrMagnitude > 刹车距离)
            {
                transform.position += ((Vector3)路径点 - transform.position).normalized * 移动速度;        //当值大于超过导航区块时会鬼畜
                yield return s;
            }
        }
        停止寻路();
    }
}
