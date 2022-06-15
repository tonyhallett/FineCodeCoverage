namespace FineCodeCoverage.Output.HostObjects
{
    public interface ISourceFileOpenerHostObject
    {
        void openAtLine(string filePath, int line);
        void openFiles(object[] filePaths);
    }
}
