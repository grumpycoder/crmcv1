//using crmc.domain;
using System.Collections.Generic;
using System.Linq;
using crmc.wotdisplay.models;

namespace crmc.wotdisplay
{
    public class Widget
    {
        private Person currentPerson;
        private Person lastPerson;


        //public Widget(DispatcherTimer timer)
        public Widget()
        {
            //MyTimer = timer;
            PersonList = new List<Person>();
        }

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
            get { return lastPerson ?? (lastPerson = PersonList.LastOrDefault()); }
            set { lastPerson = value; }
        }


        public void UpdatePersonList(IEnumerable<Person> list)
        {
            PersonList = list;
        }

        public void SetNextPerson()
        {
            CurrentPerson = PersonList.Where(i => i.Id > currentPerson.Id).OrderBy(i => i.Id).FirstOrDefault();
            if (CurrentPerson == null)
            {
                currentPerson = PersonList.FirstOrDefault();
            }
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
}