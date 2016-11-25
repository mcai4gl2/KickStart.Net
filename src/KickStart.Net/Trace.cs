using System.Runtime.CompilerServices;

namespace KickStart.Net
{
    public struct Trace
    {
        public string MemberName { get; private set; }
        public string FilePath { get; private set; }
        public int LineNumber { get; private set; }

        public static Trace Here([CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            return new Trace
            {
                MemberName = memberName,
                FilePath = filePath,
                LineNumber = lineNumber
            };
        }

        public override string ToString()
        {
            return $"[LineNumber={LineNumber}][MemberName={MemberName}][FilePath={FilePath}]";
        }
    }
}
