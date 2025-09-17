using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;

    private Transform player;
    private Rect movementBounds;
    private bool active;
    private SpriteRenderer spriteRenderer;
    private static Sprite cachedSprite;

    public static EnemyController Create()
    {
        var go = new GameObject("Enemy");
        var enemy = go.AddComponent<EnemyController>();
        enemy.InitializeGraphics();
        return enemy;
    }

    private void InitializeGraphics()
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetEnemySprite();
        spriteRenderer.sortingOrder = 5;

        var collider = gameObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.35f;
    }

    private Sprite GetEnemySprite()
    {
        if (cachedSprite != null)
            return cachedSprite;

        const string data = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAABpklEQVR42mNkAAKGuP///7+18A1mBjaz8P/PYprPgNZAFgcSgwgXQC+BwR6G/6PjP9T+y+4/ysb43+ZGJgY/v//D0P/f5gZGJgYgLEgB2D+H2T4b0v3n2DG9Z/DXxgYGAaA8SAXYP4/5PhvRPfP4Ib1n8NfGBgYBoDxIBdg/j+E+F9L9z9gxvWfw18YGBgGgPEgF2D+P8T4X0v3P2DG9Z/DXxgYGAaA8SAXYP4/xPjfG3n2DG9Z/DXxgYGAaA8SAXYP4/wvxfSfefYMZ1n8NfGBgYBoDxIBdg/j/C/F9J95/gxvWfw18YGNgYBDL+Z4b8x+a8f2PjP5/5BhYGNjAwMt/P2D2f2T4n4z/f+QYGBgY2MDAn38/YPe/0+F+M/3/kGBgYGNjAwJ9/P2H3v1PhfjP9/5BgYGBjYwMCffz9g979T4X4z/f+QYGBrY9B+1/28w8r+b8f8PhvofsPxPxvxnw+E+h+w/E/G/GfD4T6H7D8T8b8Z8PhPofEPMAAAcYB2t57lGCNAAAAAElFTkSuQmCC";
        var bytes = Convert.FromBase64String(data);
        var tex = new Texture2D(0, 0) { filterMode = FilterMode.Point };
        tex.LoadImage(bytes);
        cachedSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
        return cachedSprite;
    }

    public void Configure(Transform playerTarget, Rect bounds)
    {
        player = playerTarget;
        movementBounds = bounds;
        active = true;
    }

    private void Update()
    {
        if (!active || player == null)
            return;

        var toPlayer = (Vector2)(player.position - transform.position);
        if (toPlayer.sqrMagnitude > 0.0001f)
        {
            var direction = toPlayer.normalized;
            transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
        }

        var position = transform.position;
        position.x = Mathf.Clamp(position.x, movementBounds.xMin, movementBounds.xMax);
        position.y = Mathf.Clamp(position.y, movementBounds.yMin, movementBounds.yMax);
        transform.position = position;
    }

    public void SetActive(bool shouldBeActive)
    {
        active = shouldBeActive;
    }

    public void TakeDamage()
    {
        Destroy(gameObject);
    }
}
