using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleButtonHandler : MonoBehaviour
{
    [SerializeField] private TurnHandler handler;

    [SerializeField] private Button actionOneButton;
    [SerializeField] private Button actionTwoButton;
    [SerializeField] private Button actionThreeButton;

    [SerializeField] private Button actorOneButton;
    [SerializeField] private Button actorTwoButton;
    [SerializeField] private Button actorThreeButton;

    [SerializeField] private Button targetOneButton;
    [SerializeField] private Button targetTwoButton;
    [SerializeField] private Button targetThreeButton;

    [SerializeField] private Button executeButton;

    [SerializeField] private Button endTurnButton;

    [SerializeField] private TextMeshProUGUI targetText;

    public GameObject tooptipPanel;

    private string[] actorText;
    private string[] enemyText;

    private int action;
    private bool targetIsTeam;

    private bool[] pcAliveArray;
    private bool[] targetAliveArray;

    public void DisableUI()
    {
        actorOneButton.interactable = false;
        actorTwoButton.interactable = false;
        actorThreeButton.interactable = false;

        targetOneButton.interactable = false;
        targetTwoButton.interactable = false;
        targetThreeButton.interactable = false;

        actionOneButton.interactable = false;
        actionTwoButton.interactable = false;
        actionThreeButton.interactable = false;

        executeButton.interactable = false;

        endTurnButton.interactable = false;
    }

    public void RefreshUI()
    {
        ActorButtonUpdate(pcAliveArray);
        ActionButtonUpdate();
        TargetButtonUpdate(targetAliveArray);

        executeButton.interactable = true;

        endTurnButton.interactable = true;
    }

    public void ActionButtonPressed(int i)
    {
        action = i;
        UpdateTargetSide();

        switch (i)
        {
            case (1):
                actionOneButton.image.color = Color.green;
                actionTwoButton.image.color = Color.white;
                actionThreeButton.image.color = Color.white;
                break;
            case (2):
                actionOneButton.image.color = Color.white;
                actionTwoButton.image.color = Color.green;
                actionThreeButton.image.color = Color.white;
                break;
            case (3):
                actionOneButton.image.color = Color.white;
                actionTwoButton.image.color = Color.white;
                actionThreeButton.image.color = Color.green;
                break;
            default:
                break;
        }
    }

    public void UpdateTargetSide()
    {
        targetIsTeam = handler.HandleAttack(action);

        if (targetIsTeam)
        {
            targetText.text = "Allies";

            targetOneButton.GetComponentInChildren<TextMeshProUGUI>().text = actorText[0];
            targetTwoButton.GetComponentInChildren<TextMeshProUGUI>().text = actorText[1];
            targetThreeButton.GetComponentInChildren<TextMeshProUGUI>().text = actorText[2];
        }
        else
        {
            targetText.text = "Enemies";

            targetOneButton.GetComponentInChildren<TextMeshProUGUI>().text = enemyText[0];
            targetTwoButton.GetComponentInChildren<TextMeshProUGUI>().text = enemyText[1];
            targetThreeButton.GetComponentInChildren<TextMeshProUGUI>().text = enemyText[2];
        }

        TargetButtonUpdate(targetAliveArray);
    }

    public void ActorButtonPressed(int i)
    {
        handler.HandleActorBotSelection(i);
        ActionButtonUpdate();
        UpdateTargetSide();

        switch (i)
        {
            case (0):
                actorOneButton.image.color = Color.green;
                actorTwoButton.image.color = Color.white;
                actorThreeButton.image.color = Color.white;
                break;
            case (1):
                actorOneButton.image.color = Color.white;
                actorTwoButton.image.color = Color.green;
                actorThreeButton.image.color = Color.white;
                break;
            case (2):
                actorOneButton.image.color = Color.white;
                actorTwoButton.image.color = Color.white;
                actorThreeButton.image.color = Color.green;
                break;
            default:
                break;
        }
    }

    public void EnemyButtonPressed(int i)
    {
        handler.HandleTargetBotSelection(i);

        switch (i)
        {
            case (0):
                targetOneButton.image.color = Color.green;
                targetTwoButton.image.color = Color.white;
                targetThreeButton.image.color = Color.white;
                break;
            case (1):
                targetOneButton.image.color = Color.white;
                targetTwoButton.image.color = Color.green;
                targetThreeButton.image.color = Color.white;
                break;
            case (2):
                targetOneButton.image.color = Color.white;
                targetTwoButton.image.color = Color.white;
                targetThreeButton.image.color = Color.green;
                break;
            default:
                break;
        }
    }

    public void ExecuteButtonPressed()
    {
        handler.ExecuteAction();
    }

    public void EndTurnButtonPressed()
    {
        handler.HandleEndPlayerTurn();
    }

    public void WinButtonPressed()
    {
        handler.HandleEndRound(false);
    }

    public void LoseButtonPressed()
    {
        handler.HandleEndRound(true);
    }

    public void InitializeButtons()
    {
        actorText = handler.UpdateActorButtons();
        enemyText = handler.UpdateEnemyButtons();

        pcAliveArray = new bool[] { true, true, true };
        targetAliveArray = new bool[] { true, true, true };

        actorOneButton.GetComponentInChildren<TextMeshProUGUI>().text = actorText[0];
        actorTwoButton.GetComponentInChildren<TextMeshProUGUI>().text = actorText[1];
        actorThreeButton.GetComponentInChildren<TextMeshProUGUI>().text = actorText[2];

        targetOneButton.GetComponentInChildren<TextMeshProUGUI>().text = enemyText[0];
        targetTwoButton.GetComponentInChildren<TextMeshProUGUI>().text = enemyText[1];
        targetThreeButton.GetComponentInChildren<TextMeshProUGUI>().text = enemyText[2];

        ActionButtonUpdate();
    }

    public void ActionButtonUpdate()
    {
        string[] actionText = handler.UpdateActionButtons();
        bool[] en = handler.UpdateActionButtonEnablement();

        actionOneButton.GetComponentInChildren<TextMeshProUGUI>().text = actionText[0];
        actionTwoButton.GetComponentInChildren<TextMeshProUGUI>().text = actionText[1];
        actionThreeButton.GetComponentInChildren<TextMeshProUGUI>().text = actionText[2];

        actionOneButton.interactable = en[0];
        actionTwoButton.interactable = en[1];
        actionThreeButton.interactable = en[2];
    }

    public void ActorButtonUpdate(bool[] en)
    {
        pcAliveArray = en;

        actorOneButton.interactable = en[0];
        actorTwoButton.interactable = en[1];
        actorThreeButton.interactable = en[2];
    }

    public void TargetButtonUpdate(bool[] en)
    {
        targetAliveArray = en;

        targetOneButton.interactable = en[0];
        targetTwoButton.interactable = en[1];
        targetThreeButton.interactable = en[2];
    }

    public void OpenTooltip()
    {
        tooptipPanel.SetActive(true);
    }

    public void CloseTooltip()
    {
        tooptipPanel.SetActive(false);
    }
}
