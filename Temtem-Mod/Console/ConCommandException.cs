using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Temtem_Mod.Console
{
    [Serializable]
    public class ConCommandException : Exception
    {
        public ConCommandException()
        {
        }
        public ConCommandException(string message) : base(message)
        {
        }
        public ConCommandException(string message, Exception inner) : base(message, inner)
        {
        }
        protected ConCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        public static void CheckArgumentCount(List<string> args, int requiredArgCount)
        {
            if (args.Count < requiredArgCount)
            {
                throw new ConCommandException(string.Format("{0} argument(s) required, {1} argument(s) provided.", requiredArgCount, args.Count));
            }
        }
    }
}
