using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Best_language.src.front
{
    public abstract class TypeSymbol(string name)
    {
        public string Name { get; set; } = name;
    }

    public class IntegerTypeSymbol(string name, TypeSize size, bool isSigned) : TypeSymbol(name)
    {
        public TypeSize Size { get; } = size;
        public bool IsSigned { get; } = isSigned;
        public override string ToString() => $"{nameof(IntegerTypeSymbol)} ({Name}: {Size}, {IsSigned})";
    }

    public class FloatTypeSymbol(string name, TypeSize size) : TypeSymbol(name)
    {
        public TypeSize Size { get; } = size;
        public override string ToString() => $"{nameof(FloatTypeSymbol)} ({Name}: {Size})";
    }

    public class BoolTypeSymbol() : TypeSymbol("bool")
    {
        public override string ToString() => $"{nameof(BoolTypeSymbol)} ({Name})";
    }

    public class VoidTypeSymbol() : TypeSymbol("void")
    {
        public override string ToString() => $"{nameof(VoidTypeSymbol)} ({Name})";
    }

    public class PointerTypeSymbol(TypeSymbol elementType) : TypeSymbol($"{elementType}*")
    {
        public TypeSymbol ElementType { get; set; } = elementType;
        public override string ToString() => $"{nameof(PointerTypeSymbol)} ({Name})";
    }

    public class TypedefTypeSymbol(TypeSymbol elementType) : TypeSymbol($"{elementType}*")
    {
        public TypeSymbol ElementType { get; } = elementType;
        public override string ToString() => $"{nameof(TypedefTypeSymbol)} ({Name})";
    }


    public class EnumTypeSymbol(string? name, IntegerTypeSymbol enumType) : TypeSymbol(name!)
    {
        public IntegerTypeSymbol EnumType { get; } = enumType;

        public Dictionary<string, ulong> Members { get; } = new();
        public override string ToString() => $"{nameof(EnumTypeSymbol)} ({Name ?? "No name"})";
    }
    public class FunctionTypeSymbol(TypeSymbol returnType, List<TypeSymbol>? paramsTypes = null) : TypeSymbol("function")
    {
        public TypeSymbol ReturnType { get; } = returnType;
        public List<TypeSymbol> ParamsTypes { get; } = paramsTypes ?? [];
    }

    public enum TypeSize
    {
        @byte = 1, word = 2, dword = 4, qword = 8,
    }

    public class TypeTable
    {
        public VoidTypeSymbol Void { get; } = new();
        public BoolTypeSymbol Bool { get; } = new();

        public IntegerTypeSymbol Int8 { get; } = new("i8", TypeSize.@byte, true);
        public IntegerTypeSymbol UInt8 { get; } = new("u8", TypeSize.@byte, false);
        public IntegerTypeSymbol Int16 { get; } = new("i16", TypeSize.word, true);
        public IntegerTypeSymbol UInt16 { get; } = new("u16", TypeSize.word, false);
        public IntegerTypeSymbol Int32 { get; } = new("i32", TypeSize.dword, true);
        public IntegerTypeSymbol UInt32 { get; } = new("u32", TypeSize.dword, false);
        public IntegerTypeSymbol Int64 { get; } = new("i64", TypeSize.qword, true);
        public IntegerTypeSymbol UInt64 { get; } = new("u64", TypeSize.qword, false);

        public IntegerTypeSymbol Long { get; } = new("long", TypeSize.qword, true);
        public IntegerTypeSymbol ULong { get; } = new("ulong", TypeSize.qword, false);

        public FloatTypeSymbol Float { get; } = new("float", TypeSize.dword);
        public FloatTypeSymbol Double { get; } = new("double", TypeSize.qword);

        public IntegerTypeSymbol Char { get; } = new("char", TypeSize.@byte, true);
        public IntegerTypeSymbol Enum { get; } = new("i32", TypeSize.dword, true);

        private Dictionary<TypeSymbol, PointerTypeSymbol> _pointers = new();

        public PointerTypeSymbol GetPointer(TypeSymbol elementType)
        {
            if(!_pointers.TryGetValue(elementType, out var pointerType))
            {
                pointerType = new PointerTypeSymbol(elementType);
                _pointers.Add(elementType, pointerType);
            }

            return pointerType;
        }
    }
}
