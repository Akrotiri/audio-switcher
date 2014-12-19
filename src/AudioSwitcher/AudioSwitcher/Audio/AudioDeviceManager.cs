﻿// -----------------------------------------------------------------------
// Copyright (c) David Kean.
// -----------------------------------------------------------------------
// This source file was altered for use in AudioSwitcher.
/*
  LICENSE
  -------
  Copyright (C) 2007 Ray Molenkamp

  This source code is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this source code or the software it produces.

  Permission is granted to anyone to use this source code for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this source code must not be misrepresented; you must not
     claim that you wrote the original source code.  If you use this source code
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original source code.
  3. This notice may not be removed or altered from any source distribution.
*/
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using AudioSwitcher.Interop;

namespace AudioSwitcher.Audio
{
    [Export(typeof(AudioDeviceManager))]
    internal class AudioDeviceManager : IMMNotificationClient, IDisposable
    {
        private readonly IMMDeviceEnumerator _underlyingEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();

        public AudioDeviceManager()
        {
            int hr = _underlyingEnumerator.RegisterEndpointNotificationCallback(this);
            if (hr != HResult.OK)
                throw Marshal.GetExceptionForHR(hr);
        }

        public event EventHandler<AudioDeviceEventArgs> DeviceAdded;
        public event EventHandler<AudioDeviceRemovedEventArgs> DeviceRemoved;
        public event EventHandler<AudioDeviceEventArgs> DevicePropertyChanged;
        public event EventHandler<DefaultAudioDeviceEventArgs> DefaultDeviceChanged;
        public event EventHandler<AudioDeviceStateEventArgs> DeviceStateChanged;

        public AudioDeviceCollection GetAudioDevices(AudioDeviceKind kind, AudioDeviceState state)
        {
            IMMDeviceCollection result;
            Marshal.ThrowExceptionForHR(_underlyingEnumerator.EnumAudioEndpoints(kind, state, out result));
            return new AudioDeviceCollection(result);
        }

        public void SetDefaultAudioDevice(AudioDevice device)
        {
            SetDefaultAudioDevice(device, AudioDeviceRole.Multimedia);
            SetDefaultAudioDevice(device, AudioDeviceRole.Communications);
            SetDefaultAudioDevice(device, AudioDeviceRole.Console);
        }

        public void SetDefaultAudioDevice(AudioDevice device, AudioDeviceRole role)
        {
            if (device == null)
                throw new ArgumentNullException("device");

            // BADNESS: The following code uses an undocumented interface provided by the Audio SDK. This is completely
            // unsupported, and should be used for amusement purposes only. This is *extremely likely* to be broken 
            // in future updates and/or versions of Windows. If Larry Osterman was dead, he would be rolling over 
            // in his grave if he knew you were using this for nefarious purposes.
            IPolicyConfig config = (IPolicyConfig)new PolicyConfig();

            int hr = config.SetDefaultEndpoint(device.Id, role);
            if (hr != HResult.OK)
                throw Marshal.GetExceptionForHR(hr);
        }

        public bool IsDefaultAudioDevice(AudioDevice device, AudioDeviceRole role)
        {
            AudioDevice defaultDevice = GetDefaultAudioDevice(device.Kind, role);
            if (defaultDevice == null)
                return false;

            return String.Equals(defaultDevice.Id, device.Id, StringComparison.OrdinalIgnoreCase);
        }

        public AudioDevice GetDefaultAudioDevice(AudioDeviceKind kind, AudioDeviceRole role)
        {
            const int E_NOTFOUND = unchecked((int)0x80070490);

            IMMDevice underlyingDevice;
            int hr = _underlyingEnumerator.GetDefaultAudioEndpoint(kind, role, out underlyingDevice);
            if (hr == HResult.OK)
                return new AudioDevice(underlyingDevice);

            if (hr == E_NOTFOUND)
                return null;

            throw Marshal.GetExceptionForHR(hr);
        }

        public AudioDevice GetDevice(string id)
        {
            IMMDevice underlyingDevice;
            Marshal.ThrowExceptionForHR(_underlyingEnumerator.GetDevice(id, out underlyingDevice));

            return new AudioDevice(underlyingDevice);
        }

        void IMMNotificationClient.OnDeviceStateChanged(string deviceId, AudioDeviceState newState)
        {
            var handler = DeviceStateChanged;
            if (handler != null)
            {
                AudioDevice device = GetDevice(deviceId);
            
                handler(this, new AudioDeviceStateEventArgs(device, newState));
            }
        }

        void IMMNotificationClient.OnDeviceAdded(string deviceId)
        {
            var handler = DeviceAdded;
            if (handler != null)
            {
                AudioDevice device = GetDevice(deviceId);

                handler(this, new AudioDeviceEventArgs(device));
            }
        }

        void IMMNotificationClient.OnDeviceRemoved(string deviceId)
        {
            var handler = DeviceRemoved;
            if (handler != null)
            {
                handler(this, new AudioDeviceRemovedEventArgs(deviceId));
            }
        }

        void IMMNotificationClient.OnDefaultDeviceChanged(AudioDeviceKind kind, AudioDeviceRole role, string deviceId)
        {
            var handler = DefaultDeviceChanged;
            if (handler != null)
            {
                AudioDevice device = GetDevice(deviceId);

                handler(this, new DefaultAudioDeviceEventArgs(device, kind, role));
            }
        }

        void IMMNotificationClient.OnPropertyValueChanged(string deviceId, PropertyKey key)
        {
            var handler = DevicePropertyChanged;
            if (handler != null)
            {
                AudioDevice device = GetDevice(deviceId);

                handler(this, new AudioDeviceEventArgs(device));
            }
        }

        public void Dispose()
        {
            int hr = _underlyingEnumerator.UnregisterEndpointNotificationCallback(this);
            if (hr != HResult.OK)
                throw Marshal.GetExceptionForHR(hr);
        }
    }
}
