using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle : MonoBehaviour
{
    public event Action<int> MatchesScored;
    
    private PuzzleObjectSpawner _spawner;
    private PuzzleObject[,] _puzzleObjectGrid;
    private PuzzleObject _selectedObject;
    private Coroutine _activeSwapRoutine;
    private Vector2Int _selectedGridCoords;
    
    public int TimeToComplete { get; private set; }
    public int ScoreToComplete { get; private set; }

    public void Construct(PuzzleObjectSpawner puzzleObjectSpawner, PuzzleConfig puzzleConfig)
    {
        var gridWidth = puzzleConfig.width;
        var gridHeight = puzzleConfig.height;
        TimeToComplete = puzzleConfig.timeToComplete;
        ScoreToComplete = puzzleConfig.scoreToComplete;
        
        _spawner = puzzleObjectSpawner;
        _puzzleObjectGrid = new PuzzleObject[gridWidth, gridHeight];

        for (var row = 0; row < gridHeight; row++)
        {
            for (var column = 0; column < gridWidth; column++)
            {
                var puzzleObjectPosition = new Vector3(column, row);
                SetupPuzzleObject(_spawner.GetPuzzleObjectFrame(), puzzleObjectPosition);
                var puzzleObject = _spawner.GetRandomPuzzleObject();
                SetupPuzzleObject(puzzleObject, puzzleObjectPosition);
                _puzzleObjectGrid[column, row] = puzzleObject;
            }
        }
    }

    private void OnDisable()
    {
        if (_activeSwapRoutine != null)
        {
            StopCoroutine(_activeSwapRoutine);
        }
        _activeSwapRoutine = null;
    }

    private void SetupPuzzleObject(PuzzleObject puzzleObject, Vector3 puzzleObjectPosition)
    {
        var puzzleObjectTransform = puzzleObject.transform;
        puzzleObjectTransform.parent = transform;
        puzzleObjectTransform.position = puzzleObjectPosition;
    }

    private Vector2Int GetGridCoords(Vector3 inputPos, Camera gameCamera)
    {
        const float centerOffset = 0.5f;
        var worldPos = gameCamera.ScreenToWorldPoint(inputPos);
        var xCoord = Mathf.FloorToInt(worldPos.x + centerOffset);
        var yCoord = Mathf.FloorToInt(worldPos.y + centerOffset);
        return new Vector2Int(xCoord, yCoord);
    }

    private PuzzleObject GetGridObject(Vector2Int gridCoords)
    {
        var xCoord = gridCoords.x;
        var yCoord = gridCoords.y;
        return GetGridObject(xCoord, yCoord);
    }

    private PuzzleObject GetGridObject(int xCoord, int yCoord)
    {
        return AreGridCoordsValid(xCoord, yCoord) ? _puzzleObjectGrid[xCoord, yCoord] : default;
    }

    private void SetGridObject(PuzzleObject gridObject, Vector2 worldPosition)
    {
        var xCoord = (int)worldPosition.x;
        var yCoord = (int)worldPosition.y;
        _puzzleObjectGrid[xCoord, yCoord] = gridObject;
    }

    private bool AreGridCoordsValid(int xCoord, int yCoord)
    {
        if (xCoord < 0 || xCoord >= GetWidth()) return false;
        return yCoord >= 0 && yCoord < GetHeight();
    }

    private Vector2Int GetClampedGridCoords(Vector2Int selectedGridCoords, Vector2Int swapGridCoords)
    {
        var xCoord = selectedGridCoords.x;
        var yCoord = selectedGridCoords.y;
        if (selectedGridCoords.x > swapGridCoords.x) xCoord -= 1;
        if (selectedGridCoords.x < swapGridCoords.x) xCoord += 1;
        if (selectedGridCoords.y > swapGridCoords.y) yCoord -= 1;
        if (selectedGridCoords.y < swapGridCoords.y) yCoord += 1;
        return new Vector2Int(xCoord, yCoord);
    }

    private bool IsSwapValid(PuzzleObject selectedObject, PuzzleObject swapObject)
    {
        if (swapObject == default || swapObject == selectedObject)
        {
            return false;
        }
        var selectedGridCoordsX = selectedObject.GetGridColumn();
        var selectedGridCoordsY = selectedObject.GetGridRow();
        var swapGridCoordsX = swapObject.GetGridColumn();
        var swapGridCoordsY = swapObject.GetGridRow();
        return selectedGridCoordsY == swapGridCoordsY || selectedGridCoordsX == swapGridCoordsX;
    }

    private IEnumerator SwapObjectsPosition(PuzzleObject selectedObject, PuzzleObject swapObject)
    {
        var selectedObjectTransform = selectedObject.transform;
        var swapObjectTransform = swapObject.transform;
        var selectedObjectPos = selectedObjectTransform.position;
        var swapObjectPos = swapObjectTransform.position;
        
        SetGridObject(selectedObject, swapObjectPos);
        SetGridObject(swapObject, selectedObjectPos);
        
        var t = 0f;
        while (t < .5f)
        {
            t += Time.deltaTime;
            selectedObjectTransform.position = Vector3.MoveTowards(selectedObjectTransform.position, swapObjectPos, t);
            swapObjectTransform.position = Vector3.MoveTowards(swapObjectTransform.position, selectedObjectPos, t);
            yield return null;
        }
        
        selectedObjectTransform.position = swapObjectPos;
        swapObjectTransform.position = selectedObjectPos;
        
        _activeSwapRoutine = null;
        _selectedObject = default;

        ScoreMatches();
    }

    private IEnumerator MoveObjectToPosition(PuzzleObject puzzleObject, Vector3 worldPosition)
    {
        var puzzleObjectTransform = puzzleObject.transform;
        var t = 0f;
        while (t < .5f)
        {
            t += Time.deltaTime;
            puzzleObjectTransform.position = Vector3.MoveTowards(puzzleObjectTransform.position, worldPosition, t);
            yield return null;
        }
        puzzleObjectTransform.position = worldPosition;
    }
    
    private List<List<PuzzleObject>> GetHorizontalMatches()
    {
        var gridWidth = GetWidth();
        var gridHeight = GetHeight();
        var matchedObjects = new List<List<PuzzleObject>>();
        var activeObjectList = new List<PuzzleObject>();
        for (var row = 0; row < gridHeight; row++)
        {
            for (var column = 0; column < gridWidth; column++)
            {
                var puzzleObject = _puzzleObjectGrid[column, row];
                var lastObjectCounted = activeObjectList is { Count: > 0 } ? activeObjectList[^1] : null;
                if (lastObjectCounted != null && puzzleObject != null && 
                    lastObjectCounted.GetGridRow() == row && lastObjectCounted.Id == puzzleObject.Id)
                {
                    activeObjectList.Add(puzzleObject);
                }
                else
                {
                    activeObjectList = new List<PuzzleObject>();
                    if(puzzleObject != null) activeObjectList.Add(puzzleObject);
                    matchedObjects.Add(activeObjectList);
                }
            }
        }
        return matchedObjects;
    }
    
    private List<List<PuzzleObject>> GetVerticalMatches()
    {
        var gridWidth = GetWidth();
        var gridHeight = GetHeight();
        var matchedObjects = new List<List<PuzzleObject>>();
        var activeObjectList = new List<PuzzleObject>();
        for (var column = 0; column < gridWidth; column++)
        {
            for (var row = 0; row < gridHeight; row++)
            {
                var puzzleObject = _puzzleObjectGrid[column, row];
                var lastObjectCounted = activeObjectList is { Count: > 0 } ? activeObjectList[^1] : null;
                if (lastObjectCounted != null && puzzleObject != null && 
                    lastObjectCounted.GetGridColumn() == column && lastObjectCounted.Id == puzzleObject.Id)
                {
                    activeObjectList.Add(puzzleObject);
                }
                else
                {
                    activeObjectList = new List<PuzzleObject>();
                    if(puzzleObject != null) activeObjectList.Add(puzzleObject);
                    matchedObjects.Add(activeObjectList);
                }
            }
        }
        return matchedObjects;
    }

    private int VisualizeMatches(List<List<PuzzleObject>> matchedObjects)
    {
        var numMatched = 0;
        foreach (var objectList in matchedObjects)
        {
            if(objectList.Count < 3) continue;
            foreach (var puzzleObject in objectList)
            {
                if (puzzleObject != null && puzzleObject.gameObject.activeInHierarchy)
                {
                    var puzzleObjectPosition = puzzleObject.transform.position;
                    _spawner.ReleasePuzzleObject(puzzleObject);
                    SetGridObject(null, puzzleObjectPosition);
                    numMatched++;
                }
            }
        }
        return numMatched;
    }

    private void ScoreMatches()
    {
        var horizontalMatches = GetHorizontalMatches();
        var verticalMatches = GetVerticalMatches();
        var numMatched = VisualizeMatches(horizontalMatches);
        numMatched += VisualizeMatches(verticalMatches);
        if (numMatched > 0)
        {
            MatchesScored?.Invoke(numMatched);
            if (gameObject.activeInHierarchy)
            {
                _activeSwapRoutine = StartCoroutine(RestockPuzzleGrid());
            }
        }
    }

    private IEnumerator RestockPuzzleGrid()
    {
        var gridWidth = GetWidth();
        var gridHeight = GetHeight();
        for (var column = 0; column < gridWidth; column++)
        {
            var bottomIndex = 0;
            var topIndex = 1;
            while (topIndex < gridHeight)
            {
                var bottomObject = GetGridObject(column, bottomIndex);
                var topObject = GetGridObject(column, topIndex);
                if (topObject == null && bottomObject == null)
                {
                    topIndex++;
                    continue;
                }
                
                if (topObject != null && bottomObject == null)
                {
                    SetGridObject(null, topObject.transform.position);
                    var movePosition = new Vector3(column, bottomIndex);
                    SetGridObject(topObject, movePosition);
                    _activeSwapRoutine = StartCoroutine(MoveObjectToPosition(topObject, movePosition));
                }
                
                bottomIndex++;
                topIndex++;
            }

            while (bottomIndex < gridHeight)
            {
                if (GetGridObject(column, bottomIndex) != null)
                {
                    bottomIndex++;
                    continue;
                }
                var newPuzzleObject = _spawner.GetRandomPuzzleObject();
                var startPosition = new Vector3(column, gridHeight + 1);
                SetupPuzzleObject(newPuzzleObject, startPosition);
                var movePosition = new Vector3(column, bottomIndex);
                SetGridObject(newPuzzleObject, movePosition);
                _activeSwapRoutine = StartCoroutine(MoveObjectToPosition(newPuzzleObject, movePosition));
                bottomIndex++;
            }
        }

        if (_activeSwapRoutine != null)
        {
            yield return new WaitForSeconds(.5f);
        }

        _activeSwapRoutine = null;
        
        ScoreMatches();
    }

    public int GetWidth()
    {
        return _puzzleObjectGrid.GetLength(0);
    }

    public int GetHeight()
    {
        return _puzzleObjectGrid.GetLength(1);
    }
    
    public bool IsInteractible()
    {
        return _activeSwapRoutine == null;
    }

    public bool IsSelectedObjectValid()
    {
        return _selectedObject != default;
    }

    public void SetSelectedObject(Vector3 inputPosition, Camera gameCamera)
    {
        _selectedGridCoords = GetGridCoords(inputPosition, gameCamera);
        _selectedObject = GetGridObject(_selectedGridCoords);
    }

    public void SwapPuzzleObject(Vector3 inputPosition, Camera gameCamera)
    {
        var swapGridCoords = GetGridCoords(inputPosition, gameCamera);
        var clampedGridCoords = GetClampedGridCoords(_selectedGridCoords, swapGridCoords);
        var swapObject = GetGridObject(clampedGridCoords);
        if (IsSwapValid(_selectedObject, swapObject))
        {
            _activeSwapRoutine = StartCoroutine(SwapObjectsPosition(_selectedObject, swapObject));
        }
        else
        {
            _selectedObject = default;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        var gridWidth = GetWidth();
        var gridHeight = GetHeight();
        for (var column = 0; column < gridWidth; column++)
        {
            for (var row = 0; row < gridHeight; row++)
            {
                var puzzleObject = _puzzleObjectGrid[column, row];
                if (puzzleObject != null)
                {
                    _spawner.ReleasePuzzleObject(puzzleObject);
                }
            }
        }
    }
}