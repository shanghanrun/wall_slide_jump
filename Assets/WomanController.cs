using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WomanController : MonoBehaviour
{
    Vector3 startPos;
    Vector3 endPos;
    private Animator animator;
    float speed = 0.5f;

    void Start()
    {
        startPos = transform.position;
        endPos = new Vector3(startPos.x, startPos.y +8, startPos.z);
        animator = transform.GetComponent<Animator>();

        // Apply Root Motion을 꺼서 애니메이션이 이동을 방해하지 않도록 설정
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //PingPong을 사용하여 왕복이동
        float t = Mathf.PingPong(Time.time *speed, 1f);
        transform.position = Vector3.Lerp(startPos, endPos, t);
    }

    void OnCollisionEnter2D(Collision2D collision){
        if (collision.gameObject.tag =="Player"){
            animator.SetBool("gameEnd", true);
        }
    }
}
