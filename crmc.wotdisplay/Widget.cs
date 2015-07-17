//using crmc.domain;

using System;
using System.Collections.Generic;
using System.Linq;
using crmc.wotdisplay.models;

namespace crmc.wotdisplay
{
    public class Widget
    {
        public Widget()
        {
            PersonList = new List<Person>();
            LocalList = new LocalList();
            Quadrant = 0; 
        }

        public LocalList LocalList { get; set; }
        public IEnumerable<Person> PersonList { get; set; }

        public int Quadrant { get; set; }

        public bool IsPriorityList { get; set; }

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