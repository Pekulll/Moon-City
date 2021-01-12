using System.Collections.Generic;
using UnityEngine;

public class TagSystem : MonoBehaviour {

    public List<GameObject> allWithComponent = new List<GameObject> ();

    #region Return Array

    public GameObject[] FindTag (Tag _tag, List<GameObject> objects = null) {
        List<GameObject> gameObjectWithTag = new List<GameObject> ();
        List<GameObject> listToStudy = new List<GameObject> ();

        if (objects != null && objects.Count != 0) {
            try { GameObject go = listToStudy[0]; listToStudy = objects; } catch { listToStudy = allWithComponent; }
        } else {
            listToStudy = allWithComponent;
        }

        for (int i = 0; i < listToStudy.Count; i++) {
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

        return gameObjectWithTag.ToArray ();
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
            bool keep = true;
            TagIdentifier tagId = listToStudy[i].GetComponent<TagIdentifier>();

            foreach (Tag t in _tag)
            {
                if (!tagId._tags.Contains(t))
                {
                    keep = false;
                    break;
                }
            }

            if (keep && gameObjectWithTag.Contains(tagId.gameObject))
            {
                gameObjectWithTag.Add(tagId.gameObject);
            }
        }

        return gameObjectWithTag.ToArray ();
    }

    #endregion

    #region Return list

    public List<GameObject> FindTagList(Tag _tag, List<GameObject> objects = null)
    {
        List<GameObject> gameObjectWithTag = new List<GameObject>();
        List<GameObject> listToStudy = new List<GameObject>();

        if (objects != null && objects.Count != 0)
        {
            try { GameObject go = listToStudy[0]; listToStudy = objects; } catch { listToStudy = allWithComponent; }
        }
        else
        {
            listToStudy = allWithComponent;
        }

        for (int i = 0; i < listToStudy.Count; i++)
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

        return gameObjectWithTag;
    }

    public List<GameObject> FindTagsList (Tag[] _tag, List<GameObject> objects = null) {
        List<GameObject> gameObjectWithTag = new List<GameObject> ();
        List<GameObject> listToStudy = new List<GameObject> ();

        if (objects != null && objects.Count != 0) {
            try { GameObject go = listToStudy[0]; listToStudy = objects; } catch { listToStudy = allWithComponent; }
        } else {
            listToStudy = allWithComponent;
        }

        for (int i = 0; i < listToStudy.Count; i++)
        {
            bool keep = true;
            TagIdentifier tagId = listToStudy[i].GetComponent<TagIdentifier> ();

            foreach(Tag t in _tag)
            {
                if (!tagId._tags.Contains(t))
                {
                    keep = false;
                    break;
                }
            }

            if(keep && gameObjectWithTag.Contains(tagId.gameObject))
            {
                gameObjectWithTag.Add(tagId.gameObject);
            }
        }

        return gameObjectWithTag;
    }

    #endregion
}