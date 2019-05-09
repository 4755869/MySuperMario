using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mario : MonoBehaviour
{
    [Range(0,100)]
    public float movePower = 10;    //移动速度

    [Range(0, 2000)]
    public float jumpPower = 20;    //跳跃速度

    public float ReboundSpeed = 10; //反弹速度
    public float ReboundTime = 0.05f; //反弹延时时间

    public int state=0;            //玛丽奥状态标识

    private Rigidbody2D body;       //声明本刚体组件
    private Animator anim;          //声明动画机组件

    private bool isBreaking; //刹车标记
    private bool isOnGround; //着陆标记
    private bool isJumpUp;   //上升标记


    // Start is called before the first frame update
    void Start()
    {
     body = GetComponent<Rigidbody2D>();
     anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");                    //模拟摇杆
        float v = Input.GetAxis("Vertical");
        Vector3 force = Vector3.right * h;

        isOnGround = CheckGroundAndEnemy();                               //每一帧做着陆检测
     

        //*******************************简单移动控制*****************************************

        //水平移动

        //GetComponent<Rigidbody2D>().velocity = force * movePower; //速度改变位置
        body.AddForce(force * movePower);  //给一个力控制物体移动

        if (h > 0)
        {
            transform.rotation = Quaternion.identity;
        }

        if (h < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

   

        //*******************************\简单移动控制*****************************************




        //**********************************其他移动与动画转换控制***********************************************


        if (isOnGround)
        {
            anim.SetBool("onGround",true);
            body.gravityScale = 1;   //在地面重力为1

            //转向控制
            float speed = body.velocity.magnitude;  //访问magnitude属性获取速度的大小，velocity为vector2D类型

           anim.SetFloat("speed", speed);              //改变动画状态

            //刹车控制  参数（速度大小，加速度大小）
            if (shouldBreak(body.velocity.x, h))
            {
                if (!isBreaking)
                {
                    isBreaking = true;
                   anim.ResetTrigger("break");
                   anim.SetTrigger("break");
                }
            }

            //跳跃控制
            if (Input.GetKeyDown(KeyCode.K))
            {
                body.AddForce(Vector3.up * jumpPower, ForceMode2D.Impulse);
                isJumpUp = true;
            }

        }
        else
        {
            anim.SetBool("onGround",false);
            body.gravityScale = 5;  //在空中重力为5

            //上升判断
            if (body.velocity.y < 0)
            {
                isJumpUp = false;
            }

            //跳跃高度控制
            if (Input.GetKeyUp(KeyCode.K))
            {
                if (isJumpUp)
                {
                    isJumpUp = false;
                    body.velocity = new Vector2(body.velocity.x,Mathf.Min(body.velocity.y,10));      //重置速度，从10和y轴方向速度之间取最小那个值
                }
            }
        }
        //**********************************\状态控制***********************************************


       
    }


    //----------------------------------------------------------------------------------------------------------

    //-----射线检测发射器，参数（中心点   ，   偏移量   ，    方向         ，    距离）。一共发射三根射线中间和两个边界各一条
    bool ThreeLineCast(Vector3 center,Vector3 offset,Vector3 direction,float distance,out Transform[] hitTrans)
    {
        hitTrans = new Transform[3];   //用于存放三根射线检测到的返回结果

        //该函数返回被检测到的物体的，参数（起点，方向，距离）,可由此取得transform
        //Physics2D.Raycast();
        hitTrans[0] = Physics2D.Raycast(center+offset,direction,distance).transform;   //前射线
        hitTrans[1] = Physics2D.Raycast(center, direction, distance).transform;        //中射线
        hitTrans[2] = Physics2D.Raycast(center - offset, direction, distance).transform; //后射线

        //debug画线，调试用
        Debug.DrawLine(center, center + direction * distance, Color.red);
        Debug.DrawLine(center + offset, center + offset + direction * distance, Color.red);
        Debug.DrawLine(center - offset, center - offset + direction * distance, Color.red);

        return hitTrans[0] || hitTrans[1] || hitTrans[2];

    }


    //-----刹车判断，当速度方向与加速度方向不同时都进行刹车
    bool shouldBreak(float speed, float accerlation)
    {
        if (speed > 1 && accerlation < 0 || speed < -1 && accerlation >  0)
        {
            return true;
        }
        else
        {
            //当速度同向或减少到0前不进行复位，不然导致重复显示
            isBreaking = false;
            return false;
        }
    }

    //-----处于空中状态时的着陆检测
    bool CheckGroundAndEnemy()
    {
        bool result=false;
        Physics2D.queriesStartInColliders = false;  //检测时关闭检测，防止检测到自己碰撞体



        //bool frontHasGround =( Physics2D.Raycast(transform.position + Vector3.right * 0.5f, Vector3.down, 0.55f).transform!=null);        //前射线
        //Debug.DrawLine(transform.position + Vector3.right * 0.5f, transform.position + Vector3.right * 0.5f + Vector3.down * 0.55f,Color.red);
        Transform frontTran = Physics2D.Raycast(transform.position + Vector3.up * (transform.localScale.y / 2) + Vector3.right * 0.5f, Vector3.down, 0.55f).transform;        //前射线
        bool frontHasThing = (frontTran != null);
        Debug.DrawLine(transform.position + Vector3.up * (transform.localScale.y / 2) + Vector3.right * 0.5f, transform.position + Vector3.up * (transform.localScale.y / 2) + Vector3.right * 0.5f + Vector3.down * 0.55f,Color.red);

        if (frontTran)//假如返回的transform有Enemies组件，即为敌人。
        {
            if (frontTran.GetComponent<Enemies>() != null)
            {
                frontTran.GetComponent<Enemies>().OnHit();
                Invoke("Rebound", ReboundTime);
            }
        }


        // bool minHasGround =( Physics2D.Raycast(transform.position, Vector3.down,0.55f).transform!=null);                                 //中
        // Debug.DrawLine(transform.position, transform.position + Vector3.down * 0.55f, Color.red);
        Transform minTran = Physics2D.Raycast(transform.position + Vector3.up * (transform.localScale.y / 2), Vector3.down, 0.55f).transform;                               //中
        bool minHasThing = (minTran != null);
        Debug.DrawLine(transform.position + Vector3.up * (transform.localScale.y / 2), transform.position + Vector3.up * (transform.localScale.y / 2) + Vector3.down * 0.55f, Color.red);

        if (minTran)//假如返回的transform有Enemies组件，即为敌人。
        {
            if (minTran.GetComponent<Enemies>() != null)    
            {
                minTran.GetComponent<Enemies>().OnHit();
                Invoke("Rebound",ReboundTime);
            }
        }

        //bool backHasGround =( Physics2D.Raycast(transform.position - Vector3.right * 0.5f, Vector3.down,0.55f).transform!=null);           //后
        //Debug.DrawLine(transform.position-Vector3.right*0.5f,transform.position - Vector3.right * 0.5f + Vector3.down * 0.55f, Color.red);
        Transform backTran = Physics2D.Raycast(transform.position + Vector3.up * (transform.localScale.y / 2) - Vector3.right * 0.5f, Vector3.down, 0.55f).transform;           //后
        bool backHasThing = (backTran != null);
        Debug.DrawLine(transform.position + Vector3.up * (transform.localScale.y / 2) - Vector3.right * 0.5f, transform.position + Vector3.up * (transform.localScale.y / 2) - Vector3.right * 0.5f + Vector3.down * 0.55f, Color.red);

        if (backTran)//假如返回的transform有Enemies组件，即为敌人。
        {
            if (backTran.GetComponent<Enemies>() != null)
            {
                backTran.GetComponent<Enemies>().OnHit();
                Invoke("Rebound", ReboundTime);
            }
        }




        if (frontHasThing || minHasThing || backHasThing)
        {
            result=true;
        }

        Physics2D.queriesStartInColliders = true; //重新开启

        return result;
    }

    //-------踩踏反弹
    void Rebound()
    {
        body.velocity = Vector2.right * body.velocity.x + Vector2.up * ReboundSpeed;
    }


}
