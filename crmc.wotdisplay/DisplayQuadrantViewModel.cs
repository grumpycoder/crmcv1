using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using crmc.wotdisplay.models;
using NLog;
using NLog.Fluent;

namespace crmc.wotdisplay
{
    public class DisplayQuadrantViewModel
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public int QuadrantIndex { get; set; }

        public QuadrantType QuadrantType { get; set; }

        public List<PersonViewModel> People { get; set; }
        public List<PersonViewModel> CachePeople { get; set; }

        public bool IsCacheUpdated { get; set; }

        public DisplayQuadrantViewModel()
        {
            People = new List<PersonViewModel>();
            CachePeople = new List<PersonViewModel>();
        }

        public DisplayQuadrantViewModel(QuadrantType quadrantType): this()
        {
            QuadrantType = quadrantType;
        }

        public DisplayQuadrantViewModel(QuadrantType quadrantType, int quadrantIndex) : this(quadrantType)
        {
            QuadrantIndex = quadrantIndex;
        }


        public async Task LoadPeopleAsync()
        {
            var st = DateTime.Now; 
            Log.Debug("Retrieving people for {0}", QuadrantIndex); 
            Mapper.CreateMap<Person, PersonViewModel>().ReverseMap();
            //TODO: Refactor hardcode url
            var url = "http://localhost/crmc/";
            PersonRepository repo = new PersonRepository(url);
            var list = await repo.Get(25, QuadrantType == QuadrantType.Priority);
         
            People = Mapper.Map<List<Person>, List<PersonViewModel>>(list);
            var s = DateTime.Now.Subtract(st);

            Log.Debug("Completed retrieving people for {0} in {1}", QuadrantIndex, s.TotalSeconds);
        }

    }


    public enum QuadrantType
    {
        Normal, 
        Priority, 
        Local
    }

    public class PersonViewModel : Person
    {
        public int RotationCount { get; set; }
        public int QuadrantIndex { get; set; }
        public DateTime NextDisplayTime { get; set; }
        public DateTime LastDisplayTime { get; set; }
    }

}