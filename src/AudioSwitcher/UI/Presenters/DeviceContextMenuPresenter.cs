﻿// -----------------------------------------------------------------------
// Copyright (c) David Kean.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using AudioSwitcher.Audio;
using AudioSwitcher.Presentation;
using AudioSwitcher.Presentation.CommandModel;
using AudioSwitcher.Presentation.UI;
using AudioSwitcher.UI.Commands;
using AudioSwitcher.UI.ViewModels;

namespace AudioSwitcher.UI.Presenters
{
    // Presents the device context menu when left-clicking on the notification icon
    [Presenter(PresenterId.DeviceContextMenu, IsToggle=true)]
    internal class DeviceContextMenuPresenter : ContextMenuPresenter, IDisposable
    {
        private readonly AudioDeviceViewModelManager _viewModelManager;
        private readonly CommandManager _commandManager;

        [ImportingConstructor]
        public DeviceContextMenuPresenter(AudioDeviceViewModelManager viewModelManager, CommandManager commandManager)
        {
            _viewModelManager = viewModelManager;
            _viewModelManager.Changed += OnViewModelsChanged;
            _commandManager = commandManager;
        }

        public override void Dispose()
        {
            base.Dispose();

            _viewModelManager.Changed -= OnViewModelsChanged;
        }

        private void OnViewModelsChanged(object sender, System.EventArgs e)
        {
            ContextMenu.RefreshCommands();
        }

        protected override void Bind()
        {
            ContextMenu.AutoCloseWhenItemWithDropDownClicked = true; // When something clicks the "Device" we auto close 
            ContextMenu.WorkingAreaConstrained = true;

            AddDeviceCommands(AudioDeviceKind.Playback, Settings.Default.ShowPlaybackDevices, Resources.NoPlaybackDevices);
            AddDeviceCommands(AudioDeviceKind.Recording, Settings.Default.ShowRecordingDevices, Resources.NoRecordingDevices);

            if (ContextMenu.Items.Count == 0)
                ContextMenu.AddDisabled(Resources.NoDevices);
        }

        private void AddDeviceCommands(AudioDeviceKind kind, bool condition, string noDeviceText)
        {
            if (condition)
            {
                ContextMenu.AddSeparatorIfNeeded();

                AudioDeviceViewModel[] devices = GetDevices(kind);
                if (devices.Length == 0)
                {
                    ContextMenu.AddDisabled(noDeviceText);
                }
                else
                {
                    AddDeviceCommands(devices);
                }
            }
        }

        private void AddDeviceCommands(AudioDeviceViewModel[] devices)
        {
            foreach (AudioDeviceViewModel device in devices)
            {
                ToolStripMenuItem menu = ContextMenu.BindCommand(_commandManager, CommandId.SetAsDefaultDevice, device);
                menu.DropDown.BindCommand(_commandManager, CommandId.SetAsDefaultMultimediaDevice, device);
                menu.DropDown.BindCommand(_commandManager, CommandId.SetAsDefaultCommunicationDevice, device);
            }
        }

        private AudioDeviceViewModel[] GetDevices(AudioDeviceKind kind)
        {
            AudioDeviceState state = AudioDeviceState.Active;
            if (Settings.Default.ShowDisabledDevices)
            {
                state |= AudioDeviceState.Disabled;
            }

            if (Settings.Default.ShowUnpluggedDevices)
            {
                state |= AudioDeviceState.Unplugged;
            }

            if (Settings.Default.ShowNotPresentDevices)
            {
                state |= AudioDeviceState.NotPresent;
            }

            return _viewModelManager.ViewModels.Where(v => v.Device.Kind == kind &&
                                                           state.HasFlag(v.State))
                                               .ToArray();
        }
    }
}
