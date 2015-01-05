﻿// -----------------------------------------------------------------------
// Copyright (c) David Kean.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AudioSwitcher.Presentation.UI.Interop;

namespace AudioSwitcher.Presentation.UI
{
    // Represents a context menu strip that adds additional behavior for audio switcher
    internal class AudioContextMenuStrip : ContextMenuStrip
    {
        private bool _autoCloseWhenItemWithDropDownClicked;

        public AudioContextMenuStrip()
        {
            Renderer = new ToolStripNativeRenderer(ToolbarTheme.Toolbar);
            ShowCheckMargin = false;
            ShowImageMargin = true;
        }

        public bool AutoCloseWhenItemWithDropDownClicked
        {
            get { return _autoCloseWhenItemWithDropDownClicked; }
            set { _autoCloseWhenItemWithDropDownClicked = value; }
        }

        public bool WorkingAreaConstrained
        {
            get;
            set;
        }

        protected override void OnOpening(CancelEventArgs e)
        {
            base.OnOpening(e);

            HideDuplicatedSeparators();
        }

        public void ShowInSystemTray(Point screenLocation)
        {
            // Prevents the context menu from causing the app to show in the taskbar
            DllImports.SetForegroundWindow(new HandleRef(this, Handle));

            if (WorkingAreaConstrained)
            {
                base.Show(screenLocation);
            }
            else
            {
                // HACK: The ContextMenuStrip does some trickery that only the NotifyIcon can call
                // that allows it to be shown outside of the working area of the desktop and over
                // the top of the taskbar. To mimic that same thing, we need to call the same method 
                // that NotifyIcon uses.
                MethodInfo info = typeof(AudioContextMenuStrip).GetMethod("ShowInTaskbar", BindingFlags.NonPublic | BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, new Type[] { typeof(int), typeof(int) }, null);
                Debug.Assert(info != null);
                info.Invoke(this, new object[] { screenLocation.X, screenLocation.Y });
            }
        }

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            try
            {
                base.OnItemClicked(e);
            }
            finally
            {
                if (AutoCloseWhenItemWithDropDownClicked)
                {
                    ToolStripDropDownItem item = e.ClickedItem as ToolStripDropDownItem;
                    if (item != null && item.DropDown.Visible)
                    {
                        Close(ToolStripDropDownCloseReason.ItemClicked);
                    }
                }
            }
        }

        protected override ToolStripItem CreateDefaultItem(string text, Image image, EventHandler onClick)
        {
            if (text == "-")
                return new ToolStripSeparator();

            AudioToolStripMenuItem item = new AudioToolStripMenuItem();
            item.Text = text;
            item.Image = image;
            item.Click += onClick;

            return item;
        }

        private void HideDuplicatedSeparators()
        {
            ToolStripItem[] availableItems = Items.Cast<ToolStripItem>()
                                                  .Where(i => i.Available)
                                                  .ToArray();

            for (int i = 0; i < availableItems.Length; i++)
            {
                ToolStripItem item = availableItems[i];

                if (i == 0 && item is ToolStripSeparator)
                    item.Available = false;

                if (i == availableItems.Length - 1 && item is ToolStripSeparator)
                    item.Available = false;
            }
        }
    }
}
