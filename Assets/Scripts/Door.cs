using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite closedDoorSprite;

    private EdgeDirection direction;
    private BoxCollider2D boxCollider;
    private Sprite openDoorSprite;

    public bool isLocked { get; private set; }

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        boxCollider.isTrigger = true;
    }

    public void SetDoorSprite(Sprite door)
    {
        openDoorSprite = door;

        if (!isLocked)
        {
            spriteRenderer.sprite = door;
        }
    }

    public void SetDirection(EdgeDirection dir)
    {
        direction = dir;
    }

    public void Lock()
    {
        if (isLocked) return;

        isLocked = true;

        if (closedDoorSprite != null)
        {
            spriteRenderer.sprite = closedDoorSprite;
        }

        if (boxCollider != null)
        {
            boxCollider.isTrigger = false;
        }
    }

    public void Unlock()
    {
        if (!isLocked) return;

        isLocked = false;

        if (openDoorSprite != null)
        {
            spriteRenderer.sprite = openDoorSprite;
        }

        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLocked) return;

        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        Vector2 offset = Vector2.zero;
        Vector2 push = Vector2.zero;
        switch (direction)
        {
            case EdgeDirection.Up:
                offset = new Vector2(0, RoomManager.instance.offsetY);
                push = Vector2.up;
                break;
            case EdgeDirection.Down:
                offset = new Vector2(0, -RoomManager.instance.offsetY);
                push = Vector2.down;
                break;
            case EdgeDirection.Left:
                offset = new Vector2(-RoomManager.instance.offsetX, 0);
                push = Vector2.left;
                break;
            case EdgeDirection.Right:
                offset = new Vector2(RoomManager.instance.offsetX, 0);
                push = Vector2.right;
                break;
        }
        player.transform.position += (Vector3)(offset + push);
    }
}
