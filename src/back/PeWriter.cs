using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src.back
{
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

        public static byte[] Build(byte[] code)
        {
            int rawSize = Align(code.Length, fileAlignment);
            int imageSize = Align(checked(codeRva + code.Length), sectionAlignment);

            byte[] file = new byte[checked(rawSize + headerSize)];

            //DOS header
            Write16(0x00, 0x5A4D, file);
            Write32(0x3C, 0x80, file);

            //PE signature
            Write32(0x80, 0x00004550, file);

            //coff
            Write16(coff, 0x8664, file); // Machine
            Write16(coff + 2, 1, file);  // NumberOfSections
            Write16(coff + 16, 0xF0, file); // SizeOfOptionalHeader
            Write16(coff + 18, 0x23, file); // Characteristics

            //optional
            Write16(optional, 0x20B, file); // PE32+ (Magic)
            Write32(optional + 4, (uint)rawSize, file); // SizeOfCode
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

            Encoding.ASCII.GetBytes(".text").CopyTo(file, section); 

            Write32(section + 8, (uint)code.Length, file); // VirtualSize
            Write32(section + 12, codeRva, file); // VirtualAddress
            Write32(section + 16, (uint)rawSize, file); // SizeOfRawData
            Write32(section + 20, headerSize, file); // PointerToRawData
            Write32(section + 36, 0x60000020, file);  // Characteristics

            code.CopyTo(file, headerSize);

            return file;
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
