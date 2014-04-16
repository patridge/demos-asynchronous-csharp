using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AsyncUser {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        List<Button> buttons;
        static List<int> combination = (new[] { 8, 6, 7, 5, 3, 0, 9, }).ToList();
        
        public MainWindow() {
            InitializeComponent();
            buttons = (new[] { btn0, btn1, btn2, btn3, btn4, btn5, btn6, btn7, btn8, btn9 }).ToList();
        }

        #region "Close"
        //Task<bool> GetButtonClickTask(int buttonNumber) {
        //    string goodButtonName = "btn" + buttonNumber;
        //    TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        //    Dispatcher.Invoke(() => {
        //        Button goodButton = buttons.First(btn => btn.Name == goodButtonName);
        //        RoutedEventHandler correctClick = null;
        //        correctClick = (s, e) => {
        //            tcs.TrySetResult(true);
        //            goodButton.Click -= correctClick;
        //        };
        //        goodButton.Click += correctClick;
        //    });
        //    return tcs.Task;
        //}
        //private async void Window_Loaded(object sender, RoutedEventArgs e) {
        //    foreach (var i in combination) {
        //        await GetButtonClickTask(i);
        //    }
        //    Unlock();
        //}
        #endregion

        #region "Better"
        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            bool unlocked = false;
            while (!unlocked) {
                unlocked = await WaitForCombination();
            }
            Unlock();
        }
        Task<bool> GetButtonClickTask(int buttonNumber) {
            string goodButtonName = "btn" + buttonNumber;
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Dispatcher.Invoke(() => {
                Button goodButton = buttons.First(btn => btn.Name == goodButtonName);
                List<Button> badButtons = buttons.Where(btn => btn.Name != goodButtonName).ToList();
                RoutedEventHandler correctClick = null;
                RoutedEventHandler incorrectClick = null;
                correctClick = (s, e) => {
                    tcs.TrySetResult(true);
                    goodButton.Click -= correctClick;
                    badButtons.ForEach(btn => btn.Click -= incorrectClick);
                };
                incorrectClick = (s, e) => {
                    tcs.TrySetResult(false);
                    goodButton.Click -= incorrectClick;
                    badButtons.ForEach(btn => btn.Click -= incorrectClick);
                };
                goodButton.Click += correctClick;
                badButtons.ForEach(btn => btn.Click += incorrectClick);
            });
            return tcs.Task;
        }
        private async Task<bool> WaitForCombination() {
            // Fake task to get the ball rolling.
            Task<bool> unlocked = PreCompletedTask<bool>(true);
            foreach (int i in combination) {
                var correct = await unlocked;
                if (correct) {
                    // Move to next entry.
                    unlocked = GetButtonClickTask(i);
                }
                else {
                    // Incorrect entry.
                    return false;
                }
            }
            // Wait for final digit task;
            await unlocked;
            return true;
        }
        #endregion

        private void Unlock() {
            foreach (var btn in buttons) {
                btn.Background = new SolidColorBrush(Colors.Green);
            }
        }
        /// <summary>
        /// Creates a task that is already completed.
        /// </summary>
        public static Task<T> PreCompletedTask<T>(T result) {
            TaskCompletionSource<T> precompletedSource = new TaskCompletionSource<T>();
            precompletedSource.SetResult(result);
            return precompletedSource.Task;
        }
    }
}