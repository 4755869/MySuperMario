using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure :Block       //继承父类检测下方碰撞的方法
{
    private int hitCount=3;         //撞击计数器
    public AudioClip hitSound;
    public AudioClip emptySound;
    // Start is called before the first frame update
    void Start()
    {
        OnStart();   
    }

    // Update is called once per frame
     void Update()
    {
        
    }

    //****************************************************************
    //撞击处理
    protected override void OnHit(GameObject gameObject)
    {
        
        if (hitCount<= 0)
        {
            AudioSource.PlayClipAtPoint(emptySound, Camera.main.transform.position);
        }
        else
        {
            hitCount--;
            if (hitCount <= 0)
            {
                anim.SetBool("Empty", true);
            }
            AudioSource.PlayClipAtPoint(hitSound, Camera.main.transform.position);
            anim.SetTrigger("hit");
        }
      
       

    }


    //****************************************************************
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
    }
}
