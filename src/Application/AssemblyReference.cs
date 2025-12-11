using System.Reflection;

namespace Application;

public static class AssemblyReference
{
    public static readonly Assembly Instance = typeof(AssemblyReference).Assembly;
}
