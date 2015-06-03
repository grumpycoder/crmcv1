//using crmc.domain;

using System;
using System.Collections.Generic;
using System.Linq;
using crmc.wotdisplay.models;

namespace crmc.wotdisplay
{
    public class Widget
    {
        private Person currentPerson;
        private Person lastPerson;

        public Widget()
        {
            //MyTimer = timer;
            PersonList = new List<Person>();
            LocalList = new LocalList();
        }

        public LocalList LocalList { get; set; }
        public IEnumerable<Person> PersonList { get; set; }

        public int SkipCount { get; set; }
        public int ListSize { get; set; }
        public bool IsPriorityList { get; set; }

        public Person CurrentPerson
        {
            get { return currentPerson ?? (currentPerson = PersonList.FirstOrDefault()); }
            set { currentPerson = value; }
        }

        public Person LastPerson
        {
            get
            {
                var list = PersonList.ToList();
                return list.LastOrDefault();
                //return lastPerson ?? (lastPerson = PersonList.LastOrDefault());
            }
            set { lastPerson = value; }
        }
        
        public void UpdatePersonList(IEnumerable<Person> list)
        {
            PersonList = list;
        }

        public void SetNextPerson()
        {
            var list = PersonList.ToList();
            var idx = list.IndexOf(currentPerson);

            var nextIdx = idx + 1;
            if (nextIdx > list.Count - 1)
            {
                nextIdx = 0; 
            }
            CurrentPerson = list[nextIdx];

            //if (CurrentPerson == null)
            //{
            //    currentPerson = list[0];
            //}

            //CurrentPerson = PersonList.Where(i => i.Id > currentPerson.Id).OrderBy(i => i.Id).FirstOrDefault();
            //if (CurrentPerson == null)
            //{
            //    currentPerson = PersonList.FirstOrDefault();
            //}
        }

        public SectionSetting SectionSetting { get; set; }

    }

    public class SectionSetting
    {
        public int TopMargin { get; set; }
        public int BottomMargin { get; set; }
        public int LeftMargin { get; set; }
        public int RightMargin { get; set; }
        public int Quadrant { get; set; }
    }

    public class LocalList
    {
        public LocalList()
        {
            LocalItems = new List<LocalItem>();
        }
        public List<LocalItem> LocalItems { get; set;  } 
    }

    public class LocalItem
    {
        public Person Person { get; set; }
        public int Kiosk { get; set; }
        public int RotationCount { get; set; }
        public double LastTickTime { get; set; }   
    }


}