using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SectorManager : MonoBehaviour
{
    [SerializeField] private List<Sector> sectors = new List<Sector>();
    [SerializeField] private Image sectorItem;

    private MoonManager manager;
    private MapGenerator mapGenerator;
    private ColorManager colorManager;

    private GameObject sectorMenu;

    private List<GameObject> items;

    private RectTransform content;

    private void Start()
    {
        manager = GetComponent<MoonManager>();
        mapGenerator = GetComponent<MapGenerator>();
        colorManager = GetComponent<ColorManager>();

        sectorMenu = GameObject.Find("BC_Sectors");
        content = GameObject.Find("E_SectorContent").GetComponent<RectTransform>();

        items = new List<GameObject>();
        sectorMenu.SetActive(false);
    }

    #region Create sector

    public void GenerateSectors()
    {
        int chunkVisibleInViewDst = FindObjectOfType<EndlessTerrain>().chunkVisibleInViewDst;

        for (int yOffset = -chunkVisibleInViewDst; yOffset <= chunkVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDst; xOffset <= chunkVisibleInViewDst; xOffset++)
            {
                AddSector(new Vector2(xOffset, yOffset));
            }
        }
    }

    public void AddSector(Vector2 position)
    {
        if(mapGenerator == null)
            mapGenerator = GetComponent<MapGenerator>();

        int chunkSize = mapGenerator.chunkSize();
        float mapX = mapGenerator.realMapSize.x;
        float mapY = mapGenerator.realMapSize.y;
        Vector2 realPosition = position * chunkSize;

        if(realPosition.x + chunkSize / 2 < -mapX || realPosition.x - chunkSize / 2 > mapX
            || realPosition.y + chunkSize / 2 < -mapY || realPosition.y - chunkSize / 2 > mapY)
        {
            return;
        }

        string n = RandomName();
        sectors.Add(new Sector(n, -1, position, realPosition));
    }

    private string RandomName()
    {
        string name = "Sector-" + Random.Range(1000, 9999);

        while (NameAlreadyExist(name))
        {
            name = "Sector-" + Random.Range(1000, 9999);
        }

        return name;
    }

    public bool NameAlreadyExist(string name)
    {
        foreach(Sector s in sectors)
        {
            if(name == s.m_name)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Get sector

    public Sector GetSector(Vector2 position)
    {
        foreach(Sector s in sectors)
        {
            if(s.m_position == position)
            {
                return s;
            }
        }

        Debug.Log("[WARN:SectorManager] Can't find sector " + position.ToString());
        return null;
    }

    public int GetSectorID(Vector2 position)
    {
        for (int i = 0; i < sectors.Count; i++)
        {
            if (sectors[i].m_position == position)
            {
                return i;
            }
        }

        Debug.Log("[WARN:SectorManager] Can't find sector " + position.ToString());
        return -1;
    }

    public List<Sector> GetOwnedSectors(int side)
    {
        List<Sector> ownedSectors = new List<Sector>();

        foreach(Sector s in sectors)
        {
            if(s.m_side == side)
            {
                ownedSectors.Add(s);
            }
        }

        return ownedSectors;
    }

    #endregion

    #region Control and loose sector

    public void ControlSector(Vector2 position, int side)
    {
        int current = GetSectorID(position);

        if (current == -1) return;

        if(sectors[current].m_side == -1)
        {
            sectors[current].m_side = side;
            sectors[current].RestoreSector(side);
            Debug.Log("[INFO:SectorManager] Side " + side + " took control of sector " + sectors[current].m_name);
        }
        else if(sectors[current].m_side != side)
        {
            manager.Notify(
                "You can't control this sector!",
                "Another colony already owns this sector.",
                null,
                colorManager.importantColor,
                5,
                "/tp " + sectors[current].m_position * mapGenerator.chunkSize()
           );

           Debug.Log("[INFO:SectorManager] Side " + side + " can't take control of sector " + sectors[current].m_name);
        }
    }

    public void LooseSector(Vector2 position, int side)
    {
        int current = GetSectorID(position);

        if (current == -1) return;

        if(sectors[current].m_side == side)
        {
            sectors[current].m_side = -1; // -1 is neutral player
            sectors[current].AbandonSector(side);

            manager.Notify(
                "You lost a sector!",
                "Your control building has been destroyed.",
                null,
                colorManager.veryImportantColor,
                5,
                "/tp " + sectors[current].m_position * mapGenerator.chunkSize()
           );
        }
    }

    #endregion

    #region Sectors interface

    public void Btn_SectorMenu()
    {
        bool active = sectorMenu.activeSelf;

        if (active)
        {
            sectorMenu.SetActive(false);
        }
        else
        {
            sectorMenu.SetActive(true);
            ReorderSectors();
        }
    }

    private void DestroyItem()
    {
        if (items.Count == 0) return;

        foreach (GameObject item in items)
        {
            Destroy(item);
        }
    }

    private void ReorderSectors()
    {
        List<Sector> sect = GetOwnedSectors(manager.side);
        float count = sect.Count;

        DestroyItem();

        if (count == 0) return;

        foreach (Sector s in sect)
        {
            Image current = Instantiate(sectorItem, content) as Image;
            items.Add(current.gameObject);
            current.GetComponent<SectorItem>().Init(s);
        }
    }

    #endregion
}

[System.Serializable]
public class Sector
{
    public string m_name;
    public int m_side;
    public Vector2 m_position;
    public Vector2 m_realPosition;

    public List<Entity> m_entitiesInSector;

    public Sector(string name, int side, Vector2 position, Vector2 realPosition)
    {
        m_name = name;
        m_side = side;
        m_position = position;
        m_realPosition = realPosition;
        m_entitiesInSector = new List<Entity>();
    }

    public void AbandonSector(int side)
    {
        if (side != m_side) return;

        bool controled = false;

        foreach(Entity e in m_entitiesInSector)
        {
            if(e.entityType == EntityType.Building && e.side == side)
            {
                if (e.GetComponent<Buildings>().building.canControlSector)
                {
                    controled = true;
                    break;
                }
            }
        }

        if (controled) return;

        foreach(Entity e in m_entitiesInSector)
        {
            e.Abandon();
        }
    }

    public void RestoreSector(int side)
    {
        if (side == m_side) return;

        foreach (Entity e in m_entitiesInSector)
        {
            e.Restore(side);
        }
    }
}