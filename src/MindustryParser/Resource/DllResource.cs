using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Playground.Resource
{
    public static class DllResource
    {
        private static Assembly assembly;

        private static readonly string prefix;

        static DllResource()
        {
            assembly = typeof(DllResource).Assembly;
            prefix = $"{assembly.GetName().Name}.assets_raw";
        }

        public static IEnumerable<string> GetAvailableResources() =>
            assembly.GetManifestResourceNames().Select(n =>
            {
                n = n[(n.StartsWith(prefix) ? prefix.Length + 1 : 0)..];

                int lastDot = n.LastIndexOf('.');

                var chars = n.ToCharArray();

                for (int i = 0; i < lastDot; i++)
                {
                    if (chars[i] == '.')
                        chars[i] = '/';
                }

                return new string(chars);
            });

        public static Stream? GetStream(string name)
        {
            var split = name.Split('/');
            for (int i = 0; i < split.Length - 1; i++)
                split[i] = split[i].Replace('-', '_');

            return assembly.GetManifestResourceStream($@"{prefix}.{string.Join('.', split)}");
        }


        public static byte[]? Get(string name)
        {
            using Stream? input = GetStream(name);

            if (input == null)
                return null;

            byte[] buffer = new byte[input.Length];
            int read = input.Read(buffer, 0, buffer.Length);
            return buffer;
        }

    }
}
