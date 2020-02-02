using System;

namespace Temtem_Mod.Console
{
	public class IntConVar : BaseConVar
	{
		public int value { get; protected set; }

		public IntConVar(string name, ConVarFlags flags, string defaultValue, string helpText) : base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			int value;
			if (TextSerialization.TryParseInvariant(newValue, out value))
			{
				this.value = value;
			}
		}

		public override string GetString()
		{
			return TextSerialization.ToStringInvariant(this.value);
		}
	}
}
