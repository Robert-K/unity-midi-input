# Unity MIDI Input
Allows Unity to receive MIDI Input using winmm.dll. Includes a very basic synthesizer.

## Features
- Only one script needed to enable MIDI input
- Parses incoming MIDI messages and makes them easier to evaluate
- Supports multiple devices simultaneously and differentiates them
- Comes with a simple synthesizer script (only for demonstration purposes, it regularly throws exceptions and produces some nasty crackling noise)
- winmm.dll is documented in the [Windows Dev Center](https://msdn.microsoft.com/en-us/library/windows/desktop/dd757277(v=vs.85).aspx) and easy to understand

## System Requirements
Uses Windows' native winmm.dll and thus <b>only runs on Windows</b>.

Made with Unity 2017.3. Might work with older versions.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
