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
    public RuntimeAnimatorController[] marioControllers;  //用于替换动画控制器animatorControll，以便做状态切换效果
    public AudioClip dieSound;      //声明死亡音频组件
    private AudioClip powerupCilp;   //声明升级的音频组件
    private AudioClip pipeClip;      //声明缩小时的音频组件
    public Collider2D coll;         //声明碰撞体组件
    private SpriteRenderer spriteRenderer; //声明spriteRenderer组件

    private bool isBreaking; //刹车标记
    private bool isOnGround; //着陆标记
    private bool isJumpUp;   //上升标记
    private bool isHurt;      //受伤标记

    private float changeStateBeginTime;    //记录闪烁特效开始时刻
    private float changeStatePauseTime = 1f;  //闪烁特效时长
    private bool isChangingState;  //是否处于变换状态，用于闪烁特效
    private float blinkInterval=0.05f;   //闪烁间隔，用于闪烁特效
    private float lastBlinkTime;   //记录闪烁时最新的闪烁时刻，用于闪烁特效
    public Sprite[] mario_s;       //存放小马里奥图片，用于闪烁特效
    public Sprite[] mario_b;       //存放大马里奥图片，用于闪烁特效
    public Sprite[] mario_f;        //存放特殊马里奥图片，用于闪烁特效
    private Sprite oldSprite;       //声明当前spriteRender所存放sprite，用于闪烁特效
    private int spriteIndex;



    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        powerupCilp = Resources.Load<AudioClip>("Sounds/smb_powerup");
        pipeClip = Resources.Load<AudioClip>("Sounds/smb_pipe");
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
       
            //--------------------------受伤检测------------------------------------
            if (isHurt)                                                //假如死亡，改变状态
            {
                if (state == MARIO_SMALL)
                {
                    Die();
                }
                else
                {
                    BeginChangeState(MARIO_SMALL);
                }
                
            }

            //-------------------------变身------------------------------------------
            if (!isChangingState)
            {
                if (Input.GetKeyDown(KeyCode.P))
                {
                    BeginChangeState(MARIO_BIG);
                }
                if (Input.GetKeyDown(KeyCode.O))
                {
                    BeginChangeState(MARIO_SMALL);
                }
            }


            //判断，若处于变身状态，就进入闪烁特效的逻辑控制
            if (isChangingState)
            {
                /*该判断用于控制闪烁时长。changeStateBeginTime只会在上方的变身判断调用BeginChange()时赋值一次，变身的开始时间，
                 * 在之后闪烁期间便不会更新，因为isChangeState为false，不再进入上边的变身判断更新changeStateBeginTime。
                 * 同时，当开始时间与unscaledTime的差值越来越大，达到1秒时，便退出判断，暂停闪烁*/
                if (Time.unscaledTime - changeStateBeginTime < changeStatePauseTime)
                {
                    /*该判断控制闪烁频率，lastBlinkTime记录上次闪烁的时刻，
                     * 当秒表的时刻跟上次闪烁时刻（进入该判断的时刻）大于一定值——blinkInterval时，才再次进入闪烁*/
                    if (Time.unscaledTime - lastBlinkTime > blinkInterval)
                    {
                        if (spriteRenderer.sprite == oldSprite)
                        {
                            ChangeSprite(state, spriteIndex);
                        }
                        else
                        {
                            spriteRenderer.sprite = oldSprite;
                        }

                        lastBlinkTime = Time.unscaledTime;  //本次闪烁结束，将当期秒表时刻设置为最新的闪烁时刻
                    }
                    return;
                }

                else
                {
                    Debug.Log("changestate");
                    ChangeState(state);                  
                }


            }
           



            //*******************************简单移动控制*****************************************

            //水平移动

            //GetComponent<Rigidbody2D>().velocity = force * movePower; //速度改变位置

            //Time.Scale大于零时才添加力移动角色，防止GameController类暂停游戏时发生错误
            if (Time.timeScale > 0)
            {
                body.AddForce(force * movePower);  //给一个力控制物体移动
            }


            if (h > 0)
            {
                transform.rotation = Quaternion.identity;
            }

            if (h < 0)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }




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

        //因重心在底部，起点应设为，                取整宽度，bounds.extents为碰撞体边界一半              向下多预留0.05位置，增加踩踏的灵敏度
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
        bool rightCheck = CheckHurt(1f * Vector3.up * coll.bounds.extents.y, Vector3.right, coll.bounds.extents.x + 0.05f);
        bool leftCheck = CheckHurt(1f * Vector3.up * coll.bounds.extents.y, Vector3.left, coll.bounds.extents.x + 0.05f);
        bool upCheck = CheckHurt(1f*Vector3.right*coll.bounds.extents.x,Vector3.up,coll.bounds.extents.y);

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


    //----------状态改变的操作-----------------------------------------------------

    //状态改变时的闪烁动画动画主要逻辑块
    void BeginChangeState(int newState)
    {
        anim.enabled = false;

        changeStateBeginTime = Time.unscaledTime;
        
        //播声音
        if (newState == MARIO_SMALL)
        {
            Debug.Log("piperSound");
            AudioSource.PlayClipAtPoint(pipeClip, Camera.main.transform.position);
        }
        else
        {
            Debug.Log("powerupSound");
            AudioSource.PlayClipAtPoint(powerupCilp, Camera.main.transform.position);
        }

        Time.timeScale = 0;
        oldSprite = spriteRenderer.sprite;       //记录当前spriteRendeerr的sprite
        state = newState;

        //获取下标index，用于辅助改变sprites[]数组。即记录当进入闪烁的瞬间马里奥处于哪一个sprite图片中
        string[] spriteNameSplit = oldSprite.name.Split('_');
        string spriteNumberString = spriteNameSplit[spriteNameSplit.Length - 1];
        spriteIndex = int.Parse(spriteNumberString);

        isChangingState = true;                 //标记已经处于变身阶段，进入闪烁特效


       
    }


    //用于改变全局变量sprites[]，更改当前的sprite动画组，参与闪烁特效
    void ChangeSprite(int state,int index)
    {
        Sprite[] sprites = mario_s;

        if (state == MARIO_BIG)
        {
            sprites = mario_b;
        }

        if (state == MARIO_FIRE)
        {
            sprites = mario_f;
        }

        spriteRenderer.color = new Color(1, 1, 1, 0.5f);
        spriteRenderer.sprite = sprites[index];

    }

    //正式完成状态改变，重置碰撞体大小，改变animatorController，不参与闪烁特效
    void ChangeState(int newState)
    {
        spriteRenderer.color = new Color(1,1,1,1);

        isChangingState = false;
        Time.timeScale = 1;
        anim.enabled = true;
        float height = 1;
        switch (newState)
        {
            case MARIO_SMALL:
                height = 1;
                break;
            case MARIO_BIG:
                height = 2;
                break;
            case MARIO_FIRE:
                height = 2;
                break;
            default:
                break;
        }

    (coll as BoxCollider2D).size = new Vector2((coll as BoxCollider2D).size.x, height);
        (coll as BoxCollider2D).offset = new Vector2(0, height / 2);
        anim.runtimeAnimatorController = marioControllers[newState];
    }


    //---------------------死亡操作------------------------
    private void Die()
    {
        state = MARIO_DIE;
        Camera.main.GetComponent<AudioSource>().clip = dieSound;
        Camera.main.GetComponent<AudioSource>().loop = false;
        Camera.main.GetComponent<AudioSource>().Play();
        Invoke("DieFall", 0.1f);
    }

}
