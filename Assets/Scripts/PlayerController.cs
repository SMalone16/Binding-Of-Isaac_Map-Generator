using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float projectileSpeed = 12f;
    public float projectileLifetime = 1.5f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private CircleCollider2D bodyCollider;
    private Sprite[] frames;
    private int direction; //0-down,1-left,2-right,3-up
    private int animFrame;
    private float animTimer;
    private PlayerProjectileFactory projectileFactory;

    public int CurrentRoomIndex { get; private set; } = -1;

    private static readonly string spriteBase64 = "iVBORw0KGgoAAAANSUhEUgAAAIAAAAAQCAYAAADeWHeIAAAAiElEQVR4nO3VSwqAMAxF0ex/Zd1VxYGD2rQEkb5+7gNx4klD0GhGCCGE+MkpZfxh/katC7+57+FIEfzaXt4AvniuuI/w0wzg3fRo7xipHzL/CO4Vwa/tw0Wa+EdvbADJ/OX/IPUATvfyBvDiF+ApYvVXG8NWr2ELrj/On+N8r5nPwa/tCSE75wL1jq4JKFYPCQAAAABJRU5ErkJggg==";

    public static GameObject Create()
    {
        var go = new GameObject("Player");
        go.AddComponent<PlayerController>();
        return go;
    }

    void Awake()
    {
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        bodyCollider = gameObject.AddComponent<CircleCollider2D>();
        bodyCollider.radius = 0.3f;
        bodyCollider.isTrigger = false;
        sr = gameObject.AddComponent<SpriteRenderer>();
        LoadSprites();
        projectileFactory = new PlayerProjectileFactory();
    }

    void LoadSprites()
    {
        var bytes = Convert.FromBase64String(spriteBase64);
        var tex = new Texture2D(0, 0) { filterMode = FilterMode.Point };
        tex.LoadImage(bytes);

        frames = new Sprite[8];
        int w = 16; int h = 16;
        for (int i = 0; i < 8; i++)
        {
            frames[i] = Sprite.Create(tex, new Rect(i * w, 0, w, h), new Vector2(0.5f, 0.5f), 16);
        }
        sr.sprite = frames[0];
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input = input.normalized;
        rb.velocity = input * speed;

        if (input != Vector2.zero)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                direction = input.x > 0 ? 2 : 1;
            else
                direction = input.y > 0 ? 3 : 0;

            animTimer += Time.deltaTime;
            if (animTimer > 0.2f)
            {
                animFrame = 1 - animFrame;
                animTimer = 0f;
            }
        }
        else
        {
            animFrame = 0;
        }

        sr.sprite = frames[direction * 2 + animFrame];
    }

    private void HandleShooting()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            FireProjectile(Vector2.up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            FireProjectile(Vector2.down);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            FireProjectile(Vector2.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            FireProjectile(Vector2.right);
        }
    }

    private void FireProjectile(Vector2 direction)
    {
        if (direction == Vector2.zero) return;
        var projectile = projectileFactory.Spawn(transform.position, direction.normalized * projectileSpeed, projectileLifetime);
        projectile.IgnoreCollisionWith(gameObject);
    }

    public void SetCurrentRoom(int index)
    {
        CurrentRoomIndex = index;
    }

    private class PlayerProjectileFactory
    {
        private readonly Sprite projectileSprite;

        public PlayerProjectileFactory()
        {
            projectileSprite = LoadProjectileSprite();
        }

        public Projectile Spawn(Vector2 position, Vector2 velocity, float lifetime)
        {
            var projectile = Projectile.Create(projectileSprite);
            projectile.transform.position = position;
            projectile.Initialize(velocity, lifetime);
            return projectile;
        }

        private Sprite LoadProjectileSprite()
        {
            const string data = "iVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAYAAAAfSC3RAAAAP0lEQVR42mP8z8DwnwENgImBRRhGgGE0wWigG4KSRQBpBmYxkGE0QXQyaIphNBwmiDAGk2mAaQYmAAASD0D9Z7T2LgAAAABJRU5ErkJggg==";
            var bytes = Convert.FromBase64String(data);
            var tex = new Texture2D(0, 0) { filterMode = FilterMode.Point };
            tex.LoadImage(bytes);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
        }
    }
}
