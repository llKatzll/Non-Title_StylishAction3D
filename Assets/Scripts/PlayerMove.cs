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

    Network network;

    IEnumerator Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        condition = GetComponent<PlayerCondition>();
        network = FindObjectOfType<Network>();

        for (int i = 0; i < 10; i++)
        {
            yield return null; //1프레임을 대기하는 작업 (10프레임을 대기한다)
        }

        if (!transform.CompareTag("LocalPlayer"))
        {
            network.otherPlayer = this;
            network.otherPlayerTransform = transform;
        }

        if (network.UserIndex == 0)
        {
            if (transform.CompareTag("LocalPlayer"))
            {
                transform.position = new Vector3(0, 0, 0);
                Debug.Log("LocalPlayer" + "0" + transform.position);
            }
            else
            {
                transform.position = new Vector3(0, 0, 10);
                Debug.Log("Player" + "0" + transform.position);
            }
        }
        else
        {
            if (transform.CompareTag("LocalPlayer"))
            {
                transform.position = new Vector3(0, 0, 10);
                Debug.Log("LocalPlayer" + "1" + transform.position);
            }
            else
            {
                transform.position = new Vector3(0, 0, 0);
                Debug.Log("Player" + "1" + transform.position);
            }
        }

        Application.targetFrameRate = 60;
        network.Ping();
    }

    public void damaged(Transform hitter, int downStat)
    {
        //hitter : 타격자
        //transform : 피격자
        //타격자와 피격자의 Tag를 비교해
        if (hitter.CompareTag(transform.tag)) //태그가 서로 같다면 
        {
            return;
        }

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

    void FixedUpdate()
    {
        if (!transform.CompareTag("LocalPlayer") || network.otherPlayer == null)
        {
            if (controller.isGrounded)
            {
                gravity = -.1f; //중력이 특정한 힘에서 고정
            }
            else
            {
                gravity -= .05f; //한 프레임당 중력에 0.1이라는 힘을 더해준다
            }

            if (animator.GetBool("isGuarding"))
            {
                if (condition.CanUseStamina())
                {
                    condition.StaminaUse(2);
                }
                else
                {
                    animator.SetBool("isGuarding", false);
                }
            }

            if ((!animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) && //공격중이 아니"면서"
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Skill") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")) && //스킬 사용도
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge")))
            {
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

            }
            else
            {
                if (healCoroutine != null)
                {
                    StopCoroutine(healCoroutine);
                    healCoroutine = null;
                }
            }
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

            network.Move(new float[] { Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical") });

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
    }

    void Update()
    {
        if (!transform.CompareTag("LocalPlayer"))
        {
            return;
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
            network.SendPacket(Network.NetworkOrder.PlayerRunStart, null);
        }

        //더 이상의 입력이 없으니까 || 
        if (Input.GetAxisRaw("Vertical") == 0 || !condition.CanUseStamina())
        {
            animator.SetBool("isRunning", false);
            network.SendPacket(Network.NetworkOrder.PlayerRunEnd, null);
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
                network.SendPacket(Network.NetworkOrder.PlayerAttackStart, null); //공격 시작
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
                network.SendPacket(Network.NetworkOrder.PlayerAttack, animator.GetBool("isThreeCombo").ToString());
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

            network.SendPacket(Network.NetworkOrder.PlayerGuard, null);
            animator.SetBool("isGuarding", true);
        }

        if (Input.GetKey(KeyCode.F))
        {
            if (condition.CanUseStamina()) //F키를 꾹 누르고있으면서, 스태미나를 사용할 수 있는 상황이면
            {
                condition.StaminaUse(2); //스태미나를 사용
            }
            else //스태미나를 사용할 수 없는 상황이라면
            {
                network.SendPacket(Network.NetworkOrder.PlayerGuardEnd, null); //가드 중단을 시작한다.
                animator.SetBool("isGuarding", false); //가드 중단
            }
        }

        if (Input.GetKeyUp(KeyCode.F) || (!Input.GetKey(KeyCode.F) && animator.GetBool("isGuarding"))) //방어 모드 (때는데 걸리는 시간 검사 예정)
        {
            network.SendPacket(Network.NetworkOrder.PlayerGuardEnd, null);
            animator.SetBool("isGuarding", false);
        }

        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded && (!animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) && //공격중이 아니"면서"
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Skill") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")) && //스킬 사용도
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("isJumping") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("isJumping")))
        {
            animator.SetTrigger("isJumping");
            network.SendPacket(Network.NetworkOrder.PlayerJump, null);
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

    public void Jump()
    {
        if ((!animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) && //공격중이 아니"면서"
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Skill") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")) && //스킬 사용도
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("isJumping") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("isJumping")))
        {
            animator.SetTrigger("isJumping");
        }
    }

    public void RunStart()
    {
        animator.SetBool("isRunning", true);
    }

    public void RunEnd()
    {
        animator.SetBool("isRunning", false);
    }

    public void GuardStart()
    {
        if (!animator.GetBool("isGuarding") && !animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard"))
        {
            animator.CrossFade("GuardStart", .25f, 0);

        }
        animator.SetBool("isGuarding", true);
    }

    public void GuardEnd()
    {
        animator.SetBool("isGuarding", false);
    }

    public void AttackStart()
    {
        animator.CrossFade("AttackStart", .25f, 0);
        animator.SetBool("isThreeCombo", false);
    }
    public void Attack(bool b)
    {
        if (b)
        {
            animator.SetBool("isThreeCombo", true);
        }
        animator.SetBool("isAttacking", true);
    }

    //멀티플레이어 - 다른 유저가 움직이는 함수
    public void Move(float[] dirF)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(delegate
        {
            animator.SetInteger("Horizontal", (int)dirF[0]);
            animator.SetInteger("Vertical", (int)dirF[1]);

            if ((!animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) && //공격중이 아니"면서"
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Skill") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")) && //스킬 사용도
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge")))
            {
                Vector3 dir = (transform.right * speed * Time.deltaTime * dirF[0] * (animator.GetBool("isRunning") ? runspeed : 1)) + //가로
                (transform.forward * speed * Time.deltaTime * dirF[1] * (animator.GetBool("isRunning") ? runspeed : 1)) + //세로
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
        });
    }

    public void SkillUse(SkillCommand.Skill skillType)
    {
        switch (skillType)
        {
            case SkillCommand.Skill.G:
                condition.StaminaUse(300);
                animator.CrossFade("Skill_G", .25f);
                break;
            case SkillCommand.Skill.A:
                condition.StaminaUse(150);
                animator.CrossFade("Skill_A", .25f);
                break;
            case SkillCommand.Skill.B:
                condition.StaminaUse(150);
                animator.CrossFade("Skill_B", .25f);
                break;
            case SkillCommand.Skill.C:
                condition.StaminaUse(150);
                animator.CrossFade("Skill_C", .25f);
                break;
            case SkillCommand.Skill.DodgeBack:
                condition.StaminaUse(100);
                animator.CrossFade("Dodge_Back", .25f);
                break;
            case SkillCommand.Skill.H:
                condition.StaminaUse(200);
                animator.CrossFade("Skill_H", .25f);
                break;
        }
    }

}