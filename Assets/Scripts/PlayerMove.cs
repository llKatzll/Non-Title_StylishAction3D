using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    #region Private Field
    CharacterController controller;
    PlayerCondition condition;
    Coroutine healCoroutine;
    float gravity;
    Animator animator;
    public float runspeed;
    public float threecomboendtiming;
    long lastForward; //마지막으로 W를 눌렀던 시간
    #endregion

    public float speed;
    public int downStat = 0;
    public int maxDownStat = 0;
    // Start is called before the first frame update


    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        condition = GetComponent<PlayerCondition>();
    }

    public void damaged(Transform hitter, int downStat)
    {
        Debug.Log(hitter.name);
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Down")) //다운당하는 중일때에는 피격당하지 않는다.
        {
            return;
        }

        transform.LookAt(hitter);


        this.downStat += downStat;
        if (this.downStat >= maxDownStat)
        {
            this.downStat = 0;
            animator.CrossFade("KnockDown", .25f);
            return;
        }
        animator.SetTrigger("Damaged");
    }

    // Update is called once per frame
    void Update()
    {
        if (!transform.CompareTag("LocalPlayer"))
        {
            return;
        }
        animator.SetInteger("Horizontal", (int)Input.GetAxisRaw("Horizontal"));
        animator.SetInteger("Vertical", (int)Input.GetAxisRaw("Vertical"));

        if ((!animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) && //공격중이 아니"면서"
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Skill") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")) && //스킬 사용도
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge")))
        {
            Vector3 dir = (transform.right * speed * Time.deltaTime * Input.GetAxisRaw("Horizontal") * (animator.GetBool("isRunning") ? runspeed : 1)) + //가로
            (transform.forward * speed * Time.deltaTime * Input.GetAxisRaw("Vertical") * (animator.GetBool("isRunning") ? runspeed : 1)) + //세로
            transform.up * gravity; //위아래
            if (animator.GetBool("isRunning"))
            {
                condition.StaminaUse(1);
                if (healCoroutine != null)
                {
                    StopCoroutine(healCoroutine);
                    healCoroutine = null;
                }
            }
            else
            {
                if (healCoroutine == null)
                {
                    healCoroutine = StartCoroutine(StaminaHeal());
                }
            }
            controller.Move(dir);
        }
        else
        {
            if (healCoroutine != null)
            {
                StopCoroutine(healCoroutine);
                healCoroutine = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastForward <= 300)
            {
                if (condition.CanUseStamina())
                {
                    animator.SetBool("isRunning", true); //
                }
            }
            lastForward = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        //더 이상의 입력이 없으니까 || 
        if (Input.GetAxisRaw("Vertical") == 0 || !condition.CanUseStamina())
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

        if (Input.GetMouseButtonDown(0) && (!animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard"))
            && (!animator.GetNextAnimatorStateInfo(0).IsTag("Skill") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill"))
            && (!animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge"))) //좌클릭을 누르는 경우
        {
            if (!condition.CanUseStamina())
            {
                goto NEXT_ATTACK;
            }
            if (!animator.GetBool("isAttacking") && !animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            {
                
                animator.CrossFade("AttackStart", .25f, 0);
                animator.SetBool("isThreeCombo", false);
                goto NEXT_ATTACK;
            }

            if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= .1f) //애니메이션 10퍼센트도 안되면 선입력 판단하고 막기
            {
                goto NEXT_ATTACK;
            }

            if (animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && animator.GetNextAnimatorStateInfo(0).normalizedTime <= .1f)
            {
                goto NEXT_ATTACK;
            }

            if (!animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= .5f)
            {
                goto NEXT_ATTACK;
            }

            if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("2") && !animator.GetBool("isAttacking"))
            {
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > .3f)
                {
                    animator.SetBool("isThreeCombo", true);
                }
            }
            if (!animator.GetBool("isAttacking") && (animator.GetNextAnimatorStateInfo(0).IsTag("Attack") || animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")))
            {
                
                animator.SetBool("isAttacking", true);
            }

        }
        NEXT_ATTACK:


        if (Input.GetKeyDown(KeyCode.F) && (!animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Skill") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge"))) //방어모드
        {
            if (!animator.GetBool("isGuarding") && !animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard"))
            {
                animator.CrossFade("GuardStart", .25f, 0);

            }
            animator.SetBool("isGuarding", true);
        }
        if (Input.GetKey(KeyCode.F))
        {
            condition.StaminaUse(2);
        }
        if (Input.GetKeyUp(KeyCode.F)) //방어 모드 (때는데 걸리는 시간 검사 예정)
        {
            animator.SetBool("isGuarding", false);
        }
        if(Input.GetKeyDown(KeyCode.Space) && controller.isGrounded && (!animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) && //공격중이 아니"면서"
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Skill") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")) && //스킬 사용도
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("isJumping") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("isJumping")))
        {
            animator.SetTrigger("isJumping");

        }
    }

    IEnumerator StaminaHeal()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            condition.StaminaUse(-10);
            yield return null;
        }
    }

}
