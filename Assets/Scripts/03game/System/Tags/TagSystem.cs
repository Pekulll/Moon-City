using System.Collections.Generic;
using UnityEngine;

public class TagSystem : MonoBehaviour {

    public List<GameObject> allWithComponent = new List<GameObject> ();

    public GameObject[] FindTag (Tag _tag, List<GameObject> objects = null) {
        List<GameObject> gameObjectWithTag = new List<GameObject> ();
        List<GameObject> listToStudy = new List<GameObject> ();

        if (objects != null && objects.Count != 0) {
            try { GameObject go = listToStudy[0]; listToStudy = objects; } catch { listToStudy = allWithComponent; }
        } else {
            listToStudy = allWithComponent;
        }

        for (int i = 0; i < listToStudy.Count; i++) {
            try
            {
                TagIdentifier tagId = listToStudy[i].GetComponent<TagIdentifier>();

                for (int o = 0; o < tagId._tags.Count; o++)
                {
                    if (tagId._tags[o] == _tag)
                    {
                        gameObjectWithTag.Add(tagId.gameObject);
                        break;
                    }
                }
            }
            catch
            {

            }
        }

        return gameObjectWithTag.ToArray ();
    }

    public List<GameObject> FindTagList (Tag _tag, List<GameObject> objects = null) {
        List<GameObject> gameObjectWithTag = new List<GameObject> ();
        List<GameObject> listToStudy = new List<GameObject> ();

        if (objects != null && objects.Count != 0) {
            try { GameObject go = listToStudy[0]; listToStudy = objects; } catch { listToStudy = allWithComponent; }
        } else {
            listToStudy = allWithComponent;
        }

        for (int i = 0; i < listToStudy.Count; i++) {
            TagIdentifier tagId = listToStudy[i].GetComponent<TagIdentifier> ();

            for (int o = 0; o < tagId._tags.Count; o++) {
                if (tagId._tags[o] == _tag) {
                    gameObjectWithTag.Add (tagId.gameObject);
                    break;
                }
            }
        }

        return gameObjectWithTag;
    }

    public GameObject[] FindTags (Tag[] _tag, List<GameObject> objects = null) {
        List<GameObject> gameObjectWithTag = new List<GameObject> ();
        List<GameObject> listToStudy = new List<GameObject> ();

        if (objects != null && objects.Count != 0) {
            try { GameObject go = listToStudy[0]; listToStudy = objects; } catch { listToStudy = allWithComponent; }
        } else {
            listToStudy = allWithComponent;
        }

        for (int i = 0; i < listToStudy.Count; i++) {
            TagIdentifier tagId = listToStudy[i].GetComponent<TagIdentifier> ();

            for (int o = 0; o < tagId._tags.Count; o++) {
                for (int t = 0; t < _tag.Length; t++) {
                    if (tagId._tags[o] == _tag[t]) {
                        gameObjectWithTag.Add (tagId.gameObject);
                        break;
                    }

                    if (gameObjectWithTag.Contains (tagId.gameObject)) {
                        break;
                    }
                }
            }
        }

        return gameObjectWithTag.ToArray ();
    }

    public List<GameObject> FindTagsList (Tag[] _tag, List<GameObject> objects = null) {
        List<GameObject> gameObjectWithTag = new List<GameObject> ();
        List<GameObject> listToStudy = new List<GameObject> ();

        if (objects != null && objects.Count != 0) {
            try { GameObject go = listToStudy[0]; listToStudy = objects; } catch { listToStudy = allWithComponent; }
        } else {
            listToStudy = allWithComponent;
        }

        for (int i = 0; i < listToStudy.Count; i++) {
            TagIdentifier tagId = listToStudy[i].GetComponent<TagIdentifier> ();

            for (int o = 0; o < tagId._tags.Count; o++) {
                for (int t = 0; t < _tag.Length; t++) {
                    if (tagId._tags[o] == _tag[t]) {
                        gameObjectWithTag.Add (tagId.gameObject);
                        break;
                    }

                    if (gameObjectWithTag.Contains (tagId.gameObject)) {
                        break;
                    }
                }
            }
        }

        return gameObjectWithTag;
    }

    public List<TagIdentifier> Find (Tag _tag, List<GameObject> objects = null) {
        List<TagIdentifier> gameObjectWithTag = new List<TagIdentifier> ();
        List<GameObject> listToStudy = new List<GameObject> ();

        if (objects != null && objects.Count != 0) {
            try { GameObject go = listToStudy[0]; listToStudy = objects; } catch { listToStudy = allWithComponent; }
        } else {
            listToStudy = allWithComponent;
        }

        for (int i = 0; i < listToStudy.Count; i++) {
            TagIdentifier tagId = listToStudy[i].GetComponent<TagIdentifier> ();

            for (int o = 0; o < tagId._tags.Count; o++) {
                if (tagId._tags[o] == _tag) {
                    gameObjectWithTag.Add (tagId);
                    break;
                }
            }
        }

        return gameObjectWithTag;
    }

    /*public GameObject[] FindTags (string _tag) {
        List<GameObject> goWithTag = new List<GameObject> ();

        for (int i = 0; i < allWithComponent.Count; i++) {
            TagIdentifier tagId = allWithComponent[i].GetComponent<TagIdentifier> ();

            for (int o = 0; o < tagId.tags.Count; o++) {
                if (tagId.tags[o] == _tag) {
                    goWithTag.Add (allWithComponent[i]);
                    break;
                }
            }
        }

        GameObject[] goFound = new GameObject[goWithTag.Count];

        for (int i = 0; i < goWithTag.Count; i++) {
            goFound[i] = goWithTag[i];
        }

        return goFound;
    }

    public List<GameObject> FindTagsList (string _tag) {
        List<GameObject> goWithTag = new List<GameObject> ();

        for (int i = 0; i < allWithComponent.Count; i++) {
            TagIdentifier tagId = allWithComponent[i].GetComponent<TagIdentifier> ();

            for (int o = 0; o < tagId.tags.Count; o++) {
                if (tagId.tags[o] == _tag) {
                    goWithTag.Add (allWithComponent[i]);
                    break;
                }
            }
        }

        return goWithTag;
    }

    public List<GameObject> FindTagsList (string _tag, List<GameObject> objects) {
        List<GameObject> goWithTag = new List<GameObject> ();

        for (int i = 0; i < objects.Count; i++) {
            TagIdentifier tagId = objects[i].GetComponent<TagIdentifier> ();

            for (int o = 0; o < tagId.tags.Count; o++) {
                if (tagId.tags[o] == _tag) {
                    goWithTag.Add (objects[i]);
                    break;
                }
            }
        }

        return goWithTag;
    }

    public GameObject[] FindTags (string _tag, GameObject[] objects) {
        List<GameObject> goWithTag = new List<GameObject> ();

        for (int i = 0; i < objects.Length; i++) {
            TagIdentifier tagId = objects[i].GetComponent<TagIdentifier> ();

            for (int o = 0; o < tagId.tags.Count; o++) {
                if (tagId.tags[o] == _tag) {
                    goWithTag.Add (objects[i]);
                    break;
                }
            }
        }

        GameObject[] goFound = new GameObject[goWithTag.Count];

        for (int i = 0; i < goWithTag.Count; i++) {
            goFound[i] = goWithTag[i];
        }

        return goFound;
    }

    public List<GameObject> FindTagsList (string[] tags) {
        List<GameObject> goWithTag = new List<GameObject> ();

        for (int i = 0; i < allWithComponent.Count; i++) {
            TagIdentifier tagId = allWithComponent[i].GetComponent<TagIdentifier> ();

            foreach (string tagInId in tagId.tags) {
                foreach (string tagInRes in tags) {
                    if (tagInId == tagInRes) {
                        goWithTag.Add (allWithComponent[i]);
                        break;
                    }
                }

                if (goWithTag.Contains (tagId.gameObject)) break;
            }
        }

        return goWithTag;
    }

    public GameObject[] FindTags (string[] tags) {
        List<GameObject> goWithTag = new List<GameObject> ();

        for (int i = 0; i < allWithComponent.Count; i++) {
            TagIdentifier tagId = allWithComponent[i].GetComponent<TagIdentifier> ();

            foreach (string tagInId in tagId.tags) {
                foreach (string tagInRes in tags) {
                    if (tagInId == tagInRes) {
                        goWithTag.Add (allWithComponent[i]);
                        break;
                    }
                }

                if (goWithTag.Contains (tagId.gameObject)) break;
            }
        }

        GameObject[] goFound = new GameObject[goWithTag.Count];

        for (int i = 0; i < goWithTag.Count; i++) {
            goFound[i] = goWithTag[i];
        }

        return goFound;
    }

    public List<TagIdentifier> Find (string _tag) {
        List<TagIdentifier> goWithTag = new List<TagIdentifier> ();

        for (int i = 0; i < allWithComponent.Count; i++) {
            TagIdentifier tagId = allWithComponent[i].GetComponent<TagIdentifier> ();

            for (int o = 0; o < tagId.tags.Count; o++) {
                if (tagId.tags[o] == _tag) {
                    goWithTag.Add (tagId);
                    break;
                }
            }
        }

        return goWithTag;
    }*/
}