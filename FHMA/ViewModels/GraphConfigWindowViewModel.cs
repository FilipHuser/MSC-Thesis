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

            ModuleType = ModuleTypes.FirstOrDefault();
            Channel = Channels.FirstOrDefault();
        }

        #region SELECTED_ITEMS
        public ModuleType? ModuleType { get; set; }
        public int? Channel { get; set; }
        #endregion
    }
}
