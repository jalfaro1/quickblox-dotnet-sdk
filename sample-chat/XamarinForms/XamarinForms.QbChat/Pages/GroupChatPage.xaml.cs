﻿using QbChat.Pcl.Repository;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinForms.QbChat.ViewModels;

namespace XamarinForms.QbChat.Pages
{
    public partial class GroupChatPage : ContentPage
	{
	    private string dialogId;
		private bool isLoaded;

		public GroupChatPage (String dialogId)
		{
			InitializeComponent();

		    this.dialogId = dialogId;
			listView.ItemTapped += (object sender, ItemTappedEventArgs e) => ((ListView)sender).SelectedItem = null;
            this.messageEntry.Focused += async (sender, args) =>
            {
                ScrollList();
            };

            this.messageEntry.TextChanged += Entry_TextChanged;
        }

        void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.Equals(e.OldTextValue, e.NewTextValue))
            {
                var newText = e.NewTextValue ?? string.Empty;
                var entry = sender as Entry;
                if (newText.Length > 1024)
                {
                    entry.Text = newText.Substring(0, 1024);
                }
                else
                {
                    entry.Text = newText;
                }
            }
        }

        protected override void OnDisappearing ()
		{
			base.OnDisappearing ();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

            if (isLoaded)
                return;

            this.isLoaded = true;

            var vm = new GroupChatViewModel(dialogId);
		    this.BindingContext = vm;
            vm.OnAppearing();
		}

		public async void ScrollList ()
		{
			var sorted = listView.ItemsSource as ObservableCollection<MessageTable>;
			try {
				if (sorted != null && sorted.Count > 1) {
                    await Task.Delay(500);
                    listView.ScrollTo (sorted [sorted.Count - 1], ScrollToPosition.End, false);
				}
			}
			catch (Exception ex) {
			}
		}
	}
}

