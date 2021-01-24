using UnityEngine;
using UnityEngine.UI;

public class CommandSystem : MonoBehaviour
{
    [SerializeField] private string commandIndicator = "/";

    private InputField commandInput;
    private GameObject console;
    private Text prompt;

    private MoonManager manager;
    private EndgameChecker endgameChecker;
    private QuestManager quest;

    void Start()
    {
        manager = GetComponent<MoonManager>();
        quest = GetComponent<QuestManager>();
        endgameChecker = GetComponent<EndgameChecker>();

        commandInput = GameObject.Find("IF_Command").GetComponent<InputField>();
        prompt = GameObject.Find("T_CommandPrompt").GetComponent<Text>();
        console = GameObject.Find("E_Command");

        Btn_Prompt();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Btn_Prompt();
        }
    }

    public void Btn_Prompt()
    {
        console.SetActive(!console.activeSelf);
    }

    public void OnEndEdit()
    {
        CheckCommand(commandInput.text);
        commandInput.text = "";
    }

    public void CheckCommand(string command, bool showLog = true)
    {
        if (!command.StartsWith(commandIndicator)) return;

        string baseCommand = command;
        command = command.Remove(0, 1);

        if (command.StartsWith("event"))
        {
            command = command.Remove(0, 6);

            if (command.StartsWith("create"))
            {
                command = command.Remove(0, 7);

                if (command.StartsWith("-c"))
                {
                    command = command.Remove(0, 3);

                    try { int eventIndex = int.Parse(command); manager.InstantiateEvent(eventIndex); }
                    catch { }

                    Debug.Log("  <color=#0000E6>[INFO:Command] Classic event successfully create.</color>");
                    if (!showLog) return;
                }
                else if (command.StartsWith("-s"))
                {
                    command = command.Remove(0, 3);

                    try { int eventIndex = int.Parse(command); manager.InstantiateEvent(eventIndex); }
                    catch { }

                    Debug.Log("  <color=#0000E6>[INFO:Command] Super event successfully create.</color>");
                    if (!showLog) return;
                }
                else if (command.StartsWith("-r"))
                {
                    manager.RandomEvent();
                    Debug.Log("  <color=#0000E6>[INFO:Command] Random event successfully create.</color>");
                    if (!showLog) return;
                }
            }
        }
        else if (command.StartsWith("summon"))
        {
            command = command.Remove(0, 7);

            if (command.StartsWith("-u"))
            {
                command = command.Remove(0, 3);
                string[] args = command.Split(' ');

                if (args.Length != 4) return;

                GameObject go = Instantiate(manager.unitData[int.Parse(args[0])].model, new Vector3(int.Parse(args[1]), int.Parse(args[2]), int.Parse(args[3])), Quaternion.identity);
                Debug.Log("  <color=#0000E6>[INFO:Command] " + manager.unitData[int.Parse(args[0])].name + " successfully create.</color>");
                if (!showLog) return;
            }
            else if (command.StartsWith("-b"))
            {
                command = command.Remove(0, 3);
                string[] args = command.Split(' ');

                if (args.Length != 4) return;

                GameObject go = Instantiate(manager.buildData[int.Parse(args[0])].building, new Vector3(int.Parse(args[1]), int.Parse(args[2]), int.Parse(args[3])), Quaternion.identity);
                Debug.Log("  <color=#0000E6>[INFO:Command] " + manager.buildData[int.Parse(args[0])].name + " successfully create.</color>");
                if (!showLog) return;
            }
            else if (command.StartsWith("-p"))
            {
                command = command.Remove(0, 3);
                string[] args = command.Split(' ');

                if (args.Length != 4) return;

                GameObject go = Instantiate(manager.buildData[int.Parse(args[0])].preview, new Vector3(int.Parse(args[1]), int.Parse(args[2]), int.Parse(args[3])), Quaternion.identity);
                Debug.Log("  <color=#0000E6>[INFO:Command] [P]" + manager.buildData[int.Parse(args[0])].name + " successfully create.</color>");
                if (!showLog) return;
            }
        }
        else if (command.StartsWith("debug"))
        {
            manager.DebugInfo();
            if (!showLog) return;
        }
        else if (command.StartsWith("wave"))
        {
            command = command.Remove(0, 5);
            string[] args = command.Split(' ');

            if (args.Length < 1) return;

            manager.SpawnWave(int.Parse(args[0]), true);
            if (!showLog) return;
        }
        else if (command.StartsWith("killall"))
        {
            command = command.Remove(0, 8);
            string[] args = command.Split(' ');

            if (args.Length < 1) return;

            if(args[0] == "-c")
            {
                EnemyColony[] enemies = FindObjectsOfType<EnemyColony>();

                foreach (EnemyColony e in enemies)
                {
                    e.Failure();
                }
            }
            else if(args[0] == "-u")
            {
                Unit[] units = FindObjectsOfType<Unit>();

                for(int i = 0; i < units.Length; i++)
                {
                    units[i].ApplyDamage(9999, DamageType.Impulsion);
                }
            }
            else
            {
                return;
            }

            if (!showLog) return;
        }
        else if (command.StartsWith("loot"))
        {
            manager.AddRessources(99999, 99999, 99999, 99999, 99999);
            manager.AddOutput(999, 999, 999, 999, 999, 999);
            manager.ManageStorage(99999, 99999, 99999, 99999);

            if (!showLog) return;
        }
        else if (command.StartsWith("time"))
        {
            command = command.Remove(0, 5);
            string[] args = command.Split(' ');

            if (args.Length < 1) return;

            Time.timeScale = float.Parse(args[0]);
            if (!showLog) return;
        }
        else if (command.StartsWith("help"))
        {
            manager.Notify("Command helper\nType /event create -c\\-s\\-r <index> to create an event.");
            manager.Notify("Type /help <index> to get help.\nType /killall to kill every colonies.");
            manager.Notify("Type /loot to get ressources.\nType /notif <index> to create a notification.");
            manager.Notify("Type /summon -b\\-p\\-u <identity> to spawn an entity.\nType /time <speed> to change the time.");
            manager.Notify("Type /debug to view debug.\nType /wave <index> to spawn an enemy wave.");
        }
        else if (command.StartsWith("diplomacy"))
        {
            command = command.Remove(0, 10);
            string[] args = command.Split(' ');

            if (args.Length < 1) return;

            if(args[0] == "-g")
            {
                manager.GenerateDiplomacy();
            }
            else
            {
                return;
            }

            if (!showLog) return;
        }
        else if (command.StartsWith("teleport"))
        {
            command = command.Remove(0, 9);
            string[] args = command.Split(' ');

            if (args.Length < 3) return;

            if(args.Length == 3)
            {
                Vector3 tpPosition = new Vector3();
                bool haveSucceed = true;

                try
                {
                    tpPosition = new Vector3(int.Parse(args[0]), int.Parse(args[1]), int.Parse(args[2]));
                }
                catch
                {
                    Debug.Log("[INFO:Command] Invalid arguments (position expected)!");
                    haveSucceed = false;
                }

                if (haveSucceed)
                {
                    GameObject.Find("Player").transform.position = tpPosition;
                    Debug.Log("<color=#0000E6>[INFO:Command] Player teleported to " + tpPosition.ToString() + ".</color>");
                }
            }
        }
        else if (command.StartsWith("eg"))
        {
            command = command.Remove(0, 3);
            string[] args = command.Split(' ');

            if (args.Length < 1) return;

            if(args[0] == "-w")
            {
                endgameChecker.DeclareVictory(EndgameType.Cheat);
            }
            else if(args[0] == "-d")
            {
                endgameChecker.DeclareDefeat(EndgameType.Cheat);
            }
        }
        else
        {
            return;
        }
        /*else if (command.StartsWith("quest"))
        {
            command = command.Remove(0, 6);

            if (command.StartsWith("-b"))
            {
                command = command.Remove(0, 3);

                string[] args = command.Split(':');
                if (args.Length < 4) return;

                string questName = args[0];
                int entityTypeId = int.Parse(args[1]);
                EntityType entityType;

                if(entityTypeId == 1)
                {
                    entityType = EntityType.Unit;
                }
                else if(entityTypeId == 2)
                {
                    entityType = EntityType.Building;
                }
                else if(entityTypeId == 3)
                {
                    entityType = EntityType.Preview;
                }
                else
                {
                    return;
                }

                int objId = int.Parse(args[2]);
                int objCount = int.Parse(args[3]);

                string callback = "";

                if(args.Length > 4)
                {
                    for (int i = 4; i < args.Length; i++)
                    {
                        if (i == args.Length)
                        {
                            callback += args[i];
                        }
                        else if (i == 4)
                        {
                            callback += args[i];
                        }
                        else
                        {
                            callback += args[i] + ":";
                        }
                    }
                }

                quest.NewBuildQuest(new BuildQuest(questName, entityType, objId, objCount, new QuestReward[0], callback));
            }
            else if (command.StartsWith("-e"))
            {
                command = command.Remove(0, 3);

                string[] args = command.Split(':');
                if (args.Length < 3) return;

                string questName = args[0];
                int objTypeId = int.Parse(args[1]);
                ObjectifType objType;

                if (objTypeId == 4)
                {
                    objType = ObjectifType.Epidemy;
                }
                else if (objTypeId == 5)
                {
                    objType = ObjectifType.Riot;
                }
                else
                {
                    return;
                }

                int objCount = int.Parse(args[2]);

                string callback = "";

                if (args.Length > 3)
                {
                    for (int i = 3; i < args.Length; i++)
                    {
                        if (i == args.Length)
                        {
                            callback += args[i];
                        }
                        else if (i == 3)
                        {
                            callback += args[i];
                        }
                        else
                        {
                            callback += args[i] + ":";
                        }
                    }
                }

                quest.NewEventQuest(new EventQuest(questName, objType, objCount, new QuestReward[0], callback));
            }
            else if (command.StartsWith("-k"))
            {
                command = command.Remove(0, 3);

                string[] args = command.Split(':');
                if (args.Length < 3) return;

                string questName = args[0];
                int entityTypeId = int.Parse(args[1]);
                EntityType entityType;

                if (entityTypeId == 1)
                {
                    entityType = EntityType.Unit;
                }
                else if (entityTypeId == 2)
                {
                    entityType = EntityType.Building;
                }
                else if (entityTypeId == 3)
                {
                    entityType = EntityType.Preview;
                }
                else
                {
                    return;
                }

                int objCount = int.Parse(args[2]);

                string callback = "";

                if(args.Length > 3)
                {
                    for (int i = 3; i < args.Length; i++)
                    {
                        if (i == args.Length)
                        {
                            callback += args[i];
                        }
                        else if (i == 3)
                        {
                            callback += args[i];
                        }
                        else
                        {
                            callback += args[i] + ":";
                        }
                    }
                }

                quest.NewKillQuest(new KillQuest(questName, entityType, objCount, new QuestReward[0], callback));
            }
        }*/

        manager.Notify(string.Format(manager.Traduce("03_notif_commandexecuted"), baseCommand));
    }
}
