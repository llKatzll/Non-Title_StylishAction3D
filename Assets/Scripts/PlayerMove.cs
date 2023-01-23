using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    #region Private Field
    CharacterController controller;
    float gravity;
    Animator animator;
    public float runspeed;
    public float threecomboendtiming;
    long lastForward; //마지막으로 W를 눌렀던 시간
    #endregion

    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetInteger("Horizontal", (int)Input.GetAxisRaw("Horizontal"));
        animator.SetInteger("Vertical", (int)Input.GetAxisRaw("Vertical"));

        if ((!animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) && //공격중이거나
             (!animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard")))
        {
            Vector3 dir = (Vector3.right * speed * Time.deltaTime * Input.GetAxisRaw("Horizontal") * (animator.GetBool("isRunning") ? runspeed : 1)) + //가로
            (Vector3.forward * speed * Time.deltaTime * Input.GetAxisRaw("Vertical") * (animator.GetBool("isRunning") ? runspeed : 1)) + //세로
            Vector3.up * gravity; //위아래
            controller.Move(dir);
        }
       
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastForward <= 300)
            {
                animator.SetBool("isRunning", true);
            }
            lastForward = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        if (Input.GetAxisRaw("Vertical") == 0)
        {
            animator.SetBool("isRunning", false);
        }

        if (controller.isGrounded)
        {
            gravity = -.1f; //중력이 특정한 힘에서 고정
        }
        else
        {
            gravity -= .05f; //한 프레임당 중력에 0.1이라는 힘을 더해준다
        }

        if (Input.GetMouseButtonDown(0)) //좌클릭을 누르는 경우
        {
            if(animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("2") && !animator.GetBool("isAttacking"))
            {
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > threecomboendtiming)
                {
                    animator.SetBool("isThreeCombo", true);
                }
                else
                {
                    animator.SetBool("isThreeCombo", false);
                }
            }
            Debug.Log("Clicked");
            if (!animator.GetBool("isAttacking") && !animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            {
                animator.CrossFade("AttackStart", .25f,0);
                animator.CrossFade("AttackStart", .25f,1);
            }
            animator.SetBool("isAttacking", true);
        }


        if (Input.GetKeyDown(KeyCode.F)) //방어모드
        {
            if (!animator.GetBool("isGuarding") && !animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard"))
            {
                animator.CrossFade("GuardStart", .25f, 0);
            }
            animator.SetBool("isGuarding", true);
        }
        if (Input.GetKeyUp(KeyCode.F)) //방어 모드 (때는데 걸리는 시간 검사 예정)
        {
            animator.SetBool("isGuarding", false);
        }
    }
}
