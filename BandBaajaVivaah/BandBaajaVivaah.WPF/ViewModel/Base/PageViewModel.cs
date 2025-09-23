using System.Collections.ObjectModel;

namespace BandBaajaVivaah.WPF.ViewModel.Base
{
    public abstract class PageViewModel<T> : ViewModelBase where T : class
    {
        protected List<T> AllItems { get; set; }
        public ObservableCollection<T> DisplayedItems { get; set; }

        private T _selectedItem;
        public T SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
                UpdateNavigationStatus();
            }
        }

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = value;
                OnPropertyChanged(nameof(PageSize));
                UpdateDisplayedItems();
            }
        }

        private int _totalPages;
        public int TotalPages
        {
            get => _totalPages;
            set
            {
                _totalPages = value;
                OnPropertyChanged(nameof(TotalPages));
                UpdateNavigationStatus();
            }
        }

        public bool IsOnFirstPage => CurrentPage == 1;
        public bool IsOnLastPage => CurrentPage == TotalPages;

        protected PageViewModel()
        {
            AllItems = new List<T>();
            DisplayedItems = new ObservableCollection<T>();
        }

        /// <summary>
        /// Child classes MUST implement this to provide their own data loading logic.
        /// </summary>
        public abstract Task LoadDataAsync();

        /// <summary>
        /// Child classes MUST implement this to provide their own filtering logic.
        /// </summary>
        protected abstract IEnumerable<T> ApplyFiltering(IEnumerable<T> items);

        /// <summary>
        /// The core method that applies filtering and pagination to update the UI.
        /// </summary>
        protected void UpdateDisplayedItems()
        {
            // 1. Apply the specific filters from the child class
            var filteredList = ApplyFiltering(AllItems).ToList();

            // 2. Calculate total pages based on the filtered list
            TotalPages = (int)Math.Ceiling(filteredList.Count / (double)PageSize);
            if (TotalPages == 0) TotalPages = 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            // 3. Get the items for the current page
            var pagedItems = filteredList
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize);

            // 4. Update the collection bound to the UI
            DisplayedItems.Clear();
            foreach (var item in pagedItems)
            {
                DisplayedItems.Add(item);
            }
        }

        private void UpdateNavigationStatus()
        {
            OnPropertyChanged(nameof(IsOnFirstPage));
            OnPropertyChanged(nameof(IsOnLastPage));
        }

        public void GoToFirstPage()
        {
            if (!IsOnFirstPage)
            {
                CurrentPage = 1;
                UpdateDisplayedItems();
            }
        }

        public void GoToPreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                UpdateDisplayedItems();
            }
        }

        public void GoToNextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                UpdateDisplayedItems();
            }
        }

        public void GoToLastPage()
        {
            if (!IsOnLastPage)
            {
                CurrentPage = TotalPages;
                UpdateDisplayedItems();
            }
        }
    }
}
