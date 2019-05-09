using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    protected Animator anim;
    // Start is called before the first frame update
    //该基类暂时只做一件事，派生类都会通过基类的OnCollisionEnter2D(Collision2D collision) 检测下方是否被玩家碰撞

    void Start()
    {
       
    }

    protected virtual void OnStart()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected virtual void OnHit(GameObject gameObject)
    {

    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)  
    {

        if (collision.contacts[0].normal.y > 0)
        {
            OnHit(collision.gameObject);
        }
    }
}
