// SPDX-License-Identifier: GPL-3.0-only
// © 2026 Patrick JAILLET — RomTranslator

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RomTranslator.Core.Abstractions;
using RomTranslator.Core.Projects;
using RomTranslator.Modules.SegaSaturn.CharacterTables;
using RomTranslator.Modules.SegaSaturn.Disc;
using RomTranslator.Modules.SegaSaturn.TextExtraction;
using RomTranslator.Modules.SegaSaturn.Translation;
using RomTranslator.Tests.SegaSaturn.Support;
using RomTranslator.Tests.Support;
using Xunit;

namespace RomTranslator.Tests.SegaSaturn.Translation;

public sealed class SaturnTextInjectorTests
{
    private const long PayloadOffset = 5L * 2048;

    private static SaturnCharacterTable CreateAsciiTable()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "PredefinedTables", "ascii.tbl");
        CharacterTableFile file = CharacterTableFileReader.Read(path);
        return new SaturnCharacterTable("ASCII", file);
    }

    private static byte[] ReadLogicalBytes(string cuePath, long offset, int length)
    {
        CueSheet cueSheet = CueSheetReader.Read(cuePath);
        using SectorReader reader = new(cueSheet.FirstDataTrack!);
        long startSector = offset / SectorReader.SectorDataSize;
        int offsetInSector = (int)(offset - (startSector * SectorReader.SectorDataSize));
        byte[] sectorAligned = reader.ReadBytes(startSector, offsetInSector + length);
        byte[] result = new byte[length];
        Array.Copy(sectorAligned, offsetInSector, result, 0, length);
        return result;
    }

    [Fact]
    public void Inject_overwrites_a_source_occurrence_with_a_translation_of_equal_length()
    {
        using TemporaryDirectory temp = new();
        string sourceCue = SyntheticSaturnDiscBuilder.Build(temp.FullPath, payloadBytes: Encoding.ASCII.GetBytes("Hello"), payloadOffset: PayloadOffset);
        string outputCue = Path.Combine(temp.FullPath, "output", "test.cue");

        TranslationEntry entry = new("e1", "Hello", PayloadOffset, translatedText: "World");
        SaturnTextInjector injector = new();

        TextInjectionResult result = injector.Inject(sourceCue, outputCue, new ITranslationEntry[] { entry }, CreateAsciiTable());

        byte[] writtenBytes = ReadLogicalBytes(outputCue, PayloadOffset, 5);
        Assert.Equal("World", Encoding.ASCII.GetString(writtenBytes));
        Assert.Empty(result.Messages);
    }

    [Fact]
    public void Inject_pads_a_shorter_translation_with_spaces()
    {
        using TemporaryDirectory temp = new();
        string sourceCue = SyntheticSaturnDiscBuilder.Build(temp.FullPath, payloadBytes: Encoding.ASCII.GetBytes("Hello"), payloadOffset: PayloadOffset);
        string outputCue = Path.Combine(temp.FullPath, "output", "test.cue");

        TranslationEntry entry = new("e1", "Hello", PayloadOffset, translatedText: "Hi");
        SaturnTextInjector injector = new();

        injector.Inject(sourceCue, outputCue, new ITranslationEntry[] { entry }, CreateAsciiTable());

        byte[] result = ReadLogicalBytes(outputCue, PayloadOffset, 5);
        Assert.Equal("Hi   ", Encoding.ASCII.GetString(result));
    }

    [Fact]
    public void Inject_writes_every_occurrence_of_a_duplicated_entry()
    {
        using TemporaryDirectory temp = new();
        byte[] payload = new byte[20];
        Array.Copy(Encoding.ASCII.GetBytes("Attack"), 0, payload, 0, 6);
        Array.Copy(Encoding.ASCII.GetBytes("Attack"), 0, payload, 10, 6);
        string sourceCue = SyntheticSaturnDiscBuilder.Build(temp.FullPath, payloadBytes: payload, payloadOffset: PayloadOffset);
        string outputCue = Path.Combine(temp.FullPath, "output", "test.cue");

        TranslationEntry entry = new("e1", "Attack", new long[] { PayloadOffset, PayloadOffset + 10 }, translatedText: "Frappe");
        SaturnTextInjector injector = new();

        injector.Inject(sourceCue, outputCue, new ITranslationEntry[] { entry }, CreateAsciiTable());

        Assert.Equal("Frappe", Encoding.ASCII.GetString(ReadLogicalBytes(outputCue, PayloadOffset, 6)));
        Assert.Equal("Frappe", Encoding.ASCII.GetString(ReadLogicalBytes(outputCue, PayloadOffset + 10, 6)));
    }

    [Fact]
    public void Inject_throws_when_a_translation_is_longer_than_the_source_without_relocation()
    {
        using TemporaryDirectory temp = new();
        string sourceCue = SyntheticSaturnDiscBuilder.Build(temp.FullPath, payloadBytes: Encoding.ASCII.GetBytes("Hi"), payloadOffset: PayloadOffset);
        string outputCue = Path.Combine(temp.FullPath, "output", "test.cue");

        TranslationEntry entry = new("e1", "Hi", PayloadOffset, translatedText: "Hello there");
        SaturnTextInjector injector = new();

        Assert.Throws<InvalidOperationException>(() => injector.Inject(sourceCue, outputCue, new ITranslationEntry[] { entry }, CreateAsciiTable()));
    }

    [Fact]
    public void Inject_relocates_a_longer_translation_and_updates_its_pointer()
    {
        using TemporaryDirectory temp = new();
        long pointerOffset = 10L * 2048;
        long freeSpaceOffset = 12L * 2048;

        string sourceCue = SyntheticSaturnDiscBuilder.Build(
            temp.FullPath,
            payloadBytes: BuildPayloadWithPointer("Hi", PayloadOffset, pointerOffset),
            payloadOffset: 0);
        string outputCue = Path.Combine(temp.FullPath, "output", "test.cue");

        TranslationEntry entry = new("e1", "Hi", PayloadOffset, translatedText: "Hello there");
        PointerEncoding pointerEncoding = new(ByteWidth: 4, IsBigEndian: true, IsRelative: false, BaseOffset: 0);
        SaturnPointerRelocation relocation = new(
            pointerEncoding,
            new Dictionary<long, IReadOnlyList<long>> { [PayloadOffset] = new long[] { pointerOffset } },
            FreeSpaceOffset: freeSpaceOffset,
            FreeSpaceLength: 64);

        SaturnTextInjector injector = new();
        TextInjectionResult result = injector.Inject(sourceCue, outputCue, new ITranslationEntry[] { entry }, CreateAsciiTable(), relocation);

        byte[] relocatedText = ReadLogicalBytes(outputCue, freeSpaceOffset, "Hello there".Length);
        Assert.Equal("Hello there", Encoding.ASCII.GetString(relocatedText));

        byte[] updatedPointer = ReadLogicalBytes(outputCue, pointerOffset, 4);
        long updatedTarget = ReadUInt32BigEndian(updatedPointer);
        Assert.Equal(freeSpaceOffset, updatedTarget);

        Assert.Contains(result.Messages, message => message.Contains("Hi", StringComparison.Ordinal));
    }

    [Fact]
    public void Inject_throws_when_the_free_space_area_is_too_small()
    {
        using TemporaryDirectory temp = new();
        long pointerOffset = 10L * 2048;
        long freeSpaceOffset = 12L * 2048;

        string sourceCue = SyntheticSaturnDiscBuilder.Build(
            temp.FullPath,
            payloadBytes: BuildPayloadWithPointer("Hi", PayloadOffset, pointerOffset),
            payloadOffset: 0);
        string outputCue = Path.Combine(temp.FullPath, "output", "test.cue");

        TranslationEntry entry = new("e1", "Hi", PayloadOffset, translatedText: "Hello there");
        PointerEncoding pointerEncoding = new(ByteWidth: 4, IsBigEndian: true, IsRelative: false, BaseOffset: 0);
        SaturnPointerRelocation relocation = new(
            pointerEncoding,
            new Dictionary<long, IReadOnlyList<long>> { [PayloadOffset] = new long[] { pointerOffset } },
            FreeSpaceOffset: freeSpaceOffset,
            FreeSpaceLength: 4);

        SaturnTextInjector injector = new();

        Assert.Throws<InvalidOperationException>(() => injector.Inject(sourceCue, outputCue, new ITranslationEntry[] { entry }, CreateAsciiTable(), relocation));
    }

    private static long ReadUInt32BigEndian(byte[] bytes)
    {
        return ((long)bytes[0] << 24) | ((long)bytes[1] << 16) | ((long)bytes[2] << 8) | bytes[3];
    }

    private static void WritePointer(byte[] buffer, int index, long value)
    {
        buffer[index] = (byte)((value >> 24) & 0xFF);
        buffer[index + 1] = (byte)((value >> 16) & 0xFF);
        buffer[index + 2] = (byte)((value >> 8) & 0xFF);
        buffer[index + 3] = (byte)(value & 0xFF);
    }

    private static byte[] BuildPayloadWithPointer(string text, long textOffset, long pointerOffset)
    {
        byte[] textBytes = Encoding.ASCII.GetBytes(text);
        byte[] pointerBytes = new byte[4];
        WritePointer(pointerBytes, 0, textOffset);

        long length = Math.Max(textOffset + textBytes.Length, pointerOffset + pointerBytes.Length);
        byte[] result = new byte[length];
        Array.Copy(textBytes, 0, result, textOffset, textBytes.Length);
        Array.Copy(pointerBytes, 0, result, pointerOffset, pointerBytes.Length);
        return result;
    }
}
