using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FHMA.Models;

namespace FHMA.ViewModels
{
    class GraphConfigWindowViewModel : BaseViewModel
    {

        public ObservableCollection<ModuleType> ModuleTypes { get; private set; }
        public ObservableCollection<int> Channels { get; private set; }

        public GraphConfigWindowViewModel()
        {
            int.TryParse(ConfigurationManager.AppSettings["MaxChannels"] , out int maxGraphs);


            ModuleTypes = new ObservableCollection<ModuleType>(Enum.GetValues(typeof(ModuleType)).Cast<ModuleType>());
            Channels = new ObservableCollection<int>(Enumerable.Range(0, maxGraphs + 1));

            Graph = new Graph()
            {
                ModuleType = ModuleTypes.FirstOrDefault(),
                Channel = Channels.FirstOrDefault(),
                UpperBound = 10,
                LowerBound = -10,
                PointLimit = 1000,
            };
        }

        #region SELECTED_ITEMS
        public Graph Graph { get; set; }        
        #endregion
    }
}
