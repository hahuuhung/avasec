using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using AVASec.Core.Interfaces;
using AVASec.Core.Models;

namespace AVASec.UI.Controls
{
    public partial class NotificationCenter : UserControl
    {
        private INotificationService _notificationService;

        public NotificationCenter()
        {
            InitializeComponent();
            
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;

            _notificationService = App.ServiceProvider.GetRequiredService<INotificationService>();
            
            // Bind List
            // We might need to marshal to UI thread if service adds from background
            // But ObservableCollection + BindingOperations.EnableCollectionSynchronization is best practice.
            // For simplicity, we just bind directly assuming simple usage or service handles it.
            NotificationList.ItemsSource = _notificationService.Notifications;
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            _notificationService.ClearAll();
        }
    }
}
