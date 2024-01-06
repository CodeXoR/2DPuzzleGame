using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private Camera _gameCamera;
    [SerializeField] private UIController _uiController;
    [SerializeField] private PuzzleObjectSpawner _puzzleObjectSpawner;
    [SerializeField] private PuzzleConfig[] _puzzleConfigs;
    private Puzzle _activePuzzle;
    private float _activeGameTime;
    private int _activePuzzleLevel;
    private int _activeGameScore;
    private int _inActiveGameScore;
    private bool _gameOver;
    
    private void Start()
    {
        _uiController.StartButtonClicked += StartGame;
        _puzzleObjectSpawner.Initialize();
    }

    private void OnDestroy()
    {
        _uiController.StartButtonClicked -= StartGame;
    }

    private void StartGame()
    {
        ResetGame();
        SetupPuzzle(_puzzleConfigs[_activePuzzleLevel]);
        CenterPuzzleOnScreen();
        UpdateGameTime();
        UpdateGameScore(0);
    }

    private void SetupPuzzle(PuzzleConfig puzzleConfig)
    {
        var puzzleGameObject = new GameObject("Puzzle");
        puzzleGameObject.transform.parent = transform;
        _activePuzzle = puzzleGameObject.AddComponent<Puzzle>();
        _activePuzzle.Construct(_puzzleObjectSpawner, puzzleConfig);
        _activeGameTime += _activePuzzle.TimeToComplete;
        _activePuzzle.MatchesScored += UpdateGameScore;
    }

    private void CenterPuzzleOnScreen()
    {
        var cameraYOffset = 1f;
        var cameraTransform = _gameCamera.transform;
        cameraTransform.position = new Vector3((_activePuzzle.GetWidth() - 1) * .5f, 
            _activePuzzle.GetHeight() * .5f + cameraYOffset, cameraTransform.position.z);
    }

    private void UpdateGameTime()
    {
        _uiController.UpdateGameTimer((int)_activeGameTime);
    }

    private void UpdateGameScore(int numMatches)
    {
        const int scorePerObject = 20;
        _activeGameScore += numMatches * scorePerObject;
        _uiController.UpdateGameScore(_activeGameScore);
        UpdateGameProgress();
    }

    private void UpdateGameProgress()
    {
        if (_activeGameScore - _inActiveGameScore < _activePuzzle.ScoreToComplete) return;
       
        _inActiveGameScore += _activeGameScore;
        _activePuzzle.MatchesScored -= UpdateGameScore;
        _activePuzzle.Hide();
        _activePuzzleLevel++;

        if (_activePuzzleLevel < _puzzleConfigs.Length)
        {
            SetupPuzzle(_puzzleConfigs[_activePuzzleLevel]);
            CenterPuzzleOnScreen();
        }
        else
        {
            _gameOver = true;
            _uiController.ShowGameComplete(levelComplete: true);
        }
    }
    
    private void Update()
    {
        if(_gameOver || _activePuzzle == null) return;

        _activeGameTime -= Time.deltaTime;
        if (_activeGameTime <= 0f)
        {
            _activeGameTime = 0f;
            _gameOver = true;
            _activePuzzle.Hide();
            _uiController.ShowGameComplete(levelComplete: false);
        }
        
        UpdateGameTime();
        
        if (_activePuzzle.IsInteractible() && Input.GetMouseButtonDown(0))
        {
            _activePuzzle.SetSelectedObject(Input.mousePosition, _gameCamera);
        }

        if (_activePuzzle.IsSelectedObjectValid() && Input.GetMouseButtonUp(0))
        {
            _activePuzzle.SwapPuzzleObject(Input.mousePosition, _gameCamera);
        }
    }

    private void ResetGame()
    {
        _activeGameTime = 0f;
        _activePuzzleLevel = 0;
        _activeGameScore = 0;
        _inActiveGameScore = 0;
        _gameOver = false;
        _activePuzzle = null;
    }
}