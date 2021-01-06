using UnityEngine;
using UnityEngine.UI;

public class SectorItem : MonoBehaviour
{
    private Sector sector;

    private MoonManager manager;
    private ColorManager colorManager;

    public void Init(Sector s)
    {
        manager = GameObject.Find("Manager").GetComponent<MoonManager>();
        colorManager = GameObject.Find("Manager").GetComponent<ColorManager>();

        sector = s;
        gameObject.name = "Btn_Sector (" + s.m_side + ")";

        transform.Find("BC_Name/T_Name").GetComponent<Text>().text = sector.m_name; // Name
        transform.Find("BC_Entity/T_Name").GetComponent<Text>().text = sector.m_entitiesInSector.Count.ToString("00"); // Entities in sector
        transform.Find("BC_Position/T_Name").GetComponent<Text>().text = sector.m_position.ToString().Replace("(", "").Replace(")", ""); // Sector location

        AssignColor();
    }

    private void AssignColor()
    {
        transform.Find("BC_Name/T_Name").GetComponent<Text>().color = colorManager.text; // Name
        transform.Find("BC_Entity/T_Name").GetComponent<Text>().color = colorManager.text; // Entities in sector
        transform.Find("BC_Position/T_Name").GetComponent<Text>().color = colorManager.text; // Sector location

        transform.Find("Btn_Targetting/I_Icon").GetComponent<Image>().color = colorManager.icon; // Targetting
        transform.Find("BC_Entity/I_Icon").GetComponent<Image>().color = colorManager.icon; // Entities in sector
        transform.Find("BC_Position/I_Icon").GetComponent<Image>().color = colorManager.icon; // Sector location

        transform.Find("BC_Name").GetComponent<Image>().color = colorManager.forground; // Name
        transform.Find("Btn_Targetting").GetComponent<Image>().color = colorManager.forground; // Targetting
        transform.Find("BC_Entity").GetComponent<Image>().color = colorManager.forground; // Entities in sector
        transform.Find("BC_Position").GetComponent<Image>().color = colorManager.forground; // Sector location

        gameObject.GetComponent<Image>().color = colorManager.background;
        gameObject.GetComponent<Outline>().effectColor = colorManager.backgroundBorder;
    }

    public void Btn_GoToSector()
    {
        manager.TeleportPlayer(new Vector3(sector.m_realPosition.x, 30, sector.m_realPosition.y - 20));
    }
}
