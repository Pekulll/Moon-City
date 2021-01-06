using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ResumeMotor))]
[RequireComponent(typeof(ResearchSystem))]
[RequireComponent(typeof(TooltipMotor))]
[RequireComponent(typeof(DiplomacySystem))]
[RequireComponent(typeof(EventSystem))]
[RequireComponent(typeof(InformationsViewer))]
public class InterfaceManager : MonoBehaviour
{
    #region Show and hide

    private BuildSystem buildSystem;
    private ResearchSystem researchSystem;
    private ResumeMotor resumeMotor;
    private TooltipMotor tooltipMotor;
    private DiplomacySystem diplomacySystem;
    private EventSystem eventSystem;
    private InformationsViewer statsViewer;

    private GameObject mainInterface;

    private void Start()
    {
        buildSystem = FindObjectOfType<BuildSystem>();
        researchSystem = GetComponent<ResearchSystem>();
        resumeMotor = GetComponent<ResumeMotor>();
        tooltipMotor = GetComponent<TooltipMotor>();
        diplomacySystem = GetComponent<DiplomacySystem>();
        eventSystem = GetComponent<EventSystem>();
        statsViewer = GetComponent<InformationsViewer>();

        mainInterface = GameObject.Find("C_Interface");
    }

    public void ResetInterface()
    {
        HideInterface();
        eventSystem.DisplayEvent();
        mainInterface.SetActive(true);
    }

    public void HideInterface()
    {
        mainInterface.SetActive(false);
        buildSystem.HideBuildMenu();
        resumeMotor.HidePause();
        tooltipMotor.ResetTooltip();
        researchSystem.HideUI();
        diplomacySystem.Btn_HideDiplomacy();
        //eventSystem.HideEvent();
        statsViewer.HideInformation(forced: true);
    }

    public void ResetLeft()
    {
        buildSystem.HideBuildMenu();
        researchSystem.HideUI();
    }

    public void ResetRight()
    {
        diplomacySystem.Btn_HideDiplomacy();
        statsViewer.HideInformation(forced: true);
    }

    #endregion
}
