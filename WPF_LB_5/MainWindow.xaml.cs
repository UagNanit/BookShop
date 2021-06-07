using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Security.Cryptography;
using System.Windows.Input;

namespace WPF_LB_5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected bool IsPass(string login, string pas)//метод проверки на подленность логина и пароля
        {
            using (BookShopEntitiesAzure DbModel = new BookShopEntitiesAzure())
            {
                 byte[] data = Encoding.ASCII.GetBytes(pas);
                SHA512 shaM = new SHA512Managed();
                var result = shaM.ComputeHash(data);
                var strPas = Encoding.ASCII.GetString(result);
                if (DbModel.Users.Any(p => p.Login == login && p.Password == strPas))
                { 
                    return true;
                }
                else 
                {
                    return false;
                }
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)//проверка на подленность логина и пароля
        {
            try
            {
                btnLogin.IsEnabled = false;        
                if (IsPass(UserName.Text, PasswordBox.Password)) 
                {
                    WorkWindow work = new WorkWindow(UserName.Text);
                    work.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Incorrect Password!", "",MessageBoxButton.OK, MessageBoxImage.Error);
                }
                btnLogin.IsEnabled = true;
            } 
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }
    }
}
