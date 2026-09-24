// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.IO;
using RomTranslator.Modules.SegaSaturn.Disc;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.Disc;

public sealed class CueSheetReaderTests
{
    private static string WriteCue(string directory, string content)
    {
        string path = Path.Combine(directory, "test.cue");
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void Read_parses_a_single_data_track()
    {
        using TemporaryDirectory temp = new();
        string cuePath = WriteCue(temp.FullPath, "FILE \"test.iso\" BINARY\n  TRACK 01 MODE1/2048\n    INDEX 01 00:00:00\n");

        CueSheet sheet = CueSheetReader.Read(cuePath);

        CueTrack track = Assert.Single(sheet.Tracks);
        Assert.Equal(1, track.Number);
        Assert.Equal(CueTrackMode.Mode1Cooked, track.Mode);
        Assert.Equal(Path.Combine(temp.FullPath, "test.iso"), track.DataFilePath);
    }

    [Fact]
    public void Read_parses_raw_mode1_and_audio_tracks_across_multiple_files()
    {
        using TemporaryDirectory temp = new();
        string cuePath = WriteCue(
            temp.FullPath,
            "FILE \"data.bin\" BINARY\n"
            + "  TRACK 01 MODE1/2352\n"
            + "    INDEX 01 00:00:00\n"
            + "FILE \"track2.wav\" WAVE\n"
            + "  TRACK 02 AUDIO\n"
            + "    PREGAP 00:02:00\n"
            + "    INDEX 01 00:00:00\n");

        CueSheet sheet = CueSheetReader.Read(cuePath);

        Assert.Equal(2, sheet.Tracks.Count);
        Assert.Equal(CueTrackMode.Mode1Raw, sheet.Tracks[0].Mode);
        Assert.Equal(CueTrackMode.Audio, sheet.Tracks[1].Mode);
        Assert.Equal(Path.Combine(temp.FullPath, "track2.wav"), sheet.Tracks[1].DataFilePath);
    }

    [Fact]
    public void FirstDataTrack_skips_leading_audio_tracks()
    {
        using TemporaryDirectory temp = new();
        string cuePath = WriteCue(
            temp.FullPath,
            "FILE \"a.wav\" WAVE\n  TRACK 01 AUDIO\n    INDEX 01 00:00:00\n"
            + "FILE \"b.iso\" BINARY\n  TRACK 02 MODE1/2048\n    INDEX 01 00:00:00\n");

        CueSheet sheet = CueSheetReader.Read(cuePath);

        Assert.NotNull(sheet.FirstDataTrack);
        Assert.Equal(2, sheet.FirstDataTrack!.Number);
    }

    [Fact]
    public void Read_throws_when_no_track_is_described()
    {
        using TemporaryDirectory temp = new();
        string cuePath = WriteCue(temp.FullPath, "FILE \"test.iso\" BINARY\n");

        Assert.Throws<InvalidDataException>(() => CueSheetReader.Read(cuePath));
    }

    [Fact]
    public void Read_throws_on_an_unsupported_track_mode()
    {
        using TemporaryDirectory temp = new();
        string cuePath = WriteCue(temp.FullPath, "FILE \"test.iso\" BINARY\n  TRACK 01 MODE3/9999\n    INDEX 01 00:00:00\n");

        Assert.Throws<InvalidDataException>(() => CueSheetReader.Read(cuePath));
    }
}
