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

    [SerializeField] private Button enemyOneButton;
    [SerializeField] private Button enemyTwoButton;
    [SerializeField] private Button enemyThreeButton;

    [SerializeField] private Button executeButton;

    [SerializeField] private TextMeshProUGUI targetText;

    public GameObject tooptipPanel;

    private string[] actorText;
    private string[] enemyText;

    private bool targetIsTeam;

    private bool[] pcAliveArray;
    private bool[] aiAliveArray;

    public void ActionButtonPressed(int i)
    {
        targetIsTeam = handler.HandleAttack(i);

        if (targetIsTeam)
        {
            targetText.text = "Allies";

            enemyOneButton.GetComponentInChildren<TextMeshProUGUI>().text = actorText[0];
            enemyTwoButton.GetComponentInChildren<TextMeshProUGUI>().text = actorText[1];
            enemyThreeButton.GetComponentInChildren<TextMeshProUGUI>().text = actorText[2];

            enemyOneButton.interactable = pcAliveArray[0];
            enemyTwoButton.interactable = pcAliveArray[1];
            enemyThreeButton.interactable = pcAliveArray[2];
        }
        else
        {
            targetText.text = "Enemies";

            enemyOneButton.GetComponentInChildren<TextMeshProUGUI>().text = enemyText[0];
            enemyTwoButton.GetComponentInChildren<TextMeshProUGUI>().text = enemyText[1];
            enemyThreeButton.GetComponentInChildren<TextMeshProUGUI>().text = enemyText[2];

            enemyOneButton.interactable = aiAliveArray[0];
            enemyTwoButton.interactable = aiAliveArray[1];
            enemyThreeButton.interactable = aiAliveArray[2];
        }

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

    public void ActorButtonPressed(int i)
    {
        handler.HandleActorBotSelection(i);
        ActionButtonUpdate();

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
        handler.HandleEnemyBotSelection(i);

        switch (i)
        {
            case (0):
                enemyOneButton.image.color = Color.green;
                enemyTwoButton.image.color = Color.white;
                enemyThreeButton.image.color = Color.white;
                break;
            case (1):
                enemyOneButton.image.color = Color.white;
                enemyTwoButton.image.color = Color.green;
                enemyThreeButton.image.color = Color.white;
                break;
            case (2):
                enemyOneButton.image.color = Color.white;
                enemyTwoButton.image.color = Color.white;
                enemyThreeButton.image.color = Color.green;
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
        aiAliveArray = new bool[] { true, true, true };

        actorOneButton.GetComponentInChildren<TextMeshProUGUI>().text = actorText[0];
        actorTwoButton.GetComponentInChildren<TextMeshProUGUI>().text = actorText[1];
        actorThreeButton.GetComponentInChildren<TextMeshProUGUI>().text = actorText[2];

        enemyOneButton.GetComponentInChildren<TextMeshProUGUI>().text = enemyText[0];
        enemyTwoButton.GetComponentInChildren<TextMeshProUGUI>().text = enemyText[1];
        enemyThreeButton.GetComponentInChildren<TextMeshProUGUI>().text = enemyText[2];

        ActionButtonUpdate();
    }

    public void ActionButtonUpdate()
    {
        string[] actionText = handler.UpdateActionButtons();
        bool[] en = handler.UpdateActionButtonEnablement();

        targetText.text = "Enemies";

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

        if (targetIsTeam)
        {
            enemyOneButton.interactable = en[0];
            enemyTwoButton.interactable = en[1];
            enemyThreeButton.interactable = en[2];
        }
    }

    public void EnemyButtonUpdate(bool[] en)
    {
        aiAliveArray = en;

        if (!targetIsTeam)
        {
            enemyOneButton.interactable = en[0];
            enemyTwoButton.interactable = en[1];
            enemyThreeButton.interactable = en[2];
        }
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
