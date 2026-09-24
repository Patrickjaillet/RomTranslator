// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System.IO;
using RomTranslator.Modules.SegaSaturn.Disc;
using RomTranslator.Tests.SegaSaturn.Support;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.Disc;

public sealed class Iso9660ReaderTests
{
    private static Iso9660Reader OpenSyntheticDisc(string directory)
    {
        string cuePath = SyntheticSaturnDiscBuilder.Build(directory);
        CueSheet cueSheet = CueSheetReader.Read(cuePath);
        SectorReader sectorReader = new(cueSheet.FirstDataTrack!);
        return new Iso9660Reader(sectorReader);
    }

    [Fact]
    public void Constructor_reads_the_root_directory_from_the_primary_volume_descriptor()
    {
        using TemporaryDirectory temp = new();
        Iso9660Reader reader = OpenSyntheticDisc(temp.FullPath);

        Assert.True(reader.Root.IsDirectory);
    }

    [Fact]
    public void ListDirectory_lists_the_file_at_the_root_without_its_version_suffix()
    {
        using TemporaryDirectory temp = new();
        Iso9660Reader reader = OpenSyntheticDisc(temp.FullPath);

        var entries = reader.ListDirectory(reader.Root);

        Iso9660Entry file = Assert.Single(entries);
        Assert.Equal("0.BIN", file.Name);
        Assert.False(file.IsDirectory);
        Assert.Equal(2048, file.LengthInBytes);
    }

    [Fact]
    public void FindEntry_resolves_a_file_at_the_root_by_name()
    {
        using TemporaryDirectory temp = new();
        Iso9660Reader reader = OpenSyntheticDisc(temp.FullPath);

        Iso9660Entry? entry = reader.FindEntry("0.BIN");

        Assert.NotNull(entry);
        Assert.Equal("0.BIN", entry!.Name);
    }

    [Fact]
    public void FindEntry_returns_null_for_a_missing_file()
    {
        using TemporaryDirectory temp = new();
        Iso9660Reader reader = OpenSyntheticDisc(temp.FullPath);

        Assert.Null(reader.FindEntry("MISSING.BIN"));
    }

    [Fact]
    public void Constructor_throws_when_the_volume_descriptor_signature_is_absent()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.FullPath, "empty.iso");
        File.WriteAllBytes(path, new byte[2048 * 20]);
        File.WriteAllText(Path.Combine(temp.FullPath, "empty.cue"), "FILE \"empty.iso\" BINARY\n  TRACK 01 MODE1/2048\n    INDEX 01 00:00:00\n");

        CueSheet cueSheet = CueSheetReader.Read(Path.Combine(temp.FullPath, "empty.cue"));
        using SectorReader sectorReader = new(cueSheet.FirstDataTrack!);

        Assert.Throws<InvalidDataException>(() => new Iso9660Reader(sectorReader));
    }
}
