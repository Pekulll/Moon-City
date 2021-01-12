﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Preview : Entity
{
    [Header("Specifics")]
    public int priority = 0;

    private Material correct;
    private Material incorrect;

    [HideInInspector] public bool isEngaged = false;

    private Building building;
    [SerializeField] private Renderer[] previewRenderer;
    private List<Collider> colliders;

    private bool canBuild = true;
    private bool greatRotation = true;
    private bool haveCollider = false;

    #region Initialization

    private void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<MoonManager>();
        previewRenderer = gameObject.GetComponentsInChildren<Renderer>();

        building = manager.buildData[id];

        correct = Resources.Load<Material>("material/preview_correct");
        incorrect = Resources.Load<Material>("material/preview_incorrect");

        colliders = new List<Collider>();
        entityType = EntityType.Preview;

        SuperInitialization();
        UpdateRenderer(incorrect);
        LoadStats();
        UpdateEditor();
        CheckSave();
    }

    private void UpdateEditor()
    {
        entityName = building.name;
        gameObject.name = "[" + side + "] " + entityName;
        transform.SetParent(GameObject.Find("PreviewParent").transform);
    }

    private void CheckSave()
    {
        if (!isEngaged) return;

        if (GetComponent<Rigidbody>() == null)
        {
            gameObject.AddComponent<Rigidbody>();
            GetComponent<Rigidbody>().useGravity = false;

            if (side == manager.side)
            {
                manager.AnticipateRessources(building.energy);
            }
        }

        UpdateRenderer(correct);
    }

    private void LoadStats()
    {
        Building b = manager.buildData[id];

        maxHealth = b.maxHealth;
        weakness = DamageType.Physic;
    }

    #endregion

    private void Update()
    {
        if (isEngaged)
            return;

        if(side != manager.side)
        {
            Place();
            return;
        }

        SetPosition();

        if (Input.GetKey(KeyCode.Tab))
        {
            transform.Rotate(Vector3.up * 10f * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Cancel();
        }

        if (haveCollider)
        {
            UpdateRenderer(incorrect);
            manager.ChangeWarnText("03_ui_top_preview_0");
            return;
        }

        if (!manager.OwnSector(transform.position, side) && !building.canControlSector)
        {
            UpdateRenderer(incorrect);
            manager.ChangeWarnText("03_ui_top_preview_1");
            return;
        }

        if (!greatRotation)
        {
            UpdateRenderer(incorrect);
            manager.ChangeWarnText("03_ui_top_preview_2");
            return;
        }

        if (transform.position.y < building.heightLimit.x || transform.position.y > building.heightLimit.y)
        {
            UpdateRenderer(incorrect);
            manager.ChangeWarnText("03_ui_top_preview_3");
            return;
        }

        List<int> ints = manager.HaveRessources(id);

        if (ints.Count != 0)
        {
            bool isBuildable = false;

            if (ints.Count == 1 && ints.Contains(1)) isBuildable = true;

            if (!isBuildable)
            {
                manager.ChangeWarnText("03_ui_top_preview_4");
                return;
            }
        }

        UpdateRenderer(correct);
        manager.ChangeWarnText("03_ui_top_preview_5", true);

        if (!manager.isOverUI)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                manager.RemoveRessources(0, building.money, building.regolith, building.bioPlastique, building.food);
                manager.AddSettlers(building.colonist, 0);
                manager.HideWarnText();

                if (manager.side == side)
                    manager.canInteractWithUI = true;

                manager.HideWarnText();

                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    if (manager.HaveEnoughResource(building.colonist, building.energy, building.money, building.regolith, building.bioPlastique, building.food))
                    {
                        Instantiate(building.preview, transform.position, transform.rotation);
                        manager.canInteractWithUI = false;
                    }
                }

                Place();
            }
        }
    }

    private void UpdateRenderer(Material mat)
    {
        foreach (Renderer r in previewRenderer)
        {
            int count = r.materials.Length;
            Material[] mats = new Material[count];

            for (int i = 0; i < count; i++)
            {
                mats[i] = mat;
            }

            r.materials = mats;
        }
    }

    private void SetPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int _mask = ~(1 << 10); // c'est comliqué le binaire (not(décalage vers la gauche))

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, _mask))
        {
            Vector3 position = Vector3Int.CeilToInt(hit.point);
            //position.y = manager.GetPointHeightByColliders(transform.position);

            transform.position = position;

            Quaternion qto = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Quaternion vfy = qto;

            qto.x = Mathf.Clamp(qto.x, -0.07f, 0.07f);
            qto.z = Mathf.Clamp(qto.z, -0.07f, 0.07f);

            greatRotation = qto == vfy;
            transform.rotation = qto;

            canBuild = hit.transform.GetComponent<TagIdentifier>() == null;
        }
    }

    private void Place()
    {
        isEngaged = true;
        UpdateRenderer(correct);

        EngageUnits(UnitType.Worker);
        GetComponent<Collider>().isTrigger = false;

        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeAll;

            if (side == manager.side)
            {
                manager.AnticipateRessources(building.energy);
            }
        }
    }

    public void Cancel()
    {
        manager.canInteractWithUI = true;
        manager.DeleteGameObjectOfTagList(gameObject);
        Destroy(gameObject);
    }

    public bool Progress(float amount)
    {
        bool isFinished = ApplyHeal(amount);
        
        if (isFinished)
        {
            DisengageUnits(UnitType.Worker);

            GameObject build = Instantiate(building.building, transform.position, transform.rotation) as GameObject;
            Entity e = build.GetComponent<Entity>();
            e.id = id;
            e.groupID = groupID;
            e.side = side;

            Destroy(gameObject);
        }

        return isFinished;
    }

    #region Check collision

    private void OnTriggerEnter(Collider other)
    {
        colliders.Add(other);
        haveCollider = true;
    }

    private void OnTriggerExit(Collider other)
    {
        colliders.Remove(other);

        if (colliders.Count == 0)
            haveCollider = true;
    }

    #endregion

    private void OnDestroy()
    {
        manager.HideWarnText();
    }
}
