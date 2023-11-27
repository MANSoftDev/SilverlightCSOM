using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.WebParts;

namespace SilverlightCSOM
{
    public partial class MainPage : UserControl
    {
        private const string SITE = "http://mySite";
        private const string PAGE = "/default.aspx";

        public MainPage()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadWebParts();
        }

        private void OnSelectWebpart(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            wpTitle.Text = e.AddedItems[0].ToString();
            SelectedItem = (ListItemHelper)e.AddedItems[0];
            SelectedItem.ItemIndex = wpList.SelectedIndex;
        }

        private void OnAdd(object sender, System.Windows.RoutedEventArgs e)
        {
            CSOMHelper.AddWebPart();
            LoadWebParts();
        }

        private void OnUpdate(object sender, System.Windows.RoutedEventArgs e)
        {
            // Get the title and zone that may have been updated
            SelectedItem.Title = wpTitle.Text;
            SelectedItem.Zone = ((ComboBoxItem)wpZone.SelectedItem).Content.ToString();

            // Start a worker thread to do the processing
            ThreadPool.QueueUserWorkItem(new WaitCallback(CSOMHelper.UpdateTitle), SelectedItem);

            // Update the controls
            wpList.Items[SelectedItem.ItemIndex] = wpTitle.Text;
            wpTitle.Text = "";
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            // Start a worker thread to do the processing
            ThreadPool.QueueUserWorkItem(new WaitCallback(CSOMHelper.DeleteWebPart), SelectedItem);

            // Update the controls
            wpTitle.Text = "";
            wpList.Items.RemoveAt(SelectedItem.ItemIndex);
            SelectedItem = null;
        }

        private void LoadWebParts()
        {
            SelectedItem = null;
            wpList.Items.Clear();
            using(ClientContext ctx = new ClientContext(SITE))
            {
                try
                {
                    // Get the default page for the site
                    File file = ctx.Web.GetFileByServerRelativeUrl(PAGE);

                    // Get the WebPart manager to locate all the WebParts
                    LimitedWebPartManager wpm = file.GetLimitedWebPartManager(PersonalizationScope.Shared);

                    // Load all WebPart definitions found on the page
                    WPDefinitions = ctx.LoadQuery(wpm.WebParts.Include(w => w.Id, w => w.WebPart));

                    ctx.ExecuteQueryAsync(OnLoadSucceeded, OnFail);
                }
                catch(System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// Delegate for ExecuteQueryAsync success 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoadSucceeded(object sender, ClientRequestSucceededEventArgs e)
        {
            // Create delegate to update ListBox in UI thread
            Action updateList = () =>
                {
                    if(WPDefinitions.Count() != 0)
                    {
                        foreach(WebPartDefinition def in WPDefinitions)
                        {
                            wpList.Items.Add(new ListItemHelper() { ID = def.Id, Title = def.WebPart.Title });
                        }
                    }
                };

            this.Dispatcher.BeginInvoke(updateList);
        }

        /// <summary>
        /// Delegate for ExecuteQueryAsync failure
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFail(object sender, ClientRequestFailedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(delegate() { MessageBox.Show(e.Message); });
        }

        #region Properties

        private IEnumerable<WebPartDefinition> WPDefinitions { get; set; }
        private ListItemHelper SelectedItem { get; set; }

        #endregion


    }
}
