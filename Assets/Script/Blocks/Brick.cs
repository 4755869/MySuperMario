using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : Block
{
    GameObject debrisEffect;
    int hitCount = 1;


    // Start is called before the first frame update
    void Start()
    {
        OnStart();
        debrisEffect = Resources.Load<GameObject>("Perfabs/Effect/Debris_0");
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnHit(GameObject gameObject)
    {
        Mario mario = gameObject.GetComponent<Mario>();

        if (mario.state == 0)
        {
            anim.SetTrigger("hit");
        }
        else
        {
            Debug.Log("explor");
        }
    }


    //---------------------------------------------------------
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y >0)
        {
            if (hitCount > 0)
            {
                base.OnCollisionEnter2D(collision);     //调用父类的碰撞检测，而父类的碰撞检测调用子类Brick重写的OnHit()方法
                hitCount--;
            }
            else
            {
                Instantiate(debrisEffect, transform.position, Quaternion.identity);
                Destroy(this.gameObject);
            }
        }
    }
}
