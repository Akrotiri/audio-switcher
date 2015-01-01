﻿// -----------------------------------------------------------------------
// Copyright (c) David Kean.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using AudioSwitcher.Audio;
using AudioSwitcher.Presentation.CommandModel;

namespace AudioSwitcher.ApplicationModel.Commands
{
    [Command(CommandId.SetAsDefaultMultimediaDevice)]
    internal class SetAsDefaultMultimediaDeviceCommand : SetAsDefaultDeviceCommand
    {
        [ImportingConstructor]
        public SetAsDefaultMultimediaDeviceCommand(AudioDeviceManager manager)
            : base(manager, AudioDeviceRole.Console)
        {
            Text = Resources.SetAsDefaultMultimediaDevice;
            Image = Resources.DefaultMultimediaDevice;
        }
    }
}
