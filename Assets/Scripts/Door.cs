using UnityEngine;

public class Door : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
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

    public void SetDirection(EdgeDirection dir)
    {
        direction = dir;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
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
