using System.Text;

namespace AlumniStaticWeb.Helpers
{
    public static class StringExtensions
    {
        public static byte[] ToBytes(this string s) => Encoding.ASCII.GetBytes(s);
    }
}