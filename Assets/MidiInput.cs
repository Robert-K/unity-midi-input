using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Midi
{
    public static class MidiInput
    {
        public static bool logInput = false;

        public enum Function
        {
            NoteOff,
            NoteOn,
            PolyphonicAftertouch,
            ControlChange,
            ProgramChange,
            ChannelAftertouch,
            PitchBendChange,
        }

        public static event MidiInputDelegate OnMidiInput;
        public delegate void MidiInputDelegate(Function function, int data1, int data2);

        private static List<IntPtr> deviceHandles = new List<IntPtr>();

        private static string[] notes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        #region Public Methods

        public static int GetDeviceCount()
        {
            return midiInGetNumDevs();
        }

        public static void AddDevice(int deviceID)
        {
            IntPtr deviceHandle;
            if (midiInOpen(out deviceHandle, deviceID, MidiInProcCallback, IntPtr.Zero, 0x00030000) != 0)
                Debug.LogError("Could not open device " + deviceID + ".");
            else
                deviceHandles.Add(deviceHandle);

            if (midiInStart(deviceHandles[deviceID]) != 0)
                Debug.LogError("Could not start input.");
        }

        public static void RemoveDevice(int deviceID)
        {
            if (midiInStop(deviceHandles[deviceID]) != 0)
                Debug.LogError("Could not stop input.");

            if (midiInClose(deviceHandles[deviceID]) != 0)
                Debug.LogError("Could not close device " + deviceID + ".");
            deviceHandles.RemoveAt(deviceID);
        }

        #endregion

        #region Private Methods

        private static string IntToNote(int value)
        {
            return notes[value % 12] + value / 12;
        }

        private static void MidiInProcCallback(IntPtr hMidiIn, int wMsg, IntPtr dwInstance, int dwParam1, int dwParam2)
        {
            if (wMsg == 963)
            {
                int status = (byte)dwParam1;
                int data1 = (byte)(dwParam1 >> 8);
                int data2 = (byte)(dwParam1 >> 16);

                Function function = (Function)((status - 128) / 16);

                if (logInput)
                {
                    string output = function.ToString();

                    switch (function)
                    {
                        case Function.NoteOff:
                        case Function.NoteOn:
                            output += " / " + IntToNote(data1);
                            output += " / " + data2;
                            break;
                        case Function.PolyphonicAftertouch:
                            output += " / " + data1;
                            output += " / " + data2;
                            break;
                        case Function.ControlChange:
                            output += " / " + data1;
                            output += " / " + data2;
                            break;
                        case Function.ProgramChange:
                            output += " / " + data1;
                            break;
                        case Function.ChannelAftertouch:
                            output += " / " + data1;
                            break;
                        case Function.PitchBendChange:
                            output += " / " + data1;
                            output += " / " + data2;
                            break;
                    }

                    Debug.Log(output);
                }

                OnMidiInput(function, data1, data2);
            }
        }

        #endregion

        #region DLL Methods

        private delegate void MidiInProc(IntPtr hMidiIn, int wMsg, IntPtr dwInstance, int dwParam1, int dwParam2);

        [DllImport("winmm.dll")]
        private static extern int midiInGetNumDevs();

        [DllImport("winmm.dll")]
        private static extern int midiInClose(
            IntPtr hMidiIn);

        [DllImport("winmm.dll")]
        private static extern int midiInOpen(
            out IntPtr lphMidiIn,
            int uDeviceID,
            MidiInProc dwCallback,
            IntPtr dwCallbackInstance,
            int dwFlags);

        [DllImport("winmm.dll")]
        private static extern int midiInStart(
            IntPtr hMidiIn);

        [DllImport("winmm.dll")]
        private static extern int midiInStop(
            IntPtr hMidiIn);

        #endregion
    }
}