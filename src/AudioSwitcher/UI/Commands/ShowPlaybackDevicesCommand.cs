﻿// -----------------------------------------------------------------------
// Copyright (c) David Kean.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using AudioSwitcher.Presentation.CommandModel;

namespace AudioSwitcher.UI.Commands
{
    [Command(CommandId.ShowPlaybackDevices)]
    internal class ShowPlaybackDevicesCommand : Command
    {
        [ImportingConstructor]
        public ShowPlaybackDevicesCommand()
        {
            Text = Resources.ShowPlaybackDevices;
            Image = Resources.PlaybackDevice;
        }

        public override void UpdateStatus()
        {
            IsChecked = Settings.Default.ShowPlaybackDevices;
        }

        public override void Run()
        {
            Settings.Default.ShowPlaybackDevices = !Settings.Default.ShowPlaybackDevices;
        }
    }
}
