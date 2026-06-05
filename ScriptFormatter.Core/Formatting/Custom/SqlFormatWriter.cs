using System.IO;

namespace ScriptFormatter.Core.Formatting.Custom
{
    public sealed class SqlFormatWriter
    {
        private readonly StringWriter _writer;

        public SqlFormatWriter()
        {
            _writer = new StringWriter();
        }

        public void WriteLine(
            string text)
        {
            _writer.WriteLine(text);
        }

        public void WriteLine(
            int indentLevel,
            string text)
        {
            _writer.WriteLine(
                Indent(indentLevel) + text);
        }

        public void WriteRaw(
            string text)
        {
            _writer.Write(text);
        }

        public void WriteRawLine(
            string text)
        {
            _writer.WriteLine(text);
        }

        public string Indent(
            int indentLevel)
        {
            return new string(
                '\t',
                indentLevel);
        }

        public override string ToString()
        {
            return _writer.ToString();
        }
    }
}