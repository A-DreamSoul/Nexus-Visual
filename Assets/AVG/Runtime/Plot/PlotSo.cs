﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AVG.Runtime
{
    [Serializable]
    public class SectionCollection
    {
        public List<StartSection> startSections = new List<StartSection>();
        public List<DialogueSection> dialogueSections = new List<DialogueSection>();

        public void ResetData()
        {
            startSections?.Clear();
            dialogueSections?.Clear();
        }

        public Dictionary<string, BaseSection> ToDictionary()
        {
            var sectionList = new List<BaseSection>();
            sectionList.AddRange(startSections);
            sectionList.AddRange(dialogueSections);
            return sectionList.ToDictionary(sec => sec.Guid);
        }

        public SectionCollection(Dictionary<string, BaseSection> dictionary)
        {
            foreach (var section in dictionary.Values)
            {
                switch (section)
                {
                    case StartSection start:
                        if (!startSections.Contains(start)) startSections.Add(start);
                        break;
                    case DialogueSection dialogue:
                        if (!dialogueSections.Contains(dialogue)) dialogueSections.Add(dialogue);
                        break;
                    default:
                        Debug.Log("Unknown BaseSection");
                        break;
                }
            }
        }

        public SectionCollection()
        {
        }
    }

    [CreateAssetMenu(fileName = "New Plot", menuName = "AVG/Creat Plot")]
    public class PlotSo : ScriptableObject
    {
        public List<StartSection> startSections = new List<StartSection>();
        public List<DialogueSection> dialogueSections = new List<DialogueSection>();

        public void ResetData()
        {
            startSections?.Clear();
            dialogueSections?.Clear();
        }

        public Dictionary<string, BaseSection> BaseSectionDic
        {
            get
            {
                var sectionList = new List<BaseSection>();
                sectionList.AddRange(startSections);
                sectionList.AddRange(dialogueSections);
                return sectionList.ToDictionary(sec => sec.Guid);
            }
        }


        public void SectionCollect(Dictionary<string, BaseSection> dictionary)
        {
            foreach (var section in dictionary.Values)
            {
                switch (section)
                {
                    case StartSection start:
                        if (!startSections.Contains(start)) startSections.Add(start);
                        break;
                    case DialogueSection dialogue:
                        if (!dialogueSections.Contains(dialogue)) dialogueSections.Add(dialogue);
                        break;
                    default:
                        Debug.Log("Unknown BaseSection");
                        break;
                }
            }
        }
    }
}