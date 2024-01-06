using UnityEngine;

public class PuzzleObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    public int Id { get; private set; }
        
    public void Initialize(PuzzleObjectConfig config)
    {
        Id = config.id;
        _spriteRenderer.sprite = config.imageSprite;
    }

    public void SendToBack()
    {
        _spriteRenderer.sortingOrder = 0;
    }

    public void BringToFront()
    {
        _spriteRenderer.sortingOrder = 1;
    }

    public int GetGridRow()
    {
        return (int)transform.position.y;
    }

    public int GetGridColumn()
    {
        return (int)transform.position.x;
    }
    
    public override string ToString()
    {
        return $"{Id} - {_spriteRenderer.sprite.name}";
    }
}