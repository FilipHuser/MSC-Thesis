using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphium.ViewModels;

namespace Graphium.Interfaces
{
    internal interface IViewModelOwner
    {
        #region METHODS
        abstract ViewModelBase GetViewModel(); 
        #endregion
    }
}
