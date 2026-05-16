using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;


[System.Serializable]
public class RobotStats
{
    public string name;
    public int maxHp = 20;
    public int maxEnergy = 5;
    public int curEnergy;
    public int curHp;

    public void Reset()
    {
        curHp = maxHp;
        curEnergy = maxEnergy;
    }
}

public enum ConditionsReact { Always, LowHP, LowEnergy, EnemyAttacks, EnemyBlocks, EnemyShoots }
public enum ConditionsAgr { Always, LowHP, LowEnergy }
public enum Moves { MeleeAttack, RangedAttack, Block, RestoreEnergy }

public class RobotController : MonoBehaviour 
{
    private bool BattleOver = false;
    public RobotStats stats = new RobotStats();
    public RobotController enemy;
    public TMP_Text BattleLog;

    [Header("UI Slider")]
    public UnityEngine.UI.Slider hpSlider;
    public UnityEngine.UI.Slider energySlider;

    [Header("Agressor UI")]
    public TMP_Dropdown agrCond1;
    public TMP_Dropdown agrAct1;
    public TMP_Dropdown agrCond2;
    public TMP_Dropdown agrAct2;
    public TMP_Dropdown agrAct3;

    [Header("Reactor UI")]
    public TMP_Dropdown reactCond1;
    public TMP_Dropdown reactAct1;
    public TMP_Dropdown reactCond2;
    public TMP_Dropdown reactAct2;
    public TMP_Dropdown reactAct3;

    private Animator _animator;

    [Header("Visual Effects")]
    public GameObject shieldObject;


    private IEnumerator ActivateShield()
    {
        if (shieldObject != null)
        {
            shieldObject.SetActive(true);
            yield return new WaitForSeconds(1f);
            shieldObject.SetActive(false);
        }
    }

    private void PlayMoveAnimation(Moves move)
    {
        if (_animator == null)
            return;
        Debug.Log($"<color=cyan>Àíèìàòîð {gameObject.name} ïîëó÷èë êîìàíäó: {move}</color>");
        switch (move)
        {
            case Moves.MeleeAttack:
                _animator.SetTrigger("meleeAttack");
                break;
            case Moves.RangedAttack:
                _animator.SetTrigger("rangeAttack");
                break;
            case Moves.Block:
                _animator.SetTrigger("Defend");
                StartCoroutine(ActivateShield());
                break;
            case Moves.RestoreEnergy:
                StartCoroutine(FlashColor(Color.cyan));
                break;
        }

    }

    private IEnumerator FlashColor(Color curColor)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        UnityEngine.UI.Image[] imgs = GetComponentsInChildren<UnityEngine.UI.Image>();

        foreach (var sr in renderers)
            sr.color = curColor;
        foreach (var img in imgs)
            img.color = curColor;
        yield return new WaitForSeconds(0.15f);
        foreach (var sr in renderers)
            sr.color = Color.white;
        foreach (var img in imgs)
            img.color = Color.white;
    }

    private void TakeDamageAnimation()
    {
        if (_animator != null)
        {
            if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Damaged"))
            {
                _animator.Play("Damaged", -1, 0f);
            }

            StartCoroutine(FlashColor(Color.red));
        }
    }

    public void UpdateUI()  
    {
        if (hpSlider != null)
            hpSlider.value = stats.curHp;
        if (energySlider != null)
            energySlider.value = stats.curEnergy;
    }

    public void FirstEnemyAI()
    {
        agrCond1.value = (int)ConditionsAgr.LowEnergy;
        agrAct1.value = (int)Moves.RestoreEnergy;
        agrCond2.value = (int)ConditionsAgr.LowHP;
        agrAct2.value = (int)Moves.Block;
        agrAct3.value = (int)Moves.MeleeAttack;

        reactCond1.value = (int)ConditionsReact.EnemyAttacks;
        reactAct1.value = (int)Moves.Block;
        reactCond2.value = (int)ConditionsReact.EnemyShoots;
        reactAct2.value = (int)Moves.RangedAttack;
        reactAct3.value = (int)Moves.RestoreEnergy;
    }

    private int getMoveCost(Moves m)
    {
        if (m == Moves.MeleeAttack)
            return 1;
        if (m == Moves.Block)
            return 2;
        if (m == Moves.RangedAttack)
            return 2;
        if (m == Moves.RestoreEnergy)
            return 0;
        return 0; 
    }

    private bool CheckCondition(int index, Moves oppMove) 
    {
        ConditionsReact cnd = (ConditionsReact)index;
        switch (cnd)
        {
            case ConditionsReact.Always:
                return true;
            case ConditionsReact.LowHP:
                return stats.curHp < (stats.maxHp * 0.3f);
            case ConditionsReact.LowEnergy:
                return stats.curEnergy < 2;
            case ConditionsReact.EnemyAttacks:
                return oppMove == Moves.MeleeAttack;
            case ConditionsReact.EnemyBlocks:
                return oppMove == Moves.Block;
            case ConditionsReact.EnemyShoots:
                return oppMove == Moves.RangedAttack;
            default:
                return false;
        }
    }

    private Moves DetermineMove(int c1, int a1, int c2, int a2, int a3, bool isAgr, Moves oppMove = Moves.RestoreEnergy)
    {
        Moves selected;
        if (CheckCondition(c1, oppMove))
            selected = (Moves)a1;
        else if (CheckCondition(c2, oppMove))
            selected = (Moves)a2;
        else
            selected = (Moves)a3;


        
        if(stats.curEnergy < getMoveCost(selected))
        {
            Debug.Log("Not enough energy for move. Executing energy restore");
            return Moves.RestoreEnergy;
        }

        stats.curEnergy -= getMoveCost(selected);
        return selected;

    }

    private void Results(RobotController p, RobotController e, Moves pMove, Moves eMove)
    {

        if (pMove == Moves.RestoreEnergy) 
            p.stats.curEnergy = Mathf.Min(p.stats.maxEnergy, p.stats.curEnergy + 2);
        if (eMove == Moves.RestoreEnergy) 
            e.stats.curEnergy = Mathf.Min(e.stats.maxEnergy, e.stats.curEnergy + 2);

        int damageToEnemy = 0;
        if (pMove == Moves.MeleeAttack) 
            damageToEnemy = (eMove == Moves.Block) ? 1 : 3;
        if (pMove == Moves.RangedAttack) 
            damageToEnemy = (eMove == Moves.Block) ? 0 : 2;

        int damageToPlayer = 0;
        if (eMove == Moves.MeleeAttack) 
            damageToPlayer = (pMove == Moves.Block) ? 1 : 3;
        if (eMove == Moves.RangedAttack) 
            damageToPlayer = (pMove == Moves.Block) ? 0 : 2;

        p.PlayMoveAnimation(pMove);
        if (damageToEnemy > 0)
        {
            e.stats.curHp -= damageToEnemy;
            e.Invoke("TakeDamageAnimation", 0.2f);
        }
        if (damageToPlayer > 0)
        {
            p.stats.curHp -= damageToEnemy;
            p.Invoke("TakeDamageAnimation", 0.2f);
        }

        BattleLog.text = $"{p.stats.name}: {pMove} | {e.stats.name}: {eMove}";
    }


    private void OpenHangar()
    {
        stats.Reset();
        enemy.stats.Reset();
        BattleOver = true;
        UpdateUI();
        enemy.UpdateUI();
        StopAllCoroutines();
        if (panels != null)
            panels.OpenHangar();
    }

    private void CheckKO()
    {
        if(stats.curHp < 0 || enemy.stats.curHp <= 0)
        {
            BattleOver = true;
            string winner = stats.curHp > 0 ? stats.name : enemy.stats.name;
            BattleLog.text = $"Áèòâà îêîí÷åíà! Ïîáåäèë {winner}";
            Invoke("OpenHangar", 3f);
            StopAllCoroutines();
        }
    }

    private IEnumerator ExecuteRound(bool playerIsAgressor)
    {
        Moves pMove, eMove;

        if (playerIsAgressor)
        {
            pMove = DetermineMove(agrCond1.value, agrAct1.value, agrCond2.value, agrAct2.value, agrAct3.value, true);
            eMove = enemy.DetermineMove(enemy.reactCond1.value, enemy.reactAct1.value, enemy.reactCond2.value, enemy.reactAct2.value, enemy.reactAct3.value, false, pMove);
        }
        else
        {
            eMove = enemy.DetermineMove(enemy.agrCond1.value, enemy.agrAct1.value, enemy.agrCond2.value, enemy.agrAct2.value, enemy.agrAct3.value, true);
            pMove = DetermineMove(reactCond1.value, reactAct1.value, reactCond2.value, reactAct2.value, reactAct3.value, false, eMove);
        }

        Results(this, enemy, pMove, eMove);
        UpdateUI();
        enemy.UpdateUI();

        yield return new WaitForSeconds(2.5f); 
        CheckKO();
    }

    public IEnumerator BattleRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        bool playerTurn = true;

        while (!BattleOver)
        {
            yield return StartCoroutine(ExecuteRound(playerTurn));
            playerTurn = !playerTurn; 
        }
    }



    public PanelsController panels; 

    public void SaveAndFight()
    {
        BattleOver = false;
        stats.Reset();
        enemy.stats.Reset();
        UpdateUI();
        enemy.UpdateUI();
        if (panels != null)
            panels.StartBattle();
        StartCoroutine(BattleRoutine());
    }




    private void Start()
    {
        _animator = GetComponentInChildren<Animator>(true);

        if (_animator == null)
        {
            Debug.LogError($"Animator isn't found on object {gameObject.name}");
        }
        stats.Reset();
        UpdateUI();

        if (gameObject.name == "EnemyRobot")
        {
            FirstEnemyAI();
        }
    }

}

