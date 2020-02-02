using System;
using System.Collections.Generic;
using UnityEngine;

namespace Temtem_Mod.Console
{
    public struct ConCommandArgs
    {
        public string commandName;
        public List<string> userArgs;
        public string this[int i]
        {
            get
            {
                return this.userArgs[i];
            }
        }
        public int Count
        {
            get
            {
                return this.userArgs.Count;
            }
        }
        public string TryGetArgString(int index)
        {
            if (index < this.userArgs.Count)
            {
                return this.userArgs[index];
            }
            return null;
        }
        public string GetArgString(int index)
        {
            string text = this.TryGetArgString(index);
            if (text == null)
            {
                throw new ConCommandException(string.Format("Argument {0} must be a string.", index));
            }
            return text;
        }
        public ulong? TryGetArgUlong(int index)
        {
            ulong value;
            if (index < this.userArgs.Count && TextSerialization.TryParseInvariant(this.userArgs[index], out value))
            {
                return new ulong?(value);
            }
            return null;
        }
        public ulong GetArgULong(int index)
        {
            ulong? num = this.TryGetArgUlong(index);
            if (num == null)
            {
                throw new ConCommandException(string.Format("Argument {0} must be an unsigned integer.", index));
            }
            return num.Value;
        }
        public int? TryGetArgInt(int index)
        {
            int value;
            if (index < this.userArgs.Count && TextSerialization.TryParseInvariant(this.userArgs[index], out value))
            {
                return new int?(value);
            }
            return null;
        }
        public int GetArgInt(int index)
        {
            int? num = this.TryGetArgInt(index);
            if (num == null)
            {
                throw new ConCommandException(string.Format("Argument {0} must be an integer.", index));
            }
            return num.Value;
        }
        public bool? TryGetArgBool(int index)
        {
            int? num = this.TryGetArgInt(index);
            if (num != null)
            {
                int? num2 = num;
                int num3 = 0;
                return new bool?(num2.GetValueOrDefault() > num3 & num2 != null);
            }
            return null;
        }
        public bool GetArgBool(int index)
        {
            int? num = this.TryGetArgInt(index);
            if (num == null)
            {
                throw new ConCommandException(string.Format("Argument {0} must be a boolean.", index));
            }
            return num.Value > 0;
        }
        public float? TryGetArgFloat(int index)
        {
            float value;
            if (index < this.userArgs.Count && TextSerialization.TryParseInvariant(this.userArgs[index], out value))
            {
                return new float?(value);
            }
            return null;
        }
        public float GetArgFloat(int index)
        {
            float? num = this.TryGetArgFloat(index);
            if (num == null)
            {
                throw new ConCommandException(string.Format("Argument {0} must be a number.", index));
            }
            return num.Value;
        }
        public double? TryGetArgDouble(int index)
        {
            double value;
            if (index < this.userArgs.Count && TextSerialization.TryParseInvariant(this.userArgs[index], out value))
            {
                return new double?(value);
            }
            return null;
        }
        public double GetArgDouble(int index)
        {
            double? num = this.TryGetArgDouble(index);
            if (num == null)
            {
                throw new ConCommandException(string.Format("Argument {0} must be a number.", index));
            }
            return num.Value;
        }
        public T? TryGetArgEnum<T>(int index) where T : struct
        {
            T value;
            if (index < this.userArgs.Count && Enum.TryParse<T>(this.userArgs[index], true, out value))
            {
                return new T?(value);
            }
            return null;
        }
        public T GetArgEnum<T>(int index) where T : struct
        {
            T? t = this.TryGetArgEnum<T>(index);
            if (t == null)
            {
                throw new ConCommandException(string.Format("Argument {0} must be one of the values of {1}.", index, typeof(T).Name));
            }
            return t.Value;
        }
        public void CheckArgumentCount(int count)
        {
            ConCommandException.CheckArgumentCount(this.userArgs, count);
        }

    }
}
