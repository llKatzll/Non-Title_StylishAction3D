using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkillCommand : MonoBehaviour
{
    public enum Skill
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
    }

    private Dictionary<string, Skill> whileDodgeCommand = new Dictionary<string, Skill>() {
        { "WWE", Skill.A },
        { "AAE", Skill.B },
        { "DDE", Skill.C },
    }; //구르는 도중에 인식될 커맨드

    private Dictionary<string, Skill> whileAttackCommnand = new Dictionary<string, Skill>() {
        { "ADSE", Skill.G },
    }; //평타 도중에 인식될 커맨드

    private Dictionary<string, Skill> whileIdleCommand = new Dictionary<string, Skill>() {
        { "DASE", Skill.H },
    }; //기본 상태에서 인식될 커맨드

    long lastCommandTime = 0L;
    Animator animator;
    string _tmpCommand; //우리가 입력한 커맨드 (임시로 저장될 공간)
    string tmpCommand
    {
        get
        {
            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastCommandTime > 300) //키 입력 사이 시간 차이가 0.45초를 넘기는 경우
            {
                _tmpCommand = "";
            }
            return _tmpCommand;
        }
        set
        {
            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastCommandTime > 300) //키 입력 사이 시간 차이가 0.45초를 넘기는 경우
            {
                _tmpCommand = "";
            }
            _tmpCommand = value;
            lastCommandTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            //Debug.Log(_tmpCommand);
            SkillCheck();
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Update()
    {
        if (!transform.CompareTag("LocalPlayer"))
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            tmpCommand += "A";
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            tmpCommand += "S";
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            tmpCommand += "D";
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            tmpCommand += "W";
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            tmpCommand += "X";
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            tmpCommand += "Q";
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            tmpCommand += "E";
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            tmpCommand += "C";
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            tmpCommand += "Z";
        }
    }

    void SkillCheck()
    {
        if (animator.GetNextAnimatorStateInfo(0).IsTag("Skill") || animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")) //현재 스킬이 재생중이면
        {
            return;
        }
        if (animator.GetNextAnimatorStateInfo(0).IsTag("Attack") || animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) //현재 상태가 기본 공격 상태인경우
        {
            if (whileAttackCommnand.ContainsKey(tmpCommand)) //whileAttack tmpCommand 키가 존재하는 경우
            {
                //
                Debug.Log(whileAttackCommnand[tmpCommand]);
                switch (whileAttackCommnand[tmpCommand])
                {
                    case Skill.G:
                        animator.CrossFade("Skill_G", .25f);
                        break;
                }
            }
            return;
        }

        if (animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") || animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge")) //현재 상태가 구르기 상태인경우
        {
            if (whileDodgeCommand.ContainsKey(tmpCommand)) //whileDodge tmpCommand 키가 존재하는 경우
            {
                //
                Debug.Log(whileDodgeCommand[tmpCommand]);
                switch (whileDodgeCommand[tmpCommand])
                {
                    case Skill.A:
                        animator.CrossFade("Skill_A", .25f);
                        break;
                    case Skill.B:
                        animator.CrossFade("Skill_B", .25f);
                        break;
                    case Skill.C:
                        animator.CrossFade("Skill_C", .25f);
                        break;
                }
            }
            return;
        }

        if (!animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard")) //!기본 공격 && !구르기 && !막기
        {
            //아무것도 안하는 상태
            if (Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.Q))
            {
                animator.CrossFade("Dodge_Back", .25f);
                _tmpCommand = "";
                return;
            }

            if (whileIdleCommand.ContainsKey(tmpCommand)) //whilIdle tmpCommand 키가 존재하는 경우
            {
                //
                Debug.Log(whileIdleCommand[tmpCommand]);
                switch (whileIdleCommand[tmpCommand])
                {
                    case Skill.H:
                        animator.CrossFade("Skill_H", .25f);
                        break;
                }
            }
            return;
        }

    }
}