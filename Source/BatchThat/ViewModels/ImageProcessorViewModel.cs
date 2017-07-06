using System.Collections.Generic;
using BatchThat.Image.Filters;

namespace BatchThat.ViewModels
{
    public abstract class ImageProcessorViewModel : ViewModelBase
    {
        private bool _autoCrop;
        private bool _deskew;
        private bool _enhance;
        private bool _greyscale;
        private bool _sharpen;
        private bool _trim;
        private int _total;
        private int _current;

        public bool AutoCrop
        {
            get => _autoCrop;
            set { _autoCrop = value; OnPropertyChanged(GetPropertyInfo(this, x => x.AutoCrop).Name); }
        }

        public bool Deskew
        {
            get => _deskew;
            set { _deskew = value; OnPropertyChanged(GetPropertyInfo(this, x => x.Deskew).Name); }
        }

        public bool Enhance
        {
            get => _enhance;
            set { _enhance = value; OnPropertyChanged(GetPropertyInfo(this, x => x.Enhance).Name); }
        }

        public bool Greyscale
        {
            get => _greyscale;
            set { _greyscale = value; OnPropertyChanged(GetPropertyInfo(this, x => x.Greyscale).Name); }
        }

        public bool Sharpen
        {
            get => _sharpen;
            set { _sharpen = value; OnPropertyChanged(GetPropertyInfo(this, x => x.Sharpen).Name); }
        }

        public bool Trim
        {
            get => _trim;
            set { _trim = value; OnPropertyChanged(GetPropertyInfo(this, x => x.Trim).Name); }
        }

        public int Total
        {
            get => _total;
            set { _total = value; OnPropertyChanged(GetPropertyInfo(this, x => x.Total).Name); }
        }

        public int Current
        {
            get => _current;
            set { _current = value; OnPropertyChanged(GetPropertyInfo(this, x => x.Current).Name); }
        }

        public ImageProcessorViewModel()
        {
            Current = 0;
            Total = 100;
        }

        protected IList<Filter> GetFilters()
        {
            var filters = new List<Filter>();
            if (Deskew)
                filters.Add(new Deskew(60));
            if (AutoCrop)
                filters.Add(new AutoCrop());
            if (Enhance)
                filters.Add(new Enhance());
            if (Greyscale)
                filters.Add(new Grayscale());
            if (Sharpen)
                filters.Add(new Sharpen());
            if (Trim)
                filters.Add(new Trim());
            if (Deskew)
                filters.Add(new Deskew(40));
            if (AutoCrop)
                filters.Add(new AutoCrop());

            return filters;
        }
    }
}
