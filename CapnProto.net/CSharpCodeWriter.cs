﻿using System;
using System.IO;

namespace CapnProto
{
    public class CSharpCodeWriter : CodeWriter
    {
        public CSharpCodeWriter(TextWriter destination) : base(destination) { }
        private int indentationLevel;

        public override CodeWriter WriteLine()
        {
            base.WriteLine();
            int tmp = indentationLevel;
            while (tmp-- > 0)
            {
                Write("    ");
            }
            return this;
        }
        static string Escape(string name)
        {
            switch (name)
            {
                case "abstract":
                case "as":
                case "base":
                case "bool":
                case "break":
                case "byte":
                case "case":
                case "catch":
                case "char":
                case "checked":
                case "class":
                case "const":
                case "continue":
                case "decimal":
                case "default":
                case "delegate":
                case "do":
                case "double":
                case "else":
                case "enum":
                case "event":
                case "explicit":
                case "extern":
                case "false":
                case "finally":
                case "fixed":
                case "float":
                case "for":
                case "foreach":
                case "goto":
                case "if":
                case "implicit":
                case "in":
                case "int":
                case "interface":
                case "internal":
                case "is":
                case "lock":
                case "long":
                case "namespace":
                case "new":
                case "null":
                case "object":
                case "operator":
                case "out":
                case "override":
                case "params":
                case "private":
                case "protected":
                case "public":
                case "readonly":
                case "ref":
                case "return":
                case "sbyte":
                case "sealed":
                case "short":
                case "sizeof":
                case "stackalloc":
                case "static":
                case "string":
                case "struct":
                case "switch":
                case "this":
                case "throw":
                case "true":
                case "try":
                case "typeof":
                case "uint":
                case "ulong":
                case "unchecked":
                case "unsafe":
                case "ushort":
                case "using":
                case "virtual":
                case "void":
                case "volatile":
                case "while":
                    return "@" + name;
                default:
                    return name;
            }
        }
        public override CodeWriter BeginClass(Schema.Node node)
        {
            if (node.@struct != null)
            {
                WriteLine().Write("[global::CapnProto.Struct(0x").Write(Convert.ToString(unchecked((long)node.id), 16)).Write(")]");
            }
            WriteLine().Write("public partial class ").Write(Escape(node.displayName));
            return Indent();
        }
        static readonly char[] period = { '.' };
        public override CodeWriter BeginNamespace(string name)
        {
            WriteLine().Write("namespace ");
            var parts = name.Split(period);
            Write(Escape(parts[0]));
            for (int i = 1; i < parts.Length; i++)
            {
                Write(".").Write(Escape(parts[i]));
            }
            return Indent(); ;

        }

        public override CodeWriter BeginClass(bool @public, string name, Type baseType)
        {
            WriteLine();
            if (@public) Write("public ");
            Write("class ").Write(Escape(name));
            if(baseType != null)
            {
                Write(" : ").Write(baseType);
            }
            return Indent();
        }
        private CodeWriter Indent()
        {
            var tmp = WriteLine().Write("{");
            indentationLevel++;
            return tmp;
        }
        public override CodeWriter EndNamespace()
        {
            return Outdent();
        }
        private CodeWriter Outdent()
        {
            indentationLevel--;
            WriteLine().Write("}").WriteLine();
            return this;
        }
        public override CodeWriter EndClass()
        {
            return Outdent();
        }

        public override CodeWriter DeclareField(string name, Type type)
        {
            return WriteLine().Write("private ").Write(type).Write(" ").Write(name);
        }

        public override CodeWriter BeginOverride(System.Reflection.MethodInfo method)
        {
            WriteLine();
            if(method.IsPublic) Write("public override ");
            else if (method.IsFamilyOrAssembly) Write("protected internal override ");
            else if (method.IsFamily) Write("protected override ");
            else if (method.IsAssembly) Write("internal override ");
            Write(method.ReturnType).Write(" ").Write(method.Name).Write("(");
            var args = method.GetParameters();
            for (int i = 0; i < args.Length; i++ )
            {
                if (i != 0) Write(", ");
                Write(args[0].ParameterType).Write(" ").Write(Escape(args[0].Name));
            }
            Write(")");
            return Indent();


        }

        public override CodeWriter WriteSerializerTest(string field, Schema.Node node, string serializer)
        {
            return WriteLine().Write("if (type == typeof(").Write(node.displayName).Write(")) return ").Write(field)
                .Write(" ?? (").Write(field).Write(" = new ").Write(serializer).Write("());");
        }

        public override CodeWriter CallBase(System.Reflection.MethodInfo method)
        {
            WriteLine();
            if (method.ReturnType != null && method.ReturnType != typeof(void)) Write("return ");
            Write("base.").Write(method.Name).Write("(");
            var args = method.GetParameters();
            for(int i = 0 ; i < args.Length ; i++)
            {
                if (i != 0) Write(", ");
                Write(Escape(args[0].Name));
            }
            return Write(");");
        }

        public override CodeWriter EndOverride()
        {
            return Outdent();
        }

        public override CodeWriter Write(Type type)
        {
            if (type == null || type == typeof(Void)) return Write("void");
            return Write("global::").Write(type.FullName.Replace('+','.'));
        }

        public override CodeWriter WriteCustomSerializerClass(Schema.Node node, string baseType, string typeName, string methodName)
        {
            WriteLine().Write("class ").Write(typeName).Write(" : ").Write(baseType).Write(", global::CapnProto.ITypeSerializer<").Write(node.displayName).Write(">");
            Indent();
            
            WriteLine().Write("object global::CapnProto.ITypeSerializer.Deserialize(global::CapnProto.CapnProtoReader reader)");
            Indent().WriteLine().Write("return Deserialize(reader);");
            EndMethod();

            WriteLine().Write("public ").Write(node.displayName).Write(" Deserialize(global::CapnProto.CapnProtoReader reader)");
            Indent().WriteLine().Write("reader.ReadPreamble();").WriteLine().Write("return ").Write(methodName).Write("(1, reader, reader.ReadWord(0));");
            EndMethod();

            return EndClass();
        }


        public override CodeWriter EndMethod()
        {
            return Outdent();
        }

        public override CodeWriter WriteCustomReaderMethod(Schema.Node node, string name)
        {
            WriteLine().Write("protected ").Write(node.displayName).Write(" ").Write(name).Write("(int wordOffset, global::CapnProto.CapnProtoReader reader, ulong ptr)");


            //int maxBody = -1;
            foreach(var field in node.@struct.fields)
            {
                WriteLine().Write("// ").Write(Escape(field.name));
            }
            Indent().WriteLine().Write("throw new NotImplementedException();");

            return EndMethod();
        }
    }

}