using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroupSystem : MonoBehaviour {
    [Header ("Parameters")]
    [SerializeField] private float doubleClick;
    [SerializeField] private float dragOffset;
    [Header ("Others")]
    [SerializeField] private GameObject[] groupsUI;
    [SerializeField] private Sprite baseIcon;

    public ObjectGroup[] groups;
    [HideInInspector] public ObjectGroup currentGroup;

    private Entity selectedObject;
    private List<Entity> selectedObjects = new List<Entity>();

    private EntityType objectTypeInGroup;
    private bool isDragging;

    private RaycastHit hit;
    private Vector2 mouseDownPosition, mouseUpPosition, currentMousePosition;

    private RectTransform selectArea;
    private MoonManager manager;
    private InformationsViewer viewer;

    private void Start () {
        manager = GetComponent<MoonManager> ();
        viewer = GetComponent<InformationsViewer> ();
        selectArea = GameObject.Find ("I_GroupSelectArea").GetComponent<RectTransform> ();

        groups = new ObjectGroup[10];
        selectArea.gameObject.SetActive (false);
        Invoke ("LoadGroup", .001f);
    }

    void Update () {
        if (Time.timeScale == 0) return;

        DragAndSelectUpdate ();

        if (manager.isOverUI) return;

        DoubleClickUpdate();
        CheckForActionWhenSelected ();

        UniqueSelectionUpdate ();
        MultipleSelectionUpdate ();
        GroupRegisterUpdate ();
    }

    #region Selection (unique and multiple)

    private void UniqueSelectionUpdate () {
        if (selectedObject == null && !Input.GetKey (KeyCode.LeftShift) && !Input.GetKey (KeyCode.RightShift)) {
            if (Input.GetMouseButtonDown (0)) {
                if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit, 1000)) {
                    if (hit.transform.GetComponent<TagIdentifier> () != null) {
                        Entity e = hit.transform.GetComponent<Entity>();
                        doubleClick = 1f;

                        if (e != null) {
                            DeselectEntities ();

                            selectedObject = e;
                            selectedObject.Marker(true);
                            objectTypeInGroup = selectedObject.entityType;

                            IndividualStats (selectedObject, objectTypeInGroup);
                        }
                    } else {
                        DeselectEntities ();
                    }
                }
            }
        } else if (selectedObject != null && !Input.GetKey (KeyCode.LeftShift) && !Input.GetKey (KeyCode.RightShift)) //If the selectedUnit is already assigned
        {
            if (Input.GetMouseButtonDown (0) && (!Input.GetKey (KeyCode.LeftShift) && !Input.GetKey (KeyCode.RightShift))) {
                if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit, 1000)) {
                    if (hit.transform.GetComponent<TagIdentifier> () != null) {
                        DeselectEntities ();
                        UnshowStats ();

                        if (doubleClick != 0 && selectedObject == hit.transform.gameObject) {
                            List<Entity> temp = new List<Entity> (); // Entités du même type
                            List<Entity> onlyGoOfTheSide = new List<Entity> (); // Entités du même side

                            if (objectTypeInGroup == EntityType.Unit)
                            {
                                foreach (GameObject go in manager.FindTag(Tag.Unit))
                                {
                                    Entity e = go.GetComponent<Entity>();

                                    if (e.entityType == selectedObject.entityType)
                                    {
                                        temp.Add(e);
                                    }
                                }
                            }
                            else if (objectTypeInGroup == EntityType.Building)
                            {
                                foreach (GameObject go in manager.FindTag(Tag.Building))
                                {
                                    Entity e = go.GetComponent<Entity>();

                                    if (e.entityType == selectedObject.entityType)
                                    {
                                        temp.Add(e);
                                    }
                                }
                            }
                            else if (objectTypeInGroup == EntityType.Preview)
                            {
                                foreach (GameObject go in manager.FindTag(Tag.Preview))
                                {
                                    Entity e = go.GetComponent<Entity>();

                                    if (e.entityType == selectedObject.entityType)
                                    {
                                        temp.Add(e);
                                    }
                                }
                            }

                            int baseSide = selectedObject.side;

                            foreach (Entity e in temp) {
                                if (e.side == baseSide) {
                                    onlyGoOfTheSide.Add(e);
                                    e.Marker (true);
                                }
                            }

                            if (onlyGoOfTheSide.Count > 1) {
                                selectedObject = null;
                                selectedObjects.AddRange (onlyGoOfTheSide);
                                CreateGroup ();
                            } else if (onlyGoOfTheSide.Count == 1) {
                                selectedObject = onlyGoOfTheSide[0];
                                IndividualStats (selectedObject, objectTypeInGroup);
                            }
                        } else if (hit.transform.GetComponent<Entity>() != null) {
                            selectedObject = hit.transform.GetComponent<Entity>();
                            selectedObject.Marker(true);
                            selectedObjects.Clear();
                            objectTypeInGroup = selectedObject.entityType;
                            IndividualStats (selectedObject, objectTypeInGroup);

                            doubleClick = 1f;
                        } else {
                            if (selectedObject.GetComponent<Entity>() != null) {
                                selectedObject.GetComponent<Entity>().Marker (true);
                            }

                            doubleClick = 1f;
                        }
                    } else {
                        DeselectEntities ();
                        UnshowStats ();
                    }
                }
            }
        }
    }

    private void MultipleSelectionUpdate () {
        if ((selectedObject != null || selectedObjects.Count != 0)
            && Input.GetMouseButtonDown (0)
            && (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)))
        {
            if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit, 1000))
            {
                if (hit.transform.GetComponent<Entity>() != null)
                {
                    if (selectedObject != null && selectedObjects.Count == 0)
                    {
                        if (selectedObject != hit.transform.gameObject)
                        {
                            Entity hittedEntity = hit.transform.GetComponent<Entity>();

                            if(hittedEntity.side == selectedObject.side)
                            {
                                selectedObjects.Add(selectedObject);
                                selectedObjects.Add(hittedEntity);
                                hittedEntity.Marker(true);

                                selectedObject = null;
                                CreateGroup();
                            }
                        }
                        else
                        {
                            DeselectEntities();
                            UnshowStats();
                        }
                    }
                    else if (selectedObject == null && selectedObjects.Count != 0)
                    {
                        Entity hitMtr = hit.transform.GetComponent<Entity>();
                        Entity sldMtr = selectedObjects[0].GetComponent<Entity>();

                        if (hitMtr.side == sldMtr.side && hitMtr.entityType == sldMtr.entityType)
                        {
                            if (!selectedObjects.Contains(hitMtr))
                            {
                                selectedObjects.Add(hitMtr);
                                hitMtr.Marker(true);
                                CreateGroup();
                            }
                            else
                            {
                                selectedObjects.Remove(hitMtr);
                                hitMtr.Marker(false);
                                CreateGroup();
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion

    #region Drag and select

    private void DragAndSelectUpdate () {
        if (Input.GetMouseButtonDown (0)) {
            mouseDownPosition = Input.mousePosition;
        } else if (Input.GetMouseButtonUp (0)) {
            if (isDragging) {
                mouseUpPosition = Input.mousePosition;
                CheckUnitsInArea (mouseDownPosition, mouseUpPosition);
                ResetMousePosition ();
                selectArea.gameObject.SetActive (false);
                isDragging = false;
            }
        }

        if (Input.GetMouseButton (0)) {
            currentMousePosition = Input.mousePosition;

            if ((mouseDownPosition.y >= currentMousePosition.y + dragOffset || mouseDownPosition.y <= currentMousePosition.y - dragOffset) ||
                (mouseDownPosition.x >= currentMousePosition.x + dragOffset || mouseDownPosition.x <= currentMousePosition.x - dragOffset)) {
                isDragging = true;
            } else {
                isDragging = false;
                selectArea.gameObject.SetActive (false);
            }
        }

        if (mouseDownPosition == mouseUpPosition) {
            mouseDownPosition = new Vector2 (-1, -1);
            mouseUpPosition = new Vector2 (-2, -2);
            currentMousePosition = new Vector2 (-3, -3);
            isDragging = false;
        }

        ///Draw the selection in the interface
        if (isDragging) {
            Vector2 pivot = new Vector2 ();
            Vector2 size = new Vector2 ();

            if (mouseDownPosition.y > currentMousePosition.y) {
                pivot.y = 1;
            } else if (mouseDownPosition.y == currentMousePosition.y) {
                pivot.y = 0.5f;
            }

            if (mouseDownPosition.x > currentMousePosition.x) {
                pivot.x = 1;
            } else if (mouseDownPosition.x == currentMousePosition.x) {
                pivot.x = 0.5f;
            }

            size.x = Mathf.Abs (mouseDownPosition.x - currentMousePosition.x);
            size.y = Mathf.Abs (mouseDownPosition.y - currentMousePosition.y);

            selectArea.pivot = pivot;
            selectArea.sizeDelta = size;
            selectArea.position = new Vector3 (mouseDownPosition.x, mouseDownPosition.y, 0);
            selectArea.gameObject.SetActive (true);
        }
    }

    private void CheckUnitsInArea (Vector2 mouseDownPosition, Vector2 mouseUpPosition) {
        if (mouseDownPosition == mouseUpPosition) return;

        float minX, maxX;
        float minY, maxY;

        if (mouseDownPosition.x < mouseUpPosition.x) {
            minX = mouseDownPosition.x;
            maxX = mouseUpPosition.x;
        } else {
            minX = mouseUpPosition.x;
            maxX = mouseDownPosition.x;
        }

        if (mouseDownPosition.y < mouseUpPosition.y) {
            minY = mouseDownPosition.y;
            maxY = mouseUpPosition.y;
        } else {
            minY = mouseUpPosition.y;
            maxY = mouseDownPosition.y;
        }

        ///Debug.Log("  [INFO:GroupSystem] (" + minX + ", " + minY + ")");
        ///Debug.Log("  [INFO:GroupSystem] (" + maxX + ", " + maxY + ")");

        Vector3 minPosition = new Vector3 ();
        Vector3 maxPosition = new Vector3 ();

        Ray rayDown = Camera.main.ScreenPointToRay (new Vector3 (minX, minY));
        Ray rayUp = Camera.main.ScreenPointToRay (new Vector3 (maxX, maxY));

        RaycastHit hit;
        int mask = ~(1 << 11); // c'est compliqué le binaire (NOT(décalage vers la gauche))

        if (Physics.Raycast (rayDown, out hit, Mathf.Infinity, mask)) {
            minPosition = hit.point;
        }

        if (Physics.Raycast (rayUp, out hit, Mathf.Infinity, mask)) {
            maxPosition = hit.point;
        }

        minPosition.y = -10;
        maxPosition.y = 10;

        ///Debug.Log("  [INFO:GroupSystem] " + minPosition + " - " + maxPosition);

        DeselectEntities ();

        List<Entity> unitsInArea = manager.GetUnitsInArea (minPosition, maxPosition);

        if (unitsInArea.Count > 1) {
            ObjectGroup group = new ObjectGroup();

            objectTypeInGroup = EntityType.Unit;

            group.objectsNumber = unitsInArea.Count;
            group.groupSide = manager.side;
            group.groupType = EntityType.Unit;
            group.objectsInGroup = unitsInArea;
            currentGroup = group;

            List<Entity> selectedObj = new List<Entity>();

            foreach (Unit unit in unitsInArea) {
                selectedObj.Add(unit);
                unit.Marker(true);
            }

            selectedObjects = selectedObj;
            manager.ForcedHideStats();
            ShowGroup (currentGroup);
        } else if (unitsInArea.Count == 1) {
            Entity cur = unitsInArea[0];

            selectedObject = cur;
            cur.Marker (true);
            objectTypeInGroup = EntityType.Unit;

            IndividualStats (cur, objectTypeInGroup);
        }
    }

    private void ResetMousePosition () {
        mouseDownPosition = new Vector2 (-1, -1);
        mouseUpPosition = new Vector2 (-2, -2);
        currentMousePosition = new Vector2 (-3, -3);
    }

    #endregion

    #region Group register

    private void GroupRegisterUpdate () {
        if (selectedObjects.Count != 0) {
            if (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl)) {
                if (Input.GetKeyDown (KeyCode.Alpha1)) {
                    RegisterGroup (0);
                } else if (Input.GetKeyDown (KeyCode.Alpha2)) {
                    RegisterGroup (1);
                } else if (Input.GetKeyDown (KeyCode.Alpha3)) {
                    RegisterGroup (2);
                } else if (Input.GetKeyDown (KeyCode.Alpha4)) {
                    RegisterGroup (3);
                } else if (Input.GetKeyDown (KeyCode.Alpha5)) {
                    RegisterGroup (4);
                } else if (Input.GetKeyDown (KeyCode.Alpha6)) {
                    RegisterGroup (5);
                } else if (Input.GetKeyDown (KeyCode.Alpha7)) {
                    RegisterGroup (6);
                } else if (Input.GetKeyDown (KeyCode.Alpha8)) {
                    RegisterGroup (7);
                } else if (Input.GetKeyDown (KeyCode.Alpha9)) {
                    RegisterGroup (8);
                } else if (Input.GetKeyDown (KeyCode.Alpha0)) {
                    RegisterGroup (9);
                }
            }

            if (doubleClick == 0 && (Input.GetKeyDown (KeyCode.Alpha0) || Input.GetKeyDown (KeyCode.Alpha1) || Input.GetKeyDown (KeyCode.Alpha2) || Input.GetKeyDown (KeyCode.Alpha3) || Input.GetKeyDown (KeyCode.Alpha4) || Input.GetKeyDown (KeyCode.Alpha5) || Input.GetKeyDown (KeyCode.Alpha6) || Input.GetKeyDown (KeyCode.Alpha7) || Input.GetKeyDown (KeyCode.Alpha8) || Input.GetKeyDown (KeyCode.Alpha9))) {
                doubleClick = 1;
            } else if (doubleClick != 0) {
                if (Input.GetKeyDown (KeyCode.Alpha1)) {
                    manager.TeleportPlayer (selectedObjects[0].transform.position + new Vector3 (0, 15, -10));
                } else if (Input.GetKeyDown (KeyCode.Alpha2)) {
                    manager.TeleportPlayer (selectedObjects[0].transform.position + new Vector3 (0, 15, -10));
                } else if (Input.GetKeyDown (KeyCode.Alpha3)) {
                    manager.TeleportPlayer (selectedObjects[0].transform.position + new Vector3 (0, 15, -10));
                } else if (Input.GetKeyDown (KeyCode.Alpha4)) {
                    manager.TeleportPlayer (selectedObjects[0].transform.position + new Vector3 (0, 15, -10));
                } else if (Input.GetKeyDown (KeyCode.Alpha5)) {
                    manager.TeleportPlayer (selectedObjects[0].transform.position + new Vector3 (0, 15, -10));
                } else if (Input.GetKeyDown (KeyCode.Alpha6)) {
                    manager.TeleportPlayer (selectedObjects[0].transform.position + new Vector3 (0, 15, -10));
                } else if (Input.GetKeyDown (KeyCode.Alpha7)) {
                    manager.TeleportPlayer (selectedObjects[0].transform.position + new Vector3 (0, 15, -10));
                } else if (Input.GetKeyDown (KeyCode.Alpha8)) {
                    manager.TeleportPlayer (selectedObjects[0].transform.position + new Vector3 (0, 15, -10));
                } else if (Input.GetKeyDown (KeyCode.Alpha9)) {
                    manager.TeleportPlayer (selectedObjects[0].transform.position + new Vector3 (0, 15, -10));
                } else if (Input.GetKeyDown (KeyCode.Alpha0)) {
                    manager.TeleportPlayer (selectedObjects[0].transform.position + new Vector3 (0, 15, -10));
                }
            }
        }

        if (!Input.GetKey (KeyCode.LeftControl) && !Input.GetKey (KeyCode.RightControl)) {
            if (Input.GetKeyDown (KeyCode.Alpha1)) {
                SelectRegisteredGroup (0);
            } else if (Input.GetKeyDown (KeyCode.Alpha2)) {
                SelectRegisteredGroup (1);
            } else if (Input.GetKeyDown (KeyCode.Alpha3)) {
                SelectRegisteredGroup (2);
            } else if (Input.GetKeyDown (KeyCode.Alpha4)) {
                SelectRegisteredGroup (3);
            } else if (Input.GetKeyDown (KeyCode.Alpha5)) {
                SelectRegisteredGroup (4);
            } else if (Input.GetKeyDown (KeyCode.Alpha6)) {
                SelectRegisteredGroup (5);
            } else if (Input.GetKeyDown (KeyCode.Alpha7)) {
                SelectRegisteredGroup (6);
            } else if (Input.GetKeyDown (KeyCode.Alpha8)) {
                SelectRegisteredGroup (7);
            } else if (Input.GetKeyDown (KeyCode.Alpha9)) {
                SelectRegisteredGroup (8);
            } else if (Input.GetKeyDown (KeyCode.Alpha0)) {
                SelectRegisteredGroup (9);
            }
        }
    }

    private void RegisterGroup (int groupID) {
        ObjectGroup currentGroup = NewGroup (objectTypeInGroup);

        foreach (Entity e in currentGroup.objectsInGroup)
        {
            e.groupID = groupID;
        }

        groups[groupID] = currentGroup;
        UpdateGroupBar (groupID);

        Debug.Log ("  [INFO:GroupSystem] Group created succesfuly!");
    }

    public void SelectRegisteredGroup (int id) {
        SelectGroup (groups[id]);
    }

    private void LoadGroup () {
        Entity[] entities = FindObjectsOfType<Entity> ();
        List<Entity> playerEntities = new List<Entity> ();

        foreach (Entity e in entities) {
            if (e.side == manager.side) {
                playerEntities.Add (e);
            }
        }

        if (playerEntities.Count == 0) return;

        ObjectGroup[] savedGroups = new ObjectGroup[10];

        foreach (Entity e in playerEntities) {
            //? Debug.Log ("  [INFO:GroupSystem] Determine if this entity have a group...");
            if (e.groupID != -1) {
                int groupId = e.groupID;
                ObjectGroup currentGroup = new ObjectGroup ();

                if (savedGroups[groupId] != null)
                    currentGroup = savedGroups[groupId];

                currentGroup.groupSide = e.side;
                currentGroup.groupType = e.entityType;
                currentGroup.objectsNumber++;
                currentGroup.objectsInGroup.Add(e);

                savedGroups[groupId] = currentGroup;
            }
        }

        groups = savedGroups;

        for (int i = 0; i < groups.Length; i++) {
            if (groups[i] != null)
                UpdateGroupBar (i);
        }
    }

    #endregion

    #region Group creation

    public void ShowGroup (ObjectGroup group) {
        if (group.objectsNumber == 0) return;

        group.groupSide = group.objectsInGroup[0].side;
        manager.GroupStats (group);
    }

    public void CreateGroup () {
        currentGroup = NewGroup(objectTypeInGroup);

        if (currentGroup == null) return;

        manager.ForcedHideStats ();
        ShowGroup (currentGroup);
    }

    private ObjectGroup NewGroup (EntityType type) {
        if (selectedObjects.Count == 0) return null;

        ObjectGroup group = new ObjectGroup();

        group.objectsNumber = selectedObjects.Count;
        group.groupSide = manager.side;
        group.groupType = objectTypeInGroup;

        foreach (Entity cur in selectedObjects)
        {
            if (cur.entityType == group.groupType && group.groupType == EntityType.Unit)
                group.objectsInGroup.Add(cur);
        }

        return group;
    }

    #endregion

    #region Others

    private void IndividualStats (Entity _gameObject, EntityType _objectTypeInGroup) {
        if (objectTypeInGroup == EntityType.Preview) {
            if (!_gameObject.GetComponent<Preview>().isEngaged) {
                _gameObject.GetComponent<Entity>().Marker (false);
                return;
            }
        }
        manager.IndividualStats (_gameObject, _objectTypeInGroup);
    }

    private void UpdateGroupBar (int id) {
        Image img = groupsUI[id].transform.Find("Image").GetComponent<Image> ();
        Text txt = groupsUI[id].transform.Find("Text").GetComponent<Text> ();
        ObjectGroup cur = groups[id];

        if (cur.groupType == EntityType.Unit) {
            img.sprite = manager.unitData[cur.objectsInGroup[0].id].unitIcon;
            txt.text = cur.objectsInGroup.Count.ToString("00");
        } else {
            img.sprite = manager.buildData[cur.objectsInGroup[0].id].icon;
            txt.text = cur.objectsInGroup.Count.ToString("00");
        }
    }

    private void UnshowStats () {
        manager.ForcedHideStats ();
        currentGroup = null;
    }

    public void DeselectEntities () {
        if (selectedObject != null) {
            selectedObject.Marker(false);
        }

        if (selectedObjects.Count != 0) {
            for (int i = 0; i < selectedObjects.Count; i++) {
                selectedObjects[i].Marker(false);
            }

            selectedObjects.Clear ();
        }

        UnshowStats ();
    }

    private int GetSide (Entity obj) {
        return obj.side;
    }

    private void SelectGroup (ObjectGroup group) {
        if (group == null) return;

        DeselectEntities ();

        if (group.groupType == EntityType.Unit) {
            foreach (Unit u in group.objectsInGroup) {
                selectedObjects.Add(u);
                u.Marker(true);
            }
        } else if (group.groupType == EntityType.Building) {
            foreach (Buildings b in group.objectsInGroup) {
                selectedObjects.Add(b);
                b.Marker(true);
            }
        } else if (group.groupType == EntityType.Preview) {
            foreach (Preview p in group.objectsInGroup) {
                selectedObjects.Add(p);
                p.Marker(true);
            }
        }

        objectTypeInGroup = group.groupType;
        currentGroup = group;
        ShowGroup (group);
    }

    private void CheckForActionWhenSelected ()
    {
        if (Input.GetMouseButtonDown (1))
        {
            TryToSendAttackOrder();
            TryToSetRallyPoint();
        }
    }

    private void DoubleClickUpdate () {
        if (doubleClick > 0) doubleClick -= 2.2f * Time.deltaTime;
        else if (doubleClick < 0) doubleClick = 0;
    }

    private void TryToSendAttackOrder()
    {
        if((selectedObject != null && selectedObject.entityType != EntityType.Preview) ||
           (selectedObjects != null && selectedObjects.Count != 0 && selectedObjects[0].entityType != EntityType.Preview))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000))
            {
                if (hit.transform.gameObject.GetComponent<TagIdentifier>() != null)
                {
                    manager.SendAttackOrder(hit.transform);
                }
                else
                {
                    manager.UnitOrder(hit.point);
                }
            }
        }
    }

    private void TryToSetRallyPoint()
    {
        if ((selectedObject != null && selectedObject.entityType == EntityType.Building) ||
           (selectedObjects != null && selectedObjects.Count != 0 && selectedObjects[0].entityType == EntityType.Building))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000))
            {
                if (hit.transform.gameObject.GetComponent<TagIdentifier>() != null)
                {
                    manager.Notify(manager.Traduce("03_notif_rallypoint"), duration: 1f);
                }
                else
                {
                    viewer.SetRallyPoint(hit.point);
                }
            }
        }
    }

    public void WhenEntityIsDestroyed(Entity mtr)
    {
        if (selectedObject != null && mtr.gameObject == selectedObject)
        {
            UnshowStats();
        }
        else if (currentGroup != null)
        {
            if (currentGroup.groupType == mtr.entityType)
            {
                currentGroup.objectsInGroup.Remove(mtr);
                ShowGroup(currentGroup);
            }
        }
    }

    #endregion
}