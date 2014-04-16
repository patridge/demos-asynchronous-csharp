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
using System.Threading;
using System.Net;
using System.IO;

namespace AsyncWpf {
    public partial class MainWindow : Window {
        private void SynchronousButton_Click(object sender, RoutedEventArgs e) {
            // Locks up the UI until result comes back.
            SynchronousButton.Content = SlowService.GetNextNumber();
        }

        // Various ways that don't fix the problem using Tasks.
        private void AsyncFailButton_Click(object sender, RoutedEventArgs e) {
            #region "fail1"
            //Task nextNumberTask = Task.Run(() => {
            //    AsyncFailButton.Content = SlowService.GetNextNumber(); // BOOM!
            //    // Needed to get back to UI thread.
            //});
            #endregion

            #region "fail2"
            //int nextNumber = 0;
            //Task nextNumberTask = Task.Run(() => {
            //    nextNumber = SlowService.GetNextNumber();
            //});
            //AsyncFailButton.Content = nextNumber;
            //// Came back from task creation without waiting on a result.
            #endregion

            #region "fail3"
            //Task nextNumberTask = Task.Run(() => {
            //    Dispatcher.Invoke(() => {
            //        AsyncFailButton.Content = SlowService.GetNextNumber();
            //    });
            //});
            //// Came back to UI thread before heavy lifting was actually done.
            #endregion
        }

        // Various ways that do fix the problem using Tasks.
        private void AsyncWorkingButton_Click(object sender, RoutedEventArgs e) {
            #region "success1"
            Task nextNumberTask = Task.Run(() => {
                // Do the long stuff off-thread.
                int nextNumber = SlowService.GetNextNumber();
                Dispatcher.Invoke(() => {
                    // Come back to UI thread to update.
                    AsyncFailButton.Content = nextNumber;
                });
            });
            #endregion

            #region "success2"
            //Task.Run(() => {
            //    // Do the long stuff off-thread.
            //    return SlowService.GetNextNumber();
            //}).ContinueWith((task) => {
            //    // Attach a continuation to handle task completion.
            //    int nextNumber = task.Result;
            //    Dispatcher.Invoke(() => {
            //        AsyncFailButton.Content = nextNumber;
            //    });
            //});
            #endregion
        }

        private async void AsyncAwaitButton_Click(object sender, RoutedEventArgs e) {
            // Mark method async.
            // Await on any Task-returning method.
            // Complexities done in compiler.
            int nextNumber = await SlowService.GetNextNumberAsync();
            AsyncWorkingButton.Content = nextNumber;
        }

        private void SyncErrorButton_Click(object sender, RoutedEventArgs e) {
            try {
                SlowService.Fail();
            }
            catch (NotImplementedException notImplEx) {
                MessageBox.Show(notImplEx.Message);
            }
        }

        private void AsyncErrorFailButton_Click(object sender, RoutedEventArgs e) {
            Task nextNumberTask = Task.Run(() => {
                // Do the long stuff off-thread.
                int nextNumber = SlowService.Fail();
                Dispatcher.Invoke(() => {
                    // Come back to UI thread to update.
                    AsyncErrorFailButton.Content = nextNumber;
                });
            });
        }

        private void AsyncErrorWorkingButton_Click(object sender, RoutedEventArgs e) {
            #region success1
            //Task.Run(() => {
            //    return SlowService.Fail();
            //}).ContinueWith((task) => {
            //    // Check if anything went wrong.
            //    if (task.IsFaulted) {
            //        // Could be a whole bunch of exceptions collected together.
            //        AggregateException aggrEx = task.Exception;
            //        // This will just be the message for an AggregateException ("One or more...").
            //        MessageBox.Show(aggrEx.Message);
            //        // First exceptions message (not robust, but it works here).
            //        MessageBox.Show(aggrEx.InnerExceptions[0].Message);
            //        // Sample of slapping everything together.
            //        MessageBox.Show(string.Join(Environment.NewLine, (new[] { aggrEx.Message }).Concat(aggrEx.InnerExceptions.Select(ex => ex.Message))));
            //    }
            //    else {
            //        int nextNumber = task.Result;
            //        Dispatcher.Invoke(() => {
            //            // Come back to UI thread to update.
            //            AsyncErrorWorkingButton.Content = nextNumber;
            //        });
            //    }
            //});
            #endregion

            #region success2
            //Task.Run(() => {
            //    // Do the long stuff off-thread.
            //    return SlowService.Fail();
            //}).ContinueWith((task) => {
            //    // Could be a whole bunch of exceptions collected together.
            //    AggregateException aggrEx = task.Exception;
            //    // This will just be the message for an AggregateException ("One or more...").
            //    MessageBox.Show(aggrEx.Message);
            //    // First exceptions message (not robust, but it works here).
            //    MessageBox.Show(aggrEx.InnerExceptions[0].Message);
            //    // Sample of slapping everything together.
            //    MessageBox.Show(string.Join(Environment.NewLine, (new[] { aggrEx.Message }).Concat(aggrEx.InnerExceptions.Select(ex => ex.Message))));
            //    // Continuation option to make this run only for failures.
            //}, TaskContinuationOptions.OnlyOnFaulted);
            #endregion
        }

        private async void AsyncAwaitErrorButton_Click(object sender, RoutedEventArgs e) {
            try {
                // Flattens and allows for traditional try/catch.
                int nextNumber = await SlowService.FailAsync();
                AsyncAwaitButton.Content = nextNumber;
            }
            catch (NotImplementedException notImplEx) {
                // async/await doesn't wrap exceptions in an AggregateException.
                MessageBox.Show(notImplEx.Message);
            }
        }

        const string randomApiRequest = "http://api.sierratradingpost.com/api/1.0/products/?api_key=xe5s5rpag9y4ear9bwxsu48e&perPage=1";
        // Convert from BeginX/EndX to Task-based for async use.
        private async void AsyncLegacy_Click(object sender, RoutedEventArgs e) {
            WebRequest request = WebRequest.Create(randomApiRequest);
            Task<WebResponse> task = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse,
                                                                         request.EndGetResponse,
                                                                         null);
            var getWebResponse = await task.ConfigureAwait(continueOnCapturedContext: false);
            using (WebResponse webResponse = task.Result)
            using (Stream responseStream = webResponse.GetResponseStream())
            using (StreamReader reader = new StreamReader(responseStream)) {
                string result = await reader.ReadToEndAsync();
                Dispatcher.Invoke(() => {
                    // Come back to UI thread to update.
                    MessageBox.Show(result);
                });
            }
        }

        // Convert from callback-based to Task-based for async use.
        private async void AsyncCallback_Click(object sender, RoutedEventArgs e) {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            WebClient client = new WebClient();
            client.DownloadStringAsync(new Uri(randomApiRequest));
            client.DownloadStringCompleted += (downloadSender, downloadEventArgs) => {
                tcs.SetResult(downloadEventArgs.Result);
            };

            MessageBox.Show(await tcs.Task);
        }

        public MainWindow() {
            InitializeComponent();
        }
    }
}