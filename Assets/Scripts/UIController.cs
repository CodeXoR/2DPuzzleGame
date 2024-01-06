using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UIController : MonoBehaviour
{
    public event Action StartButtonClicked;

    private VisualElement _scoreBar;
    private Button _startButton;
    private Label _gameTimeLabel;
    private Label _gameScoreLabel;
    private VisualElement _gamePrompt;
    private Label _gamePromptText;

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _scoreBar = root.Q<VisualElement>("ScoreBar");
        _startButton = root.Q<Button>("StartButton");
        _startButton.RegisterCallback<ClickEvent>(OnStartButtonClicked);
        _gameTimeLabel = root.Q<Label>("TimeText");
        _gameScoreLabel = root.Q<Label>("ScoreText");
        _gamePrompt = root.Q<VisualElement>("GamePrompt");
        _gamePromptText = root.Q<Label>("PromptText");
        _gamePrompt.style.display = DisplayStyle.None;
    }

    private void OnStartButtonClicked(ClickEvent evt)
    {
        _startButton.style.display = DisplayStyle.None;
        ShowScoreBar();
        StartButtonClicked?.Invoke();
    }

    public void UpdateGameTimer(int gameTime)
    {
        _gameTimeLabel.text = gameTime.ToString();
    }

    public void UpdateGameScore(int gameScore)
    {
        _gameScoreLabel.text = gameScore.ToString();
    }

    public void ShowGameComplete(bool levelComplete)
    {
        HideScoreBar();
        ShowGamePrompt(levelComplete);
        StartCoroutine(Restart());
    }

    private void ShowScoreBar()
    {
        _scoreBar.AddToClassList("score-bar-show");
    }

    private void HideScoreBar()
    {
        _scoreBar.RemoveFromClassList("score-bar-show");
    }

    private void ShowGamePrompt(bool levelComplete)
    {
        _gamePrompt.style.display = DisplayStyle.Flex;
        _gamePrompt.AddToClassList("game-prompt-show");
        _gamePromptText.text = levelComplete ? "Level Complete" : "Game Over";
        _gamePromptText.AddToClassList("prompt-label-show");
    }

    private void HideGamePrompt()
    {
        _gamePrompt.RemoveFromClassList("game-prompt-show");
        _gamePromptText.RemoveFromClassList("prompt-label-show");
    }

    private IEnumerator Restart()
    {
        yield return new WaitForSeconds(2f);
        HideGamePrompt();
        yield return new WaitForSeconds(.5f);
        _gamePrompt.style.display = DisplayStyle.None;
        _startButton.style.display = DisplayStyle.Flex;
    }
}