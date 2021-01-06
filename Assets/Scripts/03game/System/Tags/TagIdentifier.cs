using System.Collections.Generic;
using UnityEngine;

public class TagIdentifier : MonoBehaviour {

    //public List<string> tags = new List<string>();
    public List<Tag> _tags = new List<Tag>();

    private void Awake()
    {
        GameObject.Find("Manager").GetComponent<TagSystem>().allWithComponent.Add(gameObject);
    }
}

[System.Serializable]
public enum Tag { Untagged, Player, Preview, Building, Unit, Enemy, Turret, Saved, Deposit }