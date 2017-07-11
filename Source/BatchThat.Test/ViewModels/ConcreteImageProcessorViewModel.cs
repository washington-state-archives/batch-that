using System.Collections.Generic;
using BatchThat.Image.Filters;
using BatchThat.ViewModels;

namespace BatchThat.Test.ViewModels
{
    public class ConcreteImageProcessorViewModel : ImageProcessorViewModel
    {
        public new IList<Filter> GetFilters()
        {
            return base.GetFilters();
        }
    }
}