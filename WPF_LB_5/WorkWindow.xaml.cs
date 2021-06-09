using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WPF_LB_5
{
    /// <summary>
    /// Interaction logic for WorkWindow.xaml
    /// </summary>
    public partial class WorkWindow : Window
    {
        
        public WorkWindow(string login)
        {
            InitializeComponent();

            RefreshBooks();
            LoadBooks();
            

            userName = login;
            lbName.Content = login;
            tm = new TimerCallback(UpdateTime);
            time = new Timer(tm, null, 0, 1000);
            saleBooks.CollectionChanged += saleBooks_CollectionChanged;
        }

        BookShopEntitiesAzure DbModel = new BookShopEntitiesAzure();//обьектная модель базы данных
        public ObservableCollection<Sales> saleBooks = new ObservableCollection<Sales>();//колекция кник в корзине
       



        public string userName="MANAGER NAME";
        private int tempIdEditBook;//для временного хранения id книги
        private int tempIdEditDescr;//для временного хранения id описания книги 

        TimerCallback tm;
        Timer time;//

        private void saleBooks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            countStaffinCart.Badge = saleBooks.Count;
            if (saleBooks.Count==0) { lbCheck.Content = 0; }
            else
            {
                decimal check = 0;
                foreach(Sales s in saleBooks)
                {
                    check += s.Price;
                }
                lbCheck.Content = check;
            }
        }

        protected void UpdateTime(object obj)//метод для обновления времени
        { 
            var date = DateTime.Now;
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate () {
                        lbDate.Content = string.Format("Today is {0:D}", date);
                        lbTame.Content = string.Format("The time is {0:t}", date);
                    });
        }
       
        protected void LoadBooks()
        {
            try
            {
                var db = DbModel.Books.ToList().Select(s => new GridBook(s));
                BaseGrid.ItemsSource = null;
                BaseGrid.ItemsSource = db.ToList();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        protected void RefreshBooks()
        {
            try
            {
                CombAuthor.ItemsSource = null;
                CombGenre.ItemsSource = null;
                CombPublisher.ItemsSource = null;
                CombSeries.ItemsSource = null;
                tbxTitle.Text = string.Empty;
                tbxYear.Text = string.Empty;
                tbxAmount.Text = string.Empty;
                tbxPages.Text = string.Empty;
                tbxCost.Text = string.Empty;
                tbxSale.Text = string.Empty;
                CombAuthor.Text = string.Empty;
                CombGenre.Text = string.Empty;
                CombPublisher.Text = string.Empty;
                CombSeries.Text = string.Empty;
                CombAuthor.ItemsSource = DbModel.Authors.Select(s => s.AuthorName).ToList();
                CombGenre.ItemsSource = DbModel.Genres.Select(s => s.GenresName).ToList();
                CombPublisher.ItemsSource = DbModel.Publishers.Select(s => s.PublisherName).ToList();
                CombSeries.ItemsSource = DbModel.Series.Select(s => s.SeriesName).ToList();

                tblSearch.Text = string.Empty;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

       

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnAdd.IsEnabled = false;

                if (!CheckFealds()) { return; }

                Books book = new Books();
                book.Title = tbxTitle.Text;
                book.Year = Int32.Parse(tbxYear.Text);
                book.Amount= Int32.Parse(tbxAmount.Text); 
                book.Pages = Int32.Parse(tbxPages.Text);
                book.Coste_price = Decimal.Parse(tbxCost.Text);
                book.Sale_price = Decimal.Parse(tbxSale.Text);
                if (CombAuthor.SelectedIndex == -1) { book.Authors = new Authors { AuthorName = CombAuthor.Text }; }
                else { book.Authors = DbModel.Authors.Where(s => s.AuthorName == CombAuthor.Text).FirstOrDefault(); }
                if (CombGenre.SelectedIndex == -1) { book.Genres = new Genres { GenresName = CombGenre.Text }; }
                else { book.Genres = DbModel.Genres.Where(s => s.GenresName == CombGenre.Text).FirstOrDefault(); }
                if (CombPublisher.SelectedIndex == -1) { book.Publishers = new Publishers { PublisherName = CombPublisher.Text }; }
                else { book.Publishers = DbModel.Publishers.Where(s => s.PublisherName == CombPublisher.Text).FirstOrDefault(); }
                if (CombSeries.SelectedIndex == -1) { book.Series = new Series { SeriesName = CombSeries.Text }; }
                else { book.Series = DbModel.Series.Where(s => s.SeriesName == CombSeries.Text).FirstOrDefault(); }

                if (!DbModel.Books.Any(b => b.Title == book.Title && b.Year == book.Year))
                {
                    DbModel.Books.Add(book);
                    DbModel.SaveChanges();
                    RefreshBooks();
                    LoadBooks();
                }
                else {new MessageBoxCustom("This book is already in the database!", MessageType.Error, MessageButtons.Ok).ShowDialog(); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { btnAdd.IsEnabled = true; }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnDel.IsEnabled = false;
                if (BaseGrid.SelectedIndex < 0) return;

                var gb = BaseGrid.SelectedItem as GridBook;
                if (gb == null) return;
                var bk = DbModel.Books.Where(b => b.Id == gb.Id).First();
                if (bk != null)
                {
                    bool? Result = new MessageBoxCustom("Are you sure, You want to delite book ? ", 
                        MessageType.Confirmation, MessageButtons.YesNo).ShowDialog();
                    if (!Result.Value) { return; }
                    DbModel.Books.Remove(bk);
                    DbModel.SaveChangesAsync().Wait();
                    RefreshBooks();
                    LoadBooks();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { btnDel.IsEnabled = true; }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DbModel != null) { DbModel.Dispose(); }
        }
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BaseGrid.SelectedIndex < 0)
                {
                    new MessageBoxCustom("Select row!",MessageType.Error, MessageButtons.Ok).ShowDialog();
                    return;
                }

                btnEdit.IsEnabled = false;
                btnAdd.IsEnabled = false;
                btnDel.IsEnabled = false;

                var grbk = (BaseGrid.SelectedItem as GridBook);
                if (grbk == null) return;
                tempIdEditBook = grbk.Id;

                tbxTitle.Text = grbk.Title;
                tbxYear.Text = grbk.Year.ToString();
                tbxAmount.Text = grbk.Amount.ToString();
                tbxPages.Text = grbk.Pages.ToString();
                tbxCost.Text = grbk.Coste_price.ToString();
                tbxSale.Text = grbk.Sale_price.ToString();
                CombAuthor.Text = grbk.Author;
                CombGenre.Text = grbk.Genre;
                CombPublisher.Text = grbk.Publisher;
                CombSeries.Text = grbk.Series;

                btnsCanSav.Visibility = Visibility.Visible;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void EndEdit()
        {
            RefreshBooks();
            LoadBooks();
            btnsCanSav.Visibility = Visibility.Collapsed;
            btnEdit.IsEnabled = true;
            btnAdd.IsEnabled = true;
            btnDel.IsEnabled = true;
            tempIdEditBook = -1;
        }

        private bool CheckFealds()
        {
            if (tbxTitle.Text == string.Empty || tbxYear.Text == string.Empty ||
               tbxPages.Text == string.Empty || tbxCost.Text == string.Empty ||
               tbxSale.Text == string.Empty || CombAuthor.Text == string.Empty ||
               CombGenre.Text == string.Empty || CombPublisher.Text == string.Empty ||
               CombSeries.Text == string.Empty || tbxAmount.Text == string.Empty)
            {
                new MessageBoxCustom("Not all data is filled!", MessageType.Error, MessageButtons.Ok).ShowDialog();
                return false;
            }
            else return true;
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
           
            if (!CheckFealds()){ return; }
            var book = DbModel.Books.Where(b => b.Id == tempIdEditBook).FirstOrDefault();
            if (book == null) return;
           
            book.Title = tbxTitle.Text;
            book.Year = Int32.Parse(tbxYear.Text);
            book.Amount = Int32.Parse(tbxAmount.Text);
            book.Pages = Int32.Parse(tbxPages.Text);
            book.Coste_price = Decimal.Parse(tbxCost.Text);
            book.Sale_price = Decimal.Parse(tbxSale.Text);
            if (CombAuthor.SelectedIndex == -1) { book.Authors = new Authors { AuthorName = CombAuthor.Text }; }
            else { book.Authors = DbModel.Authors.Where(s => s.AuthorName == CombAuthor.Text).FirstOrDefault(); }
            if (CombGenre.SelectedIndex == -1) { book.Genres = new Genres { GenresName = CombGenre.Text }; }
            else { book.Genres = DbModel.Genres.Where(s => s.GenresName == CombGenre.Text).FirstOrDefault(); }
            if (CombPublisher.SelectedIndex == -1) { book.Publishers = new Publishers { PublisherName = CombPublisher.Text }; }
            else { book.Publishers = DbModel.Publishers.Where(s => s.PublisherName == CombPublisher.Text).FirstOrDefault(); }
            if (CombSeries.SelectedIndex == -1) { book.Series = new Series { SeriesName = CombSeries.Text }; }
            else { book.Series = DbModel.Series.Where(s => s.SeriesName == CombSeries.Text).FirstOrDefault(); }
            DbModel.SaveChangesAsync().Wait();
            RefreshBooks();
            EndEdit();
        }

        public bool LongestCommonSubstring(string VariantWord, string SearchWord)//Алгоритм поиска наибольшей общей подстроки
        {
            VariantWord = VariantWord.ToUpper();
            SearchWord = SearchWord.ToUpper();
            var n = VariantWord.Length;
            var m = SearchWord.Length;
            var array = new int[n, m];
            var maxValue = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (VariantWord[i] == SearchWord[j])
                    {
                        array[i, j] = (i == 0 || j == 0) ? 1 : array[i - 1, j - 1] + 1;
                        if (array[i, j] > maxValue)
                        {
                            maxValue = array[i, j];
                        }
                    }
                }
            }
            if (SearchWord.Length - maxValue <= (SearchWord.Length / 2))
            {
                return true;
            }
            else return false;          
        }

        protected void LoadSearchBooks(string searchWord, string table)
        {
            try
            {
                IEnumerable<GridBook> db = new List<GridBook>();
                switch (table)
                {
                    case "Title":
                        db = DbModel.Books.ToList().Where(b => LongestCommonSubstring(b.Title, searchWord)).Select(s => new GridBook(s));
                        break;
                    case "Genre":
                        db = DbModel.Books.ToList().Where(b => LongestCommonSubstring(b.Genres.GenresName, searchWord)).Select(s => new GridBook(s));
                        break;
                    case "Author":
                        db = DbModel.Books.ToList().Where(b => LongestCommonSubstring(b.Authors.AuthorName, searchWord)).Select(s => new GridBook(s));
                        break;
                }
                
                BaseGrid.ItemsSource = null;
                BaseGrid.ItemsSource = db.ToList();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btnCansel_Click(object sender, RoutedEventArgs e)
        {
            EndEdit();
        }

        private void btbSearch_Click(object sender, RoutedEventArgs e)
        {
            if (tblSearch.Text == string.Empty) return;
            LoadSearchBooks(tblSearch.Text, cmbSearch.Text);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            DbModel = new BookShopEntitiesAzure();
            saleBooks.Clear();
            RefreshBooks();
            LoadBooks();
        }

       
        private void btnMinOun_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnMinOun.IsEnabled = false;
                if (BaseGrid.SelectedIndex < 0) return;
                var ind = BaseGrid.SelectedIndex;
                GridBook gb = BaseGrid.SelectedItem as GridBook;
                if (gb == null ) return;
                else if( gb.Amount == 0) return;

                var bk = DbModel.Books.Where(b => b.Id == gb.Id).FirstOrDefault();
                if (bk != null)
                {
                    bk.Amount--;
                    gb.Amount--;
                    DbModel.SaveChangesAsync().Wait();
                    LoadBooks();
                    BaseGrid.SelectedIndex = ind;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { btnMinOun.IsEnabled = true; }
        }

        private void btnSale_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnSale.IsEnabled = false;
                if (BaseGrid.SelectedIndex < 0) return;
                var ind = BaseGrid.SelectedIndex;
                GridBook gb = BaseGrid.SelectedItem as GridBook;
                if (gb == null) return;
                else if (gb.Amount == 0) return;

                var bk = DbModel.Books.Where(b => b.Id == gb.Id).FirstOrDefault();
                if (bk != null)
                {
                    bk.Amount--;

                    Sales sl = new Sales
                    {
                        Count = 1,
                        Title = bk.Title,
                        Author = bk.Authors.AuthorName,
                        Genre = bk.Genres.GenresName,
                        Price = bk.Sale_price,
                        Date = DateTime.Now,
                        Seller = userName
                    };
                    
                    //DbModel.Sales.Add(sl);
                    //DbModel.SaveChangesAsync().Wait();
                    saleBooks.Add(sl);
                    LoadBooks();
                    BaseGrid.SelectedIndex = ind;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { btnSale.IsEnabled = true; }
        }

        private void btnTopBooks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var db = DbModel.Sales.GroupBy(gr => gr.Title).Select(sel => new { Title = sel.Key, Sale = sel.Sum(x => x.Count) }).OrderByDescending(or=>or.Sale);
                BaseGrid.ItemsSource = null;
                BaseGrid.ItemsSource = db.ToList();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btnTopAuthors_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var db = DbModel.Sales.GroupBy(gr => gr.Author).Select(sel => new { Author = sel.Key, Sale = sel.Sum(x => x.Count) }).OrderByDescending(or => or.Sale);
                BaseGrid.ItemsSource = null;
                BaseGrid.ItemsSource = db.ToList();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btnTopGebres_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var db = DbModel.Sales.GroupBy(gr => gr.Genre).Select(sel => new { Genre = sel.Key, Sale = sel.Sum(x => x.Count) }).OrderByDescending(or => or.Sale);
                BaseGrid.ItemsSource = null;
                BaseGrid.ItemsSource = db.ToList();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }

        private void btnOpenFliperBookInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BaseGrid.SelectedIndex < 0) return;
                var ind = BaseGrid.SelectedIndex;
                GridBook gb = BaseGrid.SelectedItem as GridBook;
                if (gb == null) return;

                var bk = DbModel.BooksInfo.Where(b => b.IdBook == gb.Id).FirstOrDefault();
                if (bk != null)
                {
                    img.Source = new BitmapImage(new Uri(@bk.Url));
                    tbxBookDescr.Text = bk.Description;
                    BaseGrid.SelectedIndex = ind;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btnCloseFliperBookInfo_Click(object sender, RoutedEventArgs e)
        {
            img.Source = null;
            tbxBookDescr.Text = "Description";
        }

        private void BaseGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (BaseGrid.SelectedIndex < 0 || !FliperBookInfo.IsFlipped) return;
                var ind = BaseGrid.SelectedIndex;
                GridBook gb = BaseGrid.SelectedItem as GridBook;
                if (gb == null) return;

                var bk = DbModel.BooksInfo.Where(b => b.IdBook == gb.Id).FirstOrDefault();
                if (bk != null)
                {
                    img.Source = new BitmapImage(new Uri(@bk.Url));
                    tbxBookDescr.Text = bk.Description;
                    BaseGrid.SelectedIndex = ind;
                }
                else
                {
                    img.Source = null;
                    tbxBookDescr.Text = "Description";
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btnEditDescr_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BaseGrid.SelectedIndex < 0) return;
  
                GridBook gb = BaseGrid.SelectedItem as GridBook;
                if (gb == null) return;
                tempIdEditDescr = gb.Id;
                var bk = DbModel.BooksInfo.Where(b => b.IdBook == tempIdEditDescr).FirstOrDefault();
                if (bk != null)
                {
                    txbUrlImg.Text = bk.Url;
                }
                else
                {
                    txbUrlImg.Text = string.Empty;
                }
                BaseGrid.IsEnabled = false;
                btnCloseFliperBookInfo.IsEnabled = false;
                txbUrlImg.Visibility = Visibility.Visible;
                btnCancelDescr.Visibility = Visibility.Visible;
                btnSaveDescr.Visibility = Visibility.Visible;
                tbxBookDescr.IsReadOnly = false;
                btnEditDescr.IsEnabled = false;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btnSaveDescr_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BooksInfo bk = DbModel.BooksInfo.Where(b => b.IdBook == tempIdEditDescr).FirstOrDefault();
                if (bk != null)
                {
                    bk.Url = txbUrlImg.Text;
                    bk.Description = tbxBookDescr.Text;
                }
                else
                {
                    bk = new BooksInfo();
                    bk.Url = txbUrlImg.Text;
                    bk.Description = tbxBookDescr.Text;
                    bk.IdBook = tempIdEditDescr;
                    DbModel.BooksInfo.Add(bk);
                }
                img.Source = new BitmapImage(new Uri(@bk.Url));
                DbModel.SaveChangesAsync().Wait();
                btnCancelDescr_Click(sender, e);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btnCancelDescr_Click(object sender, RoutedEventArgs e)
        {
            BaseGrid.IsEnabled = true;
            btnCloseFliperBookInfo.IsEnabled = true;
            txbUrlImg.Visibility = Visibility.Collapsed;
            tbxBookDescr.IsReadOnly = true;
            btnCancelDescr.Visibility = Visibility.Collapsed;
            btnSaveDescr.Visibility = Visibility.Collapsed;
            btnEditDescr.IsEnabled = true;
            txbUrlImg.Text = string.Empty;
            tempIdEditDescr = -1;
        }

        private void btnTotalSale_Click(object sender, RoutedEventArgs e)
        {
            if (saleBooks.Count == 0) { return; }
            SaleWindow sale = new SaleWindow(this);
            sale.Owner = this;
            bool? Result = sale.ShowDialog();
            if (Result.Value) 
            {
                foreach(Sales s in saleBooks)
                {
                    DbModel.Sales.Add(s);
                }
                DbModel.SaveChangesAsync().Wait(); 
            }
            saleBooks.Clear();
            LoadBooks();
        }
    }
}
