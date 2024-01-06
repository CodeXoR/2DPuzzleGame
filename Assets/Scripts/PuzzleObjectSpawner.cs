using UnityEngine;
using UnityEngine.Pool;

public class PuzzleObjectSpawner : MonoBehaviour
{
    private const int DEFAULT_POOL_CAPACITY = 72;
    private const int MAX_POOL_SIZE = 256;

    [SerializeField] private PuzzleObject _puzzleObjectTemplate;
    [SerializeField] private PuzzleObjectConfig _puzzleObjectFrameConfig;
    private IObjectPool<PuzzleObject> _objectPool;

    public void Initialize()
    {
        _objectPool = new ObjectPool<PuzzleObject>(
            createFunc: CreatePuzzleObject,
            actionOnGet: ActivatePuzzleObject,
            actionOnRelease: DeactivatePuzzleObject,
            actionOnDestroy: DestroyPuzzleObject,
            collectionCheck: true,
            defaultCapacity: DEFAULT_POOL_CAPACITY,
            maxSize: MAX_POOL_SIZE);
    }

    private PuzzleObject CreatePuzzleObject()
    {
        var newObject = Instantiate(_puzzleObjectTemplate);
        return newObject;
    }

    private void ActivatePuzzleObject(PuzzleObject puzzleObject)
    {
        puzzleObject.transform.parent = transform;
        puzzleObject.gameObject.SetActive(true);
    }

    private void DeactivatePuzzleObject(PuzzleObject puzzleObject)
    {
        puzzleObject.gameObject.SetActive(false);
    }

    private void DestroyPuzzleObject(PuzzleObject puzzleObject)
    {
        Destroy(puzzleObject.gameObject);
    }

    public PuzzleObject Get()
    {
        return _objectPool.Get();
    }

    public PuzzleObject GetPuzzleObjectFrame()
    {
        if (_puzzleObjectFrameConfig == null) return default;
        var puzzleObjectFrame = Get();
        puzzleObjectFrame.Initialize(_puzzleObjectFrameConfig);
        puzzleObjectFrame.SendToBack();
        return puzzleObjectFrame;
    }

    public PuzzleObject GetRandomPuzzleObject(PuzzleObjectConfig[] puzzleObjectConfigs)
    {
        if (puzzleObjectConfigs.Length == 0) return default;
        var puzzleObject = Get();
        var puzzleObjectConfig = puzzleObjectConfigs[Random.Range(0, puzzleObjectConfigs.Length)];
        puzzleObject.Initialize(puzzleObjectConfig);
        puzzleObject.BringToFront();
        return puzzleObject;
    }

    public void ReleasePuzzleObject(PuzzleObject puzzleObject)
    {
        _objectPool.Release(puzzleObject);
    }
}