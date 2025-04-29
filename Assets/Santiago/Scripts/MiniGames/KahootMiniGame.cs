using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class KahootMiniGame : MiniGame
{
    [SerializeField]
    private KahootData_SO KahootData;

    [SerializeField]
    private TMP_Text questionTitle;

    private NetworkVariable<int> currentQuestion = new();
    private NetworkVariable<int> correctAnswer = new();

    [SerializeField]
    private List<TMP_Text> posibleAnswers;

    NetworkVariable<bool> startGame = new(false);


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            networkRoundDuration.Value = 10f;
        }
        currentQuestion.OnValueChanged += (oldValue, newValue) =>
        {
            ShowLevel();
        };
        startGame.OnValueChanged += (oldValue, newValue) =>
        {
            EnableGameMap();
        };
    }
    // Update is called once per frame
    void Update()
    {

    }
    
    public void EnableGameMap()
    {
        foreach (var obj in objectsForTheGame)
        {
            obj.gameObject.SetActive(true);
        }
    }
    [ServerRpc]
    public void CurrentLevelServerRpc()
    {
        correctAnswer.Value = KahootData.GetCorrectAnswer(currentQuestion.Value);

    }

    public void ShowLevel()
    {
        questionTitle.text = KahootData.GetQuestionTitle(currentQuestion.Value);
        int j = 0;
        foreach (TMP_Text text in posibleAnswers)
        {
            text.text = KahootData.GetAnswer(currentQuestion.Value, j);
            j++;
        }
    }
    [ServerRpc]
    public void NextQuestionServerRpc()
    {
        currentQuestion.Value++;
    }

    public void RequestNextLevel()
    {
        NextQuestionServerRpc();
    }

    public override async Task OnGame()
    {
        await base.OnGame();
        while (networkRoundDuration.Value < roundTime)
        {
            await Task.Delay(1000);
            networkRoundDuration.Value += 1;
        }
    }
}
