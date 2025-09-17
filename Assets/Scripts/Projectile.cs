using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 velocity;
    private float lifetime;
    private float timeAlive;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private CircleCollider2D hitbox;

    public static Projectile Create(Sprite sprite)
    {
        var go = new GameObject("PlayerProjectile");
        var projectile = go.AddComponent<Projectile>();
        projectile.InitializeGraphics(sprite);
        return projectile;
    }

    private void InitializeGraphics(Sprite sprite)
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 10;

        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        hitbox = gameObject.AddComponent<CircleCollider2D>();
        hitbox.isTrigger = true;
        hitbox.radius = 0.2f;
    }

    public void Initialize(Vector2 newVelocity, float newLifetime)
    {
        velocity = newVelocity;
        lifetime = newLifetime;
        timeAlive = 0f;
    }

    private void Update()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime);
        timeAlive += Time.deltaTime;
        if (timeAlive >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void IgnoreCollisionWith(GameObject owner)
    {
        var ownerColliders = owner.GetComponents<Collider2D>();
        foreach (var ownerCollider in ownerColliders)
        {
            Physics2D.IgnoreCollision(hitbox, ownerCollider, true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
            return;

        var enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakeDamage();
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
