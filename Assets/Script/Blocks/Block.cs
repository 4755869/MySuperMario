using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    protected Animator anim;
    // Start is called before the first frame update
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
