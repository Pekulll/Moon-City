using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Buildings))]
public class TurretMotor : MonoBehaviour
{
    [Header("Turret stats")]
    public float damage;
    public float cooldown;
    public float range;
    [SerializeField] private bool perceShield;
    [SerializeField] private DamageType type;

    [Header("Animation part")]
    [SerializeField] private Transform partToRotate;
    [SerializeField] private float turnSpeed = 6.5f;
    [SerializeField] private string fireAnimationName;

    private Animator anim;

    public Transform target;
    private string targetName;

    private bool isInFight = false;

    private MoonManager manager;
    private Buildings health;

    private void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<MoonManager>();
        health = GetComponent<Buildings>();
        anim = GetComponent<Animator>();

        StartCoroutine(UpdateTarget());

        if(health.side != manager.side)
        {
            gameObject.AddComponent<EnemyDefense>();
        }
    }

    private IEnumerator UpdateTarget()
    {
        WaitForSeconds delay1 = new WaitForSeconds(2);
        WaitForSeconds delay2 = new WaitForSeconds(.8f);

        while (true)
        {
            if (isInFight)
            {
                yield return delay1;
            }
            else
            {
                GameObject[] ennemies = FindEnemy();
                float shortestDistance = Mathf.Infinity;
                GameObject nearestEnemy = null;

                if (ennemies.Length == 0)
                {
                    yield return delay1;
                }
                else
                {
                    foreach (GameObject enemy in ennemies)
                    {
                        float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);

                        if (distanceToEnemy < shortestDistance)
                        {
                            shortestDistance = distanceToEnemy;
                            nearestEnemy = enemy;
                        }
                    }

                    if (nearestEnemy != null)
                    {
                        if(this.range >= shortestDistance)
                        {
                            target = nearestEnemy.transform;
                            targetName = target.name;
                        }                
                    }
                    else
                    {
                        target = null;
                        targetName = "";
                    }

                    yield return delay2;
                }
            }
        }
    }

    private GameObject[] FindEnemy()
    {
        List<TagIdentifier> temp = new List<TagIdentifier>();
        temp.AddRange(manager.Find(Tag.Unit));
        temp.AddRange(manager.Find(Tag.Building));
        List<GameObject> gtemp = new List<GameObject>();
        
        foreach (TagIdentifier go in temp)
        {
            Tag tg = go._tags[0];

            if (tg == Tag.Unit)
            {
                int side = go.GetComponent<Unit>().side;

                if (side != health.side)
                {
                    if (manager.GetWarStatut(health.side, side))
                    {
                        gtemp.Add(go.gameObject);
                    }
                }
            }
            else if (tg == Tag.Building)
            {
                int side = go.GetComponent<Buildings>().side;

                if (side != health.side)
                {
                    if (manager.GetWarStatut(health.side, side))
                    {
                        gtemp.Add(go.gameObject);
                    }
                }
            }
        }

        GameObject[] gos = new GameObject[gtemp.Count];

        for(int i = 0; i < gtemp.Count; i++)
        {
            gos[i] = gtemp[i];
        }

        return gos;
    }

    private void Update()
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;

        if(dir != new Vector3())
        {
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
            partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
        }

        if(!isInFight)
        {
            isInFight = true;
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        WaitForSeconds delay = new WaitForSeconds(cooldown);

        while (target != null)
        {
            try 
            { 
                if(target.name != targetName)
                {
                    target = null;
                    targetName = "";
                    isInFight = false;
                }
            }
            catch
            {
                target = null;
                targetName = "";
                isInFight = false;
                break;
            }

            if (target == null) break;

            try
            {
                target.GetComponent<Entity>().ApplyDamage(damage, type);

                if (fireAnimationName != "" && anim != null)
                    anim.Play(fireAnimationName);
            }
            catch
            {
                target = null;
                isInFight = false;
            }

            yield return delay;
        }

        isInFight = false;
        StartCoroutine(UpdateTarget());
    }

    public void SetTarget(Transform target)
    {
        StopCoroutine(Attack());
        isInFight = false;

        this.target = target;
        targetName = this.target.name;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}