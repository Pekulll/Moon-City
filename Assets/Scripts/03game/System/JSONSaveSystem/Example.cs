using UnityEngine;

public class Example : MonoBehaviour
{
    private void Start()
    {
        JSONSaveSystem.Save(
            Application.persistentDataPath + "/myFolder",
            "myFile",
            new SaveExample()
        );
    }
    
    [System.Serializable]
    public class SaveExample
    {
        public int myInt;
        public float myFloat;
        public double myDouble;
        public bool myBool;
        public string[] myTab;

        public SaveExample()
        {
            this.myInt = 15;
            this.myFloat = 9f;
            this.myDouble = 20.02;
            this.myBool = true;
            this.myTab = new string[] {"Hello", "Beautiful", "World"};
        }
    }
}
