namespace JustSaying.UnitTests.TestHelpers;

/// <summary>
/// Simple replacement for Xunit's ITestOutputHelper
/// </summary>
public interface ITestOutputHelper
{
    void WriteLine(string message);
    void WriteLine(string format, params object[] args);
}

/// <summary>
/// Console-based implementation of ITestOutputHelper
/// </summary>
public class ConsoleTestOutputHelper : ITestOutputHelper
{
    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    public void WriteLine(string format, params object[] args)
    {
        Console.WriteLine(format, args);
    }
}
