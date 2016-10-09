using KickStart.Net.Extensions;

namespace KickStart.Net.Configurations
{
    public interface IConfiguration
    {
        string Environment { get; }
        string Source { get; }
        string Key { get; }
        string Value { get; }
    }

    public static class Configurations
    {
        public static string Name(string name, params string[] names)
        {
            return ".".JoinSkipNulls(name, ".".JoinSkipNulls(names));
        }
    }

    public struct Configuration : IConfiguration
    {
        public string Environment { get; set; }
        public string Source { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is IConfiguration))
                return false;
            var other = (IConfiguration) obj;
            return Objects.SafeEquals(Environment, other.Environment) &&
                   Objects.SafeEquals(Source, other.Source) &&
                   Objects.SafeEquals(Key, other.Key) &&
                   Objects.SafeEquals(Value, other.Value);
        }
    }
}
