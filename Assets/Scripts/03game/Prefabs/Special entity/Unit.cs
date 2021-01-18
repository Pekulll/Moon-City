using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : Entity
{
    [Header("Specifics")]
    public float damageAmount = 1f;
    public float cooldown = 1f;
    private float range = 10f;
    private DamageType damageType = DamageType.Physic;

    private float speed;

    public int level;
    public float experience;
    public float maxExperience;

    [HideInInspector] public int agressivity;
    [HideInInspector] public int formation;

    [HideInInspector] public int killCount;

    public List<Entity> targets;
    public List<Order> orders;

    private Entity currentTarget;
    private int currentTargetIndex;

    [HideInInspector] public UnitType unitType;

    private bool haveOrder;
    private bool isInRangeOfTarget;

    private EntityType[] targetedEntity;
    private bool isInitiate;

    private void Start()
    {
        Initialize();
    }

    public void Initialize(bool onSave = false)
    {
        if (isInitiate) return;

        isInitiate = true;

        manager = GameObject.Find("Manager").GetComponent<MoonManager>();
        entityType = EntityType.Unit;

        SuperInitialization();
        LoadStats(onSave);
        
        if(unitType == UnitType.Military)
            targetedEntity = new EntityType[3] { EntityType.Building, EntityType.Preview, EntityType.Unit };
        else if(unitType == UnitType.Worker)
            targetedEntity = new EntityType[2] { EntityType.Building, EntityType.Preview };
        else if (unitType == UnitType.Medic)
            targetedEntity = new EntityType[1] { EntityType.Building };
        else if (unitType == UnitType.Security)
            targetedEntity = new EntityType[1] { EntityType.Building };

        ResetTarget();
        UpdateEditor();
        StartCoroutine(Alive());
    }

    private void UpdateEditor()
    {
        entityName = manager.unitData[id].name;
        gameObject.name = "[" + side + "] " + entityName;
        transform.SetParent(GameObject.Find("UnitParent").transform);
    }

    private void LoadStats(bool onSave)
    {
        Units u = manager.unitData[id];

        unitType = u.unitType;

        range = u.range;
        damageType = u.damageType;
        speed = u.speed;

        maxShield = u.shield;
        maxEnergy = u.energy;

        if (level > 1)
        {
            damageAmount = u.levelDamage[level - 2];
            maxHealth = u.levelHealth[level - 2];
        }
        else
        {
            cooldown = u.cooldown;
            damageAmount = u.damage;
            maxHealth = u.health;
        }

        if (!onSave)
        {
            health = maxHealth;
            shield = maxShield;
            energy = maxEnergy;

            agressivity = u.baseAgressivity;

            level = 1;
        }

        maxExperience = Mathf.Pow(10, level);
    }

    private IEnumerator Alive()
    {
        WaitForSeconds inactive = new WaitForSeconds(5f);
        WaitForSeconds delay = new WaitForSeconds(cooldown);
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        //Debug.Log("[INFO:Unit] Update target each: " + 1 / Time.deltaTime + " frames");

        int lastTargetsUpdate = 0;

        while(health > 0)
        {
            CheckForOrder();

            if (haveOrder && orders.Count > 0 && orders[0].orderType == OrderType.Position)
            {
                ExecuteOrder();
                yield return wait;
            }
            else
            {
                if (agressivity != 0)
                {
                    if (lastTargetsUpdate++ >= 1 / Time.deltaTime && !haveOrder && currentTarget == null)
                    {
                        lastTargetsUpdate = 0;
                        FindTargets();
                        SetTargetToNearestTarget(agressivity == 1);
                    }

                    if (!isInRangeOfTarget)
                    {
                        MoveToTarget();
                        yield return wait;
                    }
                    else
                    {
                        yield return delay;
                        Attack();
                    }
                }
                else
                {
                    yield return inactive;
                }
            }
        }
    }

    #region Engage / disengage

    public void Engage(Entity caller)
    {
        if (targets.Contains(caller)) return;

        targets.Add(caller);
    }

    public void Disengage(Entity caller)
    {
        if (!targets.Contains(caller)) return;

        if (currentTarget == caller) RemoveTarget(caller);
        else targets.Remove(caller);
    }

    #endregion

    #region Target

    private void FindTargets()
    {
        Entity[] entities = FindObjectsOfType<Entity>();
        bool searchAlly = unitType != UnitType.Military;

        foreach(Entity e in entities)
        {
            if (e == this) continue;

            foreach(EntityType et in targetedEntity)
            {
                if (e.entityType == et && !targets.Contains(e))
                {
                    if (searchAlly && e.side == side)
                    {
                        if (e.entityType == EntityType.Preview && e.GetComponent<Preview>().isEngaged)
                        {
                            targets.Add(e);
                        }
                    }
                    else if (!searchAlly && e.side != side && manager.GetWarStatut(side, e.side))
                    {
                        targets.Add(e);
                    }

                    break;
                }
            }
        }

        //Debug.Log("[INFO:Unit] Found " + targets.Count + " targets for " + targetedEntity.Length + " targeted entity and a total of " + entities.Length + " entities");
    }

    private void SetTarget(int index)
    {
        if (index > targets.Count) return;

        currentTarget = targets[index];
        currentTargetIndex = index;
    }

    private void SetTarget(Entity entity)
    {
        if (!targets.Contains(entity)) return;
        SetTarget(targets.IndexOf(entity));
    }

    private void RemoveTarget(Entity entity)
    {
        ResetTarget();
        targets.Remove(entity);
    }

    private void ResetTarget()
    {
        currentTarget = null;
        currentTargetIndex = -1;
        isInRangeOfTarget = false;
    }

    private void SetTargetToNearestTarget(bool inRange = true)
    {
        List<int> toRemove = new List<int>();
        float distance = Mathf.Infinity;
        Entity nearest = null;

        for(int i = 0; i < targets.Count; i++)
        {
            Entity current = targets[i];
            float currentDistance = Vector3.Distance(current.transform.position, transform.position);

            if(distance > currentDistance)
            {
                distance = currentDistance;
                nearest = current;
            }
        }

        foreach(int i in toRemove)
        {
            targets.RemoveAt(i);
        }

        if (nearest != null && (distance <= range || !inRange))
            SetTarget(nearest);
    }

    #endregion

    #region Move

    private void MoveToTarget()
    {
        if (currentTarget == null) return;

        transform.LookAt(currentTarget.transform);
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, currentTarget.transform.position) <= range) isInRangeOfTarget = true;
        else isInRangeOfTarget = false;
    }

    private void MoveToPoint(Vector3 point)
    {
        if (point == new Vector3()) return;

        transform.LookAt(point);
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, point) <= .1f)
        {
            orders.RemoveAt(0);
            haveOrder = orders.Count > 0;
        }
    }

    #endregion

    #region Orders

    private void CheckForOrder()
    {
        haveOrder = orders.Count > 0;

        if (haveOrder) ExecuteOrder();
    }

    private void ExecuteOrder()
    {
        if(!haveOrder || orders.Count <= 0)
        {
            haveOrder = false;
            return;
        }

        ResetTarget();

        if (orders[0].orderType == OrderType.Position)
        {
            MoveToPoint(new Vector3 (orders[0].position[0], orders[0].position[1], orders[0].position[2]));
            haveOrder = true;
        }
        else if (orders[0].orderType == OrderType.Entity)
        {
            SetTarget(orders[0].entity);
            haveOrder = true;
        }
    }

    private void RemoveOrder(int index = 0)
    {
        if (orders.Count <= index) return;
        orders.RemoveAt(index);
    }

    public void AddOrder(Order newOrder)
    {
        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            orders.Add(newOrder);
        else
        {
            orders = new List<Order>();
            orders.Add(newOrder);
        }

        CheckForOrder();
    }

    private void Attack()
    {
        if (currentTargetIndex == -1) return;

        bool isDestroyed = true;

        if (unitType == UnitType.Military) isDestroyed = currentTarget.ApplyDamage(damageAmount, damageType);
        else if (unitType == UnitType.Worker)
        {
            if (currentTarget.entityType == EntityType.Building)
            {
                isDestroyed = currentTarget.ApplyHeal(damageAmount);
            }
            else if (currentTarget.entityType == EntityType.Preview)
            {
                isDestroyed = currentTarget.GetComponent<Preview>().Progress(damageAmount);
            }
        }
        else Debug.Log("[INFO:Unit] Not implemented statement.");

        if (isDestroyed)
        {
            ResetTarget();
            if (haveOrder) RemoveOrder();
        }
    }

    #endregion

    #region Others

    public int ChangeAgressivity()
    {
        agressivity = (agressivity + 1) % 3;
        return agressivity;
    }

    public int ChangeFormation()
    {
        formation = (formation + 1) % 3;
        return formation;
    }

    public void AddExp(float exp)
    {
        experience += exp;

        if (experience >= maxExperience)
        {
            if (level < manager.unitData[id].maxLevel)
            {
                level++;

                damageAmount = manager.unitData[id].levelDamage[level - 2];
                health += manager.unitData[id].levelHealth[level - 2] - maxHealth;
                maxHealth = manager.unitData[id].levelHealth[level - 2];

                experience -= maxExperience;
                maxExperience = 10 ^ level;

                Debug.Log("[INFO:Unit] Unit has level up!");
                manager.Notify(10);
            }
            else
            {
                experience = maxExperience;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }

    #endregion
}

[System.Serializable]
public class Order
{
    public OrderType orderType;
    public float[] position;
    public Entity entity;

    public Order(OrderType type, Vector3 pos = new Vector3(), Entity e = null)
    {
        orderType = type;
        position = new float[3] { pos.x, pos.y, pos.z };
        entity = e;
    }
}

[System.Serializable]
public enum OrderType { Position, Entity }