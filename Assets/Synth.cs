using UnityEngine;
using System;
using System.Collections.Generic;
using Midi;

[RequireComponent(typeof(AudioSource))]
public class Synth : MonoBehaviour
{
    private struct Note
    {
        public int note;
        public double phase;
        public int velocity;

        public Note(int note, int velocity)
        {
            this.note = note;
            phase = 0L;
            this.velocity = velocity;
        }
    }

    private List<Note> notes = new List<Note>();

    public enum Waveform
    {
        Sine,
        Square,
        Triangle
    }

    [SerializeField]
    private Waveform waveform;

    [SerializeField]
    private double gain = 0.75;

    private double pitchBend;

    private double sampleRate;

    void OnEnable()
    {
        sampleRate = AudioSettings.outputSampleRate;

        MidiInput.logInput = true;

        Debug.Log(MidiInput.GetDeviceCount() + " device(s) found. Trying to use device 0.");

        MidiInput.OnMidiInput += ProcessInput;
        MidiInput.AddDevice(0);
    }

    void OnDisable()
    {
        MidiInput.OnMidiInput -= ProcessInput;
        MidiInput.RemoveDevice(0);
    }

    private void ProcessInput(MidiInput.Function function, int data1, int data2)
    {
        switch (function)
        {
            case MidiInput.Function.NoteOff:
                for (int i = notes.Count - 1; i >= 0; i--)
                {
                    if (notes[i].note == data1) notes.RemoveAt(i);
                }
                break;
            case MidiInput.Function.NoteOn:
                notes.Add(new Note(data1, data2));
                break;
            case MidiInput.Function.PitchBendChange:
                pitchBend = (double)data2 / 128L * 4 - 2;
                break;
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (notes.Count == 0)
            return;

        for (var i = 0; i < data.Length; i += channels)
        {
            double amplitude = 0;

            for (int j = 0; j < notes.Count; j++)
            {
                Note current = notes[j];
                double frequency = 16.35 * Math.Pow(1.059463094359, current.note + pitchBend);
                current.phase += frequency * 2 * Math.PI / sampleRate;
                switch (waveform)
                {
                    case Waveform.Sine:
                        amplitude += Math.Sin(notes[j].phase) * notes[j].velocity / 128L;
                        break;
                    case Waveform.Square:
                        amplitude += Math.Round(Math.Sin(notes[j].phase)) * notes[j].velocity / 128L;
                        break;
                    case Waveform.Triangle:
                        amplitude += Mathf.PingPong((float)notes[j].phase, 1f) * notes[j].velocity / 128L;
                        break;
                }
                if (current.phase > 2 * Math.PI) current.phase -= 2 * Math.PI;
                notes[j] = current;
            }

            amplitude *= gain / notes.Count;

            for (int j = 0; j < channels; j++)
            {
                data[i + j] = (float)amplitude;
            }
        }
    }
}
