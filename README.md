# streamdeck-stopwatch
A C# Stopwatch implementation for the Elgato Stream Deck device.

**Author's website and contact information:** [https://barraider.github.io](https://barraider.github.io)

## New in v1.8

* New milliseconds support for greater accuracy
* Choose from 10 different `time formats` to display as much or as less detail on the key as needed. 🔥
* New `File Title Prefix` setting allows to set chosen text before the time when saved to file (great to show some text before the time on stream!)

## New in v1.7
*Our faithful Stopwatch plugin coming back with a much needed overhaul:*
- Visual indication if the stopwatch is running or stopped
- User-Customizable images for Enabled/Paused states
- New `Shared Id` feature allows you to view and control the same stopwatch from different profiles and keys
- Multi-Action support (works great with the use of Shared Ids)
- Backend logic improved to keep much more accurate time keeping when running for very long periods of times

## Current functionality
- New: Long press (2 seconds) on the key to reset the stopwatch
- Multiple Stopwatches suppport
- Stopwatches now continue to count even when moving between Stream Deck profiles. 
- Stopping/Starting a watch
- Option to not reset the watch after it's restarted
- Two display styles
- `Shared Id` feature allows you to view and control the same stopwatch from different profiles and keys

## Feature roadmap
Always open to more suggestions.

## How do I get started using it?
Install by clicking the com.barraider.stopwatch.streamDeckPlugin file in the Releases folder:
https://github.com/BarRaider/streamdeck-stopwatch/releases

## I found a bug, who do I contact?
For support please contact the developer. Contact information is available at https://barraider.github.io

## I have a feature request, who do I contact?
Please contact the developer. Contact information is available at https://barraider.github.io

## Dependencies
* Uses StreamDeck-Tools by BarRaider: [![NuGet](https://img.shields.io/nuget/v/streamdeck-tools.svg?style=flat)](https://www.nuget.org/packages/streamdeck-tools)

* Uses [Easy-PI](https://github.com/BarRaider/streamdeck-easypi) by BarRaider - Provides seamless integration with the Stream Deck PI (Property Inspector)

## Change Log


## New in v1.6
- `Clear file on reset` mode is awesome for making the text "Disappear" from the stream on reset
- :new: `Lap Mode` - Every press records the timestamp when pressed. A long press will reset the timer and copy all the laps to the clipboard (`Ctrl-V` to see them)

## New in v1.5
- Time is written to a file of your choice so you can display the elapsed time on your stream
- File is now created immediately when you press the "Save" button
