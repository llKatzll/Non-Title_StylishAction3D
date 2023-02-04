using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCondition : MonoBehaviour
{
    public int currentHp; //5.0 / 10.0 > 0.5
    public int maxHp;

    public int currentStamina;
    public int maxStamina;

    public Image hpBar;
    public Image staminaBar;

    public CanvasGroup hpCanvas;
    public CanvasGroup staminaCanvas;

    private Coroutine hpCoroutine;
    private Coroutine staminaCoroutine;
    //평타
    //스킬
    //달릴 때
    //

    public void Awake()
    {
        hpCanvas.alpha = 0;
        staminaCanvas.alpha = 0;
    }

    IEnumerator FadeOut(CanvasGroup canvas) //
    {
        canvas.alpha = 1;
        yield return new WaitForSeconds(2f);
        canvas.alpha = 0;
    }

    public void Hit(int damage)
    {
        currentHp -= damage;
        if (currentHp <= 0)
        {
            currentHp = 0;
            return;
        }
        hpBar.fillAmount = ((float)currentHp) / maxHp;
        if (hpCoroutine != null)
        {
            StopCoroutine(hpCoroutine);
        }
        hpCoroutine = StartCoroutine(FadeOut(hpCanvas));
    }

    public bool CanUseStamina()
    {
        return currentStamina > 0; //스태미나가 1이라도 남아있으면
    }

    public void StaminaUse(int stamina) //
    {
        currentStamina -= stamina;
        if (currentStamina <= 0)
        {
            currentStamina = 0;
            staminaBar.fillAmount = ((float)currentStamina) / maxStamina;
            return;
        }
        if (currentStamina >= maxStamina)
        {
            currentStamina = maxStamina;
            staminaBar.fillAmount = ((float)currentStamina) / maxStamina;
            return;
        }
        staminaBar.fillAmount = ((float)currentStamina) / maxStamina;

        //새로
        if (staminaCoroutine != null)
        {
            StopCoroutine(staminaCoroutine);
        }
        staminaCoroutine = StartCoroutine(FadeOut(staminaCanvas));
    }
}