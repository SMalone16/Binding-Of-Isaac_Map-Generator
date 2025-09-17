using UnityEngine;

public class Door : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    private int sourceIndex;
    private int targetIndex;
    private EdgeDirection direction;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col == null)
        {
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
        }
    }

    public void SetDoorSprite(Sprite door)
    {
        spriteRenderer.sprite = door;
    }

    public void SetConnection(int fromIndex, int toIndex)
    {
        sourceIndex = fromIndex;
        targetIndex = toIndex;
    }

    public Vector2 GetSize()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return Vector2.one;

        var bounds = spriteRenderer.sprite.bounds.size;
        var lossy = transform.lossyScale;
        return new Vector2(bounds.x * lossy.x, bounds.y * lossy.y);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        if (!RoomManager.instance.TryGetRoom(targetIndex, out _))
            return;

        var destination = RoomManager.instance.GetRoomCenter(targetIndex);
        var offset = GetEntryOffset();
        player.transform.position = destination + offset;

        var body = player.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.velocity = Vector2.zero;
        }

        if (player.CurrentRoomIndex != sourceIndex)
        {
            player.SetCurrentRoom(sourceIndex);
        }

        player.SetCurrentRoom(targetIndex);
        RoomManager.instance.NotifyPlayerEntered(targetIndex, player);
    }

    public void SetDirection(EdgeDirection dir)
    {
        direction = dir;
    }

    private Vector2 GetEntryOffset()
    {
        const float offsetAmount = 1.25f;
        switch (direction)
        {
            case EdgeDirection.Up:
                return Vector2.down * offsetAmount;
            case EdgeDirection.Down:
                return Vector2.up * offsetAmount;
            case EdgeDirection.Left:
                return Vector2.right * offsetAmount;
            case EdgeDirection.Right:
                return Vector2.left * offsetAmount;
            default:
                return Vector2.zero;
        }
    }
}
