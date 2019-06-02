using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;


namespace chat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        User MainUser;
        public MainWindow()
        {
            InitializeComponent();

            WorkHorse.GetAllBlockedUsers();
        }

        // this method will call the GetChat() method in WorkHorse class
        // and will populate the chat list box with the results
        private async void GetChat()
        {
            await WorkHorse.GetChat();

            FillChatBox();
        }

        private async void PostChat(string message)
        {
            await WorkHorse.PostChatAsync(MainUser.UserName, message);

            FillChatBox();
        }

        private void FillChatBox()
        {
            lb_chat.Items.Clear();

            foreach (var chat in WorkHorse.ChatList)
            {
                string sender = chat.Fields.Username;
                string message = chat.Fields.Message;

                // check if the sender is blocked, if so; block the message
                if (WorkHorse.IsBlocked(MainUser.UserName, sender))
                    message = "*** blocked ***";

                lb_chat.Items.Add(sender + ": " + message);
            }
        }

        // when user clicks login button
        private void Btn_sign_in_Click(object sender, RoutedEventArgs e)
        {
            // get the text from user name field, trim it
            string name = txt_sign_in.Text.Trim();

            // check if it is non-empty
            if (name.Length > 0)
            {
                MainUser = new User(name);

                GetChat();

                // reveal chat grid
                ChangeGrid("chat");


            }
        }

        // when user presses enter at login field
        private void Txt_sign_in_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                // route to the btn click method
                Btn_sign_in_Click(sender, new RoutedEventArgs());
        }

        private void Btn_chat_Click(object sender, RoutedEventArgs e)
        {
            if (!UserLoginCheck())
                return;

            ChangeGrid("chat");
            FillChatBox();
        }

        private void Btn_direct_message_Click(object sender, RoutedEventArgs e)
        {
            if (!UserLoginCheck())
                return;

            ChangeGrid("dm1");
        }

        private void Btn_search_Click(object sender, RoutedEventArgs e)
        {
            ChangeGrid("search");
        }

        private void Btn_block_Click(object sender, RoutedEventArgs e)
        {
            if (!UserLoginCheck())
                return;

            ChangeGrid("block");
            ShowBlockedList();


        }

        // this is the common method for changing the active grid
        private void ChangeGrid(string grid)
        {
            // firstly, hide all the grids
            grid_block.Visibility = Visibility.Hidden;
            grid_chat.Visibility = Visibility.Hidden;
            grid_direct_message1.Visibility = Visibility.Hidden;
            grid_direct_message2.Visibility = Visibility.Hidden;
            grid_search.Visibility = Visibility.Hidden;
            grid_sign_in.Visibility = Visibility.Hidden;

            // next, reveal only the chosen grid
            switch (grid)
            {
                case "block":
                    grid_block.Visibility = Visibility.Visible;
                    break;
                case "chat":
                    grid_chat.Visibility = Visibility.Visible;
                    break;
                case "dm1":
                    grid_direct_message1.Visibility = Visibility.Visible;
                    break;
                case "dm2":
                    grid_direct_message2.Visibility = Visibility.Visible;
                    break;
                case "search":
                    grid_search.Visibility = Visibility.Visible;
                    break;
                case "signin":
                    grid_sign_in.Visibility = Visibility.Visible;
                    break;
            }
        }

        // this method will check whether the user name is set or not
        // so that the application can be run as the specific user
        private bool UserLoginCheck()
        {
            if (MainUser != null)
                return true;

            MessageBox.Show("Firstly, please set your user name!");
            ChangeGrid("signin");
            return false;
        }

        private void Btn_block_send_Click(object sender, RoutedEventArgs e)
        {
            string user = txt_block.Text.Trim();

            if (user.Length > 0)
            {
                // prevent self blocking
                if (user.Equals(MainUser.UserName))
                {
                    MessageBox.Show("Sorry! You are not allowed to block yourself!");
                    txt_block.Text = "";
                    return;
                }

                WorkHorse.BlockUser(MainUser.UserName, user);
                txt_block.Text = "";
                ShowBlockedList();
            }
        }

        private void Txt_block_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Btn_block_send_Click(sender, new RoutedEventArgs());
            }
        }

        private void ShowBlockedList()
        {
            List<string> blockedList = WorkHorse.GetBlockedUsers(MainUser.UserName);

            lb_block.Items.Clear();

            if (blockedList != null && blockedList.Count > 0)
            {
                foreach (string s in blockedList)
                {
                    lb_block.Items.Add(s);
                }
            }

        }

        private void Btn_remove_block_Click(object sender, RoutedEventArgs e)
        {
            // get the selected user
            if (lb_block.SelectedItem == null)
                return;

            string removeUser = lb_block.SelectedItem.ToString();

            if (removeUser != null)
            {
                WorkHorse.UnblockUser(MainUser.UserName, removeUser);

                ShowBlockedList();
            }
        }

        // when user click send button to send message
        private void Btn_chat_send_Click(object sender, RoutedEventArgs e)
        {
            string message = txt_chat.Text.Trim();

            if (message.Length > 0)
            {
                PostChat(message);
                txt_chat.Text = "";
            }
        }
        // when user presses enter to send message
        private void Txt_chat_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Btn_chat_send_Click(sender, new RoutedEventArgs());
            }
        }
    }
}
