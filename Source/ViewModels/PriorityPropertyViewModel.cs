using System.Collections.Generic;
using System.Reflection;
using DuplicateHider.Models;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace DuplicateHider.ViewModels
{
    public class PriorityPropertyViewModel : ObservableObject
    {
        private PriorityProperty priorityProperty;
        public PriorityProperty PriorityProperty { get => priorityProperty; set => SetValue(ref priorityProperty, value); }

        private PropertyInfo propertyInfo;

        public PriorityPropertyViewModel(PriorityProperty priorityProperty, IPlayniteAPI playniteAPI)
        {
            this.priorityProperty = priorityProperty;
            var gameType = typeof(Game);
            if (gameType.GetProperty(priorityProperty.PropertyName) is PropertyInfo info)
            {
                propertyInfo = info;
            }
        }


    }
}
