using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mario : MonoBehaviour
{
    public const int MARIO_REBRORN = -2;  //重生
    public const int MARIO_DIE = -1;
    public const int MARIO_SMALL = 0;
    public const int MARIO_BIG = 1;
    public const int MARIO_FIRE = 2;

    public int state = MARIO_SMALL;            //玛丽奥状态标识

    [Range(0, 100)]
    public float movePower = 10;    //移动速度

    [Range(0, 2000)]
    public float jumpPower = 20;    //跳跃速度

    public float ReboundSpeed = 10; //反弹速度
    public float ReboundTime = 0.015f; //反弹延时时间


    private Rigidbody2D body;       //声明本刚体组件
    private Animator anim;          //声明动画机组件
    public AudioClip dieSound;      //声明死亡音频组件
    public Collider2D coll;         //声明碰撞体组件

    private bool isBreaking; //刹车标记
    private bool isOnGround; //着陆标记
    private bool isJumpUp;   //上升标记
    private bool isHurt;      //受伤标记




    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state != MARIO_DIE) 
        //-------------------------------状态不为死亡，执行以下操作--------------------------------------------------------------------
        {
            float h = Input.GetAxis("Horizontal");                    //模拟摇杆
            float v = Input.GetAxis("Vertical");
            Vector3 force = Vector3.right * h;

            isOnGround = CheckGroundAndEnemy();                               //每一帧做着陆检测
            isHurt = CheckHurt();                                               //每一帧检测受伤
            Debug.Log(isHurt);

            if (isHurt)                                                //假如死亡，改变状态
            {
               
                state = MARIO_DIE;
                Camera.main.GetComponent<AudioSource>().clip = dieSound;
                Camera.main.GetComponent<AudioSource>().loop = false;
                Camera.main.GetComponent<AudioSource>().Play();
                Invoke("DieFall", 0.1f);
            }


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




            if (isOnGround)  //如果角色在地面
            {
                anim.SetBool("onGround", true);
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
            else  //如果角色在空中
            {
                anim.SetBool("onGround", false);
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
                        body.velocity = new Vector2(body.velocity.x, Mathf.Min(body.velocity.y, 10));      //重置速度，从10和y轴方向速度之间取最小那个值
                    }
                }
            }


            //**********************************\状态控制***********************************************
        }

        else
        //------------------------------状态为死亡-------------------------------------------------
        {
            anim.SetBool("die", true);
        }


    }


    //----------------------------------------------------------------------------------------------------------

    //-----射线检测发射器，参数（中心点   ，   偏移量   ，    方向         ，    距离）。一共发射三根射线中间和两个边界各一条
    bool ThreeLineCast(Vector3 center, Vector3 offset, Vector3 direction, float distance, out Transform[] hitTrans)
    {
        hitTrans = new Transform[3];   //用于存放三根射线检测到的返回结果

        //该函数返回被检测到的物体的，参数（起点，方向，距离）,可由此取得transform
        //Physics2D.Raycast();
        hitTrans[0] = Physics2D.Raycast(center + offset, direction, distance).transform;   //前射线
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
        if (speed > 1 && accerlation < 0 || speed < -1 && accerlation > 0)
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
        bool result = false;
        Physics2D.queriesStartInColliders = false;  //检测时关闭检测，防止检测到自己碰撞体
        Transform[] hitTrans;

        //因重心在底部，起点应设为，               只取整宽度，bounds.extents为碰撞体边界一半              向下多预留0.05位置，增加踩踏的灵敏度
        result = ThreeLineCast(coll.bounds.center, Vector3.right * coll.bounds.extents.x, Vector3.down, coll.bounds.extents.y + 0.05f, out hitTrans);//发射三条向下射线检测

        for (int i = 0; i < 3; i++)         //碰撞类型检测
        {
            if (hitTrans[i])
            {
                Enemies enemies = hitTrans[i].GetComponent<Enemies>();
                if (enemies)                     //如果存在Enemies组件，则为Enemise类型，需要调用Rebound（）弹跳角色
                {
                    enemies.OnHit();
                    Invoke("Rebound", ReboundTime);
                }
            }
        }


        Physics2D.queriesStartInColliders = true; //重新开启

        return result;
    }



    //-------踩踏反弹
    void Rebound()
    {
        body.velocity = Vector2.right * body.velocity.x + Vector2.up * ReboundSpeed;
    }



    //-------上左右方的伤害检测

    public bool CheckHurt()                                          //CheckHurt总方法，调用下方重载的CheckHurt()
    {
        //              高度偏移量，取100%高度,注意偏移参数offset的方向为up               向右少预留0.05位置，减少角色被伤害可能性
        bool rightCheck = CheckHurt(1f * Vector3.up * coll.bounds.extents.y, Vector3.right, coll.bounds.extents.x - 0.05f);
        bool leftCheck = CheckHurt(1f * Vector3.up * coll.bounds.extents.y, Vector3.left, coll.bounds.extents.x - 0.05f);
        bool upCheck = CheckHurt(0.8f*Vector3.right*coll.bounds.extents.x,Vector3.up,coll.bounds.extents.y);

        return rightCheck || leftCheck || upCheck;
    }



    public bool CheckHurt(Vector3 offset, Vector3 direction, float distance)  //核心逻辑
    {
        bool hurtResult = false;
        Physics2D.queriesStartInColliders = false;  //检测时关闭检测，防止检测到自己碰撞体
        Transform[] hitTrans;


        //             因角色重心在底部，起点从transform.position + (Vector3.up * transform.localScale.y / 2)处开始
        bool threeLineResult = ThreeLineCast(coll.bounds.center, offset, direction, distance, out hitTrans);//发射三条射线检测

        if (threeLineResult)
        {
            for (int i = 0; i < 3; i++)         //碰撞类型检测
            {
                if (hitTrans[i])
                {

                    Enemies enemies = hitTrans[i].GetComponent<Enemies>();
                    if (enemies)                     //如果存在Enemies组件，则为Enemise类型，需要调用。。。
                    {
                        hurtResult = true;
                        Debug.Log("Mario onHit by enemy");
                        break;
                    }
                }
            }
        }


        Physics2D.queriesStartInColliders = true; //重新开启

        return hurtResult;
    }

    //----------死亡反应
    void DieFall()
    {
        body.velocity = Vector3.up * jumpPower * 0.5f;
        Destroy(this.gameObject, 2f);
        GetComponent<Collider2D>().enabled = false;
    }


}
