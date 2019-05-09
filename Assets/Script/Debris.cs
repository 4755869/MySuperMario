using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debris : MonoBehaviour
{
    //碎石生成器
    private GameObject[] debris = new GameObject[2];
    private float y_minSpeed = 15;
    private float y_maxSpeed = 13;
    private float x_speed = 2;

    GameObject debris1;
    GameObject debris2;
    GameObject debris3;
    GameObject debris4;
    // Start is called before the first frame update
    void Start()
    {
        debris[0] = Resources.Load<GameObject>("Perfabs/Effect/Debris_1");
        debris[1] = Resources.Load<GameObject>("Perfabs/Effect/Debris_2");

        SpawnDbris();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
            SpawnDbris();
    }

    void SpawnDbris()
    {
        //撞击后初始化生成4个碎石
        debris1 = Instantiate(debris[0], transform.position, Quaternion.identity);
        debris1.GetComponent<Rigidbody2D>().velocity = new Vector2(x_speed,y_minSpeed );

        debris2 = Instantiate(debris[0], transform.position, Quaternion.identity);
        debris2.GetComponent<Rigidbody2D>().velocity = new Vector2(x_speed, y_maxSpeed);

        debris3 = Instantiate(debris[1], transform.position, Quaternion.identity);
        debris3.GetComponent<Rigidbody2D>().velocity = new Vector2(-x_speed, y_minSpeed);

        debris4 = Instantiate(debris[1], transform.position, Quaternion.identity);
        debris4.GetComponent<Rigidbody2D>().velocity = new Vector2(-x_speed, y_maxSpeed);

    }


}
