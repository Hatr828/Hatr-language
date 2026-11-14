using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Best_language.src.back
{
    public struct PeImport
    {
        public int Offset;
        public string DLL;
        public string Name;
    }
    public class PeWriter
    {
        private const int fileAlignment = 0x200;
        private const int sectionAlignment = 0x1000;
        private const int headerSize = 0x200;
        private const int codeRva = 0x1000;
        private const ulong imageBase = 0x140000000;
        private const int coff = 0x84;
        private const int optional = 0x98;
        private const int section = 0x188;

        public static byte[] Build(byte[] code, IReadOnlyList<PeImport> importFixups)
        {
            int rawSize = Align(code.Length, fileAlignment);
            int importRva = Align(checked(codeRva + code.Length), sectionAlignment);

            byte[] imports = BuildImports(importFixups, importRva,
                out var addresses, out int directorySize, out int iatOffset, out int iatSize);

            bool hasImports = imports.Length != 0;

            int importRawSize = Align(imports.Length, fileAlignment);
            int importFileOffset = checked(headerSize + rawSize);
            int imageSize = hasImports ? Align(checked(importRva + importRawSize), sectionAlignment) : Align(checked(codeRva + rawSize), sectionAlignment);

            byte[] file = new byte[checked(importRawSize + importFileOffset)];

            //DOS header
            Write16(0, 0x5A4D, file);
            Write32(60, 0x80, file);

            //PE signature
            Write32(0x80, 0x00004550, file);

            //coff
            Write16(coff, 0x8664, file); // Machine
            Write16(coff + 2, (ushort)(hasImports ? 2 : 1), file);  // NumberOfSections
            Write16(coff + 16, 0xF0, file); // SizeOfOptionalHeader
            Write16(coff + 18, 0x23, file); // Characteristics

            //optional
            Write16(optional, 0x20B, file); // PE32+ (Magic)
            Write32(optional + 4, (uint)rawSize, file); // SizeOfCode
            Write32(optional + 8, (uint)importRawSize, file); // SizeOfInitializedData
            Write32(optional + 16, codeRva, file); // AddressOfEntryPoint
            Write32(optional + 20, codeRva, file);  // BaseOfCode
            Write64(optional + 24, imageBase, file); // ImageBase

            Write32(optional + 32, sectionAlignment, file); // SectionAlignment
            Write32(optional + 36, fileAlignment, file); // FileAlignment

            Write16(optional + 40, 6, file); // MajorOperatingSystemVersion
            Write16(optional + 48, 6, file); // MajorSubsystemVersion

            Write32(optional + 56, (uint)imageSize, file); // SizeOfImage
            Write32(optional + 60, headerSize, file); // SizeOfHeaders

            Write16(optional + 68, 3, file); // Subsystem
            Write16(optional + 70, 0x100, file); // DllCharacteristics

            Write64(optional + 72, 0x100000, file); // SizeOfStackReserve
            Write64(optional + 80, 0x1000, file); // SizeOfStackCommit
            Write64(optional + 88, 0x100000, file); // SizeOfHeapReserve
            Write64(optional + 96, 0x1000, file); // SizeOfHeapCommit

            Write32(optional + 108, 16, file); // NumberOfRvaAndSizes

            if(hasImports)
            {
                //Import
                Write32(optional + 120, (uint)importRva, file);
                Write32(optional + 124, (uint)directorySize, file);

                //IAT 
                Write32(optional + 208, (uint)(importRva + iatOffset), file);
                Write32(optional + 212, (uint)iatSize, file);
            }

            WriteSection(file, section, ".text", code.Length, codeRva, rawSize, headerSize, 0x60000020);

            if (hasImports)
            {
                WriteSection(file, section + 40, ".idata", imports.Length, importRva, importRawSize, importFileOffset, 0xC0000040);
            }

            code.CopyTo(file, headerSize);
            imports.CopyTo(file, importFileOffset);

            foreach (PeImport fixup in  importFixups)
            {
                uint targetRva = addresses[fixup];
                long nextInstructionRva = (long)codeRva + fixup.Offset + 4;
                int displacement = checked((int)(targetRva - nextInstructionRva));

                BinaryPrimitives.WriteInt32LittleEndian(file.AsSpan(headerSize + fixup.Offset, 4), displacement);
            }

            return file;
        }

        public static byte[] BuildImports(IReadOnlyList<PeImport> fixups, int importRva,
            out Dictionary<PeImport, uint> addresses, 
            out int directorySize, out int iatOffset, out int iatSize)
        {
            addresses = new();
            directorySize = 0;
            iatOffset = 0;
            iatSize = 0;

            if (fixups.Count == 0) return [];

            var imports = fixups.GroupBy(x => x.DLL, StringComparer.OrdinalIgnoreCase)
                .Select(import => (DLL: import.Key, Names: import.Select(x => x.Name).Distinct().ToArray()))
                .ToArray();

            List<byte> data = new();

            directorySize = checked((imports.Length + 1) * 20);
            Reserve(directorySize, data);
            Reserve(Align(data.Count, 8) - data.Count, data);

            int[] iltOffsets = new int[imports.Length];
            int[] iatOffsets = new int[imports.Length];
            int[] dllOffsets = new int[imports.Length];
            int[][] namesOffsets = new int[imports.Length][];

            for (int i = 0; i < imports.Length; i++)
                iltOffsets[i] = Reserve(checked((imports[i].Names.Length + 1) * 8), data);
            
            iatOffset = data.Count;

            for (int i = 0; i < imports.Length; i++)
                iatOffsets[i] = Reserve(checked((imports[i].Names.Length + 1) * 8), data);
            iatSize = data.Count - iatOffset;

            for (int i = 0; i < imports.Length; i++)
            {
                namesOffsets[i] = new int[imports[i].Names.Length];

                for (int j = 0; j < imports[i].Names.Length; j++)
                {
                    Reserve(Align(data.Count, 2) - data.Count, data);
                    namesOffsets[i][j] = data.Count;

                    Reserve(2, data);
                    AddString(imports[i].Names[j], data);
                }

                dllOffsets[i] = AddString(imports[i].DLL, data);
            }

            byte[] res = data.ToArray();

            Dictionary<(string DLL, string Name), uint> symbols = new();

            for (int i = 0; i < imports.Length; i++)
            {
                int descriptor = i * 20;

                Write32(descriptor, checked((uint)(importRva + iltOffsets[i])), res);
                Write32(descriptor + 12, checked((uint)(importRva + dllOffsets[i])), res);
                Write32(descriptor + 16, checked((uint)(importRva + iatOffsets[i])), res);

                for (int j = 0; j < imports[i].Names.Length; j++)
                {
                    uint nameRva = checked((uint)(importRva + namesOffsets[i][j]));

                    int iltEntry = iltOffsets[i] + j * 8;
                    int iatEntry = iatOffsets[i] + j * 8;

                    Write64(iltEntry, nameRva, res);
                    Write64(iatEntry, nameRva, res);

                    var key = (imports[i].DLL.ToUpperInvariant(), imports[i].Names[j]);
                    symbols.Add(key, checked((uint)(importRva + iatEntry)));
                }
            }

            foreach(PeImport fixup in fixups)
            {
                var key = (fixup.DLL.ToUpperInvariant(), fixup.Name);
                addresses[fixup] = symbols[key];
            }

            return res;
        }

        private static int Reserve(int size, List<byte> list)
        {
            int offset = list.Count;   

            CollectionsMarshal.SetCount(list, checked(offset + size));
            CollectionsMarshal.AsSpan(list).Slice(offset, size).Clear();

            return offset;
        }
        private static int AddString(string val, List<byte> list)
        {
            int offset = list.Count;

            list.AddRange(Encoding.ASCII.GetBytes(val));
            list.Add(0);

            return offset;
        }

        private static void WriteSection(byte[] file, int offset, string name, int virtualSize, 
            int rva, int rawSize, int rawOffset, uint characteristics)
        {
            Encoding.ASCII.GetBytes(name).CopyTo(file, offset);

            Write32(offset + 8, (uint)virtualSize, file);
            Write32(offset + 12, (uint)rva, file);
            Write32(offset + 16, (uint)rawSize, file);
            Write32(offset + 20, (uint)rawOffset, file);
            Write32(offset + 36, characteristics, file);
        }

        private static void Write16(int offset, ushort value, byte[] arr) =>
            BinaryPrimitives.WriteUInt16LittleEndian(arr.AsSpan(offset, 2), value);
        private static void Write32(int offset, uint value, byte[] arr) =>
           BinaryPrimitives.WriteUInt32LittleEndian(arr.AsSpan(offset, 4), value);
        private static void Write64(int offset, ulong value, byte[] arr) =>
           BinaryPrimitives.WriteUInt64LittleEndian(arr.AsSpan(offset, 8), value);

        private static int Align(int value, int alignment) => checked((value + alignment - 1) & -alignment);
    }
} 