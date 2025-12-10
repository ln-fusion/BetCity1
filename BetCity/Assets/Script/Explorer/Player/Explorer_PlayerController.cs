using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class PlayerNature
{
    public static int maxSanity ;
    public static int currentSanity;
    public static int maxActionPoints;
    public static int currentActionPoints;
    public static int currentNodeNum;
}
public class Explorer_PlayerController : MonoBehaviour
{
    public GameObject player;
    private RectTransform playertransform;
    private static bool Initial = false;
    private Animator animator;
    [Header("玩家状态")]
    public static int playerstatus = 0;
    //0空闲 1行走 2 丢骰子
    public Explorer_ScreenController screencontroller;
    //move相关
    public float movespeed;
    private void Awake()
    {
        if (!Initial)
        {
            Initial = true;
        }
        playertransform = player.GetComponent<RectTransform>();
        animator = player.GetComponent<Animator>();
    }
    void Start()
    {
        
    }
    public void ToNode(Node currentnode, Node targetnode)
    {
        if(playerstatus==0)
        {
            if (PlayerNature.currentActionPoints>0)
            {
                PlayerNature.currentActionPoints--;
                StartCoroutine(Move(currentnode, targetnode));
            }
            else
            {
                Explorer_ScreenController.CreateMessage("AP点不足");
                return;
            }

        }
        else
        {
            Explorer_ScreenController.CreateMessage("当前无法操作");
        }
    }
    public IEnumerator Move(Node currentnode,Node targetnode)
    {
        PlayerNature.currentNodeNum = targetnode.id;
        playerstatus = 1;
        animator.SetBool("move",true);

        Vector2 movetarget = new Vector2(targetnode.Xposition, targetnode.Yposition) - new Vector2(currentnode.Xposition, currentnode.Yposition);
        Vector2 target = new Vector2(targetnode.Xposition, targetnode.Yposition) + new Vector2(-50, 50);
        Vector2 moveframe = movetarget.normalized;
        float distance = movetarget.magnitude;
        while (distance>10)
        {
            playertransform.anchoredPosition +=moveframe*movespeed*Time.deltaTime;
            distance = Vector2.Distance(playertransform.anchoredPosition, target);
            yield return null;
        }
        playertransform.anchoredPosition = target;
        animator.SetBool("move", false);
        screencontroller.printPlayerNature();
        yield return null;
        playerstatus = 0;
    }
    public void ToNodeInstant(Node targetnode)
    {
        PlayerNature.currentNodeNum = targetnode.id;
        playertransform.anchoredPosition = new Vector2(targetnode.Xposition - 50, targetnode.Yposition + 50);
    }
    public void addap()
    {
        if (PlayerNature.currentActionPoints<PlayerNature.maxActionPoints)
        {
            PlayerNature.currentActionPoints++;
            screencontroller.printPlayerNature();
        }
        else
        {
            Explorer_ScreenController.CreateMessage("AP点已满");
        }
    }
    public void addsan()
    {
        if (PlayerNature.currentSanity < PlayerNature.maxSanity)
        {
            PlayerNature.currentSanity++;
            screencontroller.printPlayerNature();
        }
        else
        {
            Explorer_ScreenController.CreateMessage("理智值已满");
        }
    }


}
