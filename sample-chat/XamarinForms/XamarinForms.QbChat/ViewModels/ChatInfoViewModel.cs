﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Quickblox.Sdk.Modules.UsersModule.Models;
using Xamarin.Forms;
using QbChat.Pcl.Repository;

namespace XamarinForms.QbChat.ViewModels
{
    public class ChatInfoViewModel : ViewModel
    {
        private string dialogId;
		private bool isLeaveStarted; 

        public ChatInfoViewModel(string dialogId)
        {
            this.dialogId = dialogId;
            this.LeaveChatCommand = new Command(LeaveChatCommandExecute);
            this.AddOccupantsCommand = new Command(this.AddOccupantsCommandExecute);
        }

        private List<User> users;
        private bool isLoading;

        public List<User> Users
        {
            get { return this.users; }
            set
            {
                this.users = value; 
                this.RaisePropertyChanged();
            }
        }

        public ICommand LeaveChatCommand { get; private set; }

        public ICommand AddOccupantsCommand { get; private set; }

        public override void OnAppearing()
        {
            base.OnAppearing();

            if (isLoading)
                return;

            this.isLoading = true;

            this.IsBusyIndicatorVisible = true;
            
            Task.Factory.StartNew(async () => {
                var dialog = Database.Instance().GetDialog(this.dialogId);

                var users = await App.QbProvider.GetUsersByIdsAsync(dialog.OccupantIds);
                Device.BeginInvokeOnMainThread(() => {
                    Users = users;
                    this.IsBusyIndicatorVisible = false;
                });
            });
        }

        private async void LeaveChatCommandExecute(object obj)
        {
			if (this.isLeaveStarted)
			{
				return;
			}

			this.isLeaveStarted = true;

            this.IsBusyIndicatorVisible = true;

            var result = await App.Current.MainPage.DisplayAlert("Leave Chat", "Do you really want to Leave Chat?", "Yes", "Cancel");
			if (result)
			{
				try
				{
					var dialogInfo = await App.QbProvider.GetDialogAsync(this.dialogId);
					if (dialogInfo != null)
					{
						dialogInfo.OccupantsIds.Remove(App.QbProvider.UserId);

						var groupManager = App.QbProvider.GetXmppClient().GetGroupChatManager(dialogInfo.XmppRoomJid, dialogInfo.Id);
						groupManager.NotifyAboutGroupUpdate(new List<int>(), new List<int>() { App.QbProvider.UserId }, dialogInfo);
						groupManager.LeaveGroup(App.QbProvider.UserId.ToString());

					 	await App.QbProvider.DeleteDialogAsync(dialogInfo.Id);
						Database.Instance().DeleteDialog(dialogId);

						await App.Navigation.PopToRootAsync();
					}
					else {
						this.isLeaveStarted = false;
					}
				}
				catch (Exception ex)
				{
					this.isLeaveStarted = false;
				}
			}
			else {
				this.isLeaveStarted = false;
			}

            this.IsBusyIndicatorVisible = false;
        }

        private void AddOccupantsCommandExecute(object obj)
        {
            App.Navigation.PushAsync(new AddOccupantsIdsPage(this.dialogId));
        }
    }
}
