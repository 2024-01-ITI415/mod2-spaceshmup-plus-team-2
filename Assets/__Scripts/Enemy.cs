using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    public float speed = 10f;
    public float fireRate = 0.3f;
    public float health = 10;
    public int score = 100;
    public float showDamageDuration = 0.1f;
    public float powerUpDropChance = 1f;

    public Color[] originalColors;
    public Material[] materials;
    public bool showingDamage = false;
    public float damageDoneTime;
    public bool notifiedOfDestruction = false;

    protected BoundsCheck bndCheck;
    public Transform player;

    private void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        materials = Utils.GetAllMaterials(gameObject);
        originalColors = new Color[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            originalColors[i] = materials[i].color;
        }

        player = GameObject.FindGameObjectWithTag("Hero").transform;
    }

    public Vector3 pos
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    void Update()
    {
        MoveTowardsPlayer();

        if (showingDamage && Time.time > damageDoneTime)
        {
            UnShowDamage();
        }

        if (bndCheck != null && bndCheck.offDown)
        {
            Destroy(gameObject);
        }
    }

    public virtual void Move()
    {
        Vector3 tempPos = pos;
        tempPos.y -= speed * Time.deltaTime;
        pos = tempPos;
    }

    void MoveTowardsPlayer()
    {
        if (player != null)
        {
            Vector3 direction = player.position - transform.position;
            direction.Normalize();
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision coll)
    {
        GameObject otherGO = coll.gameObject;
        switch (otherGO.tag)
        {
            case "ProjectileHero":
                Projectile p = otherGO.GetComponent<Projectile>();
                if (!bndCheck.isOnScreen)
                {
                    Destroy(otherGO);
                    break;
                }

                ShowDamage();
                health -= Main.GetWeaponDefinition(p.type).damageOnHit;
                if (health <= 0)
                {
                    if (!notifiedOfDestruction)
                    {
                        Main.S.ShipDestroyed(this);
                    }
                    notifiedOfDestruction = true;
                    Destroy(gameObject);
                }
                Destroy(otherGO);
                break;

            default:
                print("Enemy hit by non-ProjectileHero: " + otherGO.name);
                break;
        }
    }

    private float damageTimer = .5f;
    private float currentDamageTimer;
    private void OnTriggerStay(Collider other)
    {
        GameObject otherGO = other.gameObject;
        if (otherGO.CompareTag("LaserHero"))
        {
            // If this Enemy is off screen, don't damage it.
            if (!bndCheck.isOnScreen)
            {
                currentDamageTimer = damageTimer;
            }

            if (currentDamageTimer <= 0)
            {
                // Hurt this Enemy
                ShowDamage();

                // Get the damage amount from the Main WEAP_DICT
                health -= Main.GetWeaponDefinition(WeaponType.laser).continuousDamage;
                currentDamageTimer = damageTimer;
            }

            currentDamageTimer -= Time.deltaTime;

            if (health <= 0)
            {
                // Tell the Main singleton that this ship was destroyed
                if (!notifiedOfDestruction)
                {
                    Main.S.ShipDestroyed(this);
                }
                notifiedOfDestruction = true;
                // Destroy this enemy
                Destroy(this.gameObject);
            }
        }
    }

    void ShowDamage()
    {
        foreach (Material m in materials)
        {
            m.color = Color.red;
        }
        showingDamage = true;
        damageDoneTime = Time.time + showDamageDuration;
    }

    void UnShowDamage()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].color = originalColors[i];
        }
        showingDamage = false;
    }
}