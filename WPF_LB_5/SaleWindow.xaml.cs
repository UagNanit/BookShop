using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;

namespace WPF_LB_5
{
    /// <summary>
    /// Interaction logic for SaleWindow.xaml
    /// </summary>
    public partial class SaleWindow : Window
    {
        public SaleWindow(WorkWindow ouner)
        {
            InitializeComponent();
            saleBooks.CollectionChanged += saleBooks_CollectionChanged;
            saleBooks = ouner.saleBooks;
            lbCheck.Content = ouner.lbCheck.Content;
            SaleList.ItemsSource = saleBooks;
        }

        public ObservableCollection<Sales> saleBooks = new ObservableCollection<Sales>();

        private void btnClose_Click(object sender, RoutedEventArgs e)//закрыть окно
        {
            this.DialogResult = false;
            this.Close();
        }
       
        private void saleBooks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)//подсчет цены товара в чеке
        {
           
            if (saleBooks.Count == 0) { lbCheck.Content = 0; }
            else
            {
                decimal check = 0;
                foreach (Sales s in saleBooks)
                {
                    check += s.Price;
                }
                lbCheck.Content = check;
            }
        }

        private void btnCheck_Click(object sender, RoutedEventArgs e)//осуществить "продажу книг"
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
