using System.Collections.Generic;
using System.Security.Policy;
using System.Threading.Tasks;
using AutoMapper;
using crmc.wotdisplay.models;

namespace crmc.wotdisplay
{
    public class DisplayQuadrantViewModel
    {
        public int QuadrantIndex { get; set; }

        public QuadrantType QuadrantType { get; set; }

        public List<PersonViewModel> People { get; set; }
        public List<PersonViewModel> CachePeople { get; set; }

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
            Mapper.CreateMap<Person, PersonViewModel>().ReverseMap();
            //TODO: Refactor hardcode url
            var url = "http://localhost/crmc/";
            PersonRepository repo = new PersonRepository(url);
            var list = await repo.Get(25, QuadrantType == QuadrantType.Priority);
          
            People = Mapper.Map<List<Person>, List<PersonViewModel>>(list); 

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
    }

}