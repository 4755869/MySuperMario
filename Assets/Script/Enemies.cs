 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemies : MonoBehaviour
{
    private Animator anim;
    private Collider2D coll;
    private Rigidbody2D body;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        body = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //---------------------------------------------------------------------

    public void OnHit()
    {
        anim.SetTrigger("hit");
        coll.enabled = false;
        body.isKinematic = true;
        Destroy(this.gameObject,1f);
    }

    //---------------------------------------------------------------------

    private void OnCollisionEnter2D(Collision2D collision)
    {
      
    }


}
