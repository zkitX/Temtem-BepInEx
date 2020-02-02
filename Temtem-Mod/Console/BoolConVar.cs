using System;

namespace Temtem_Mod.Console
{
	public class BoolConVar : BaseConVar
	{
		public bool value { get; protected set; }

		public BoolConVar(string name, ConVarFlags flags, string defaultValue, string helpText) : base(name, flags, defaultValue, helpText)
		{
		}

		public void SetBool(bool newValue)
		{
			this.value = newValue;
		}

		public override void SetString(string newValue)
		{
			int num;
			if (TextSerialization.TryParseInvariant(newValue, out num))
			{
				this.value = (num != 0);
			}
		}

		public override string GetString()
		{
			if (!this.value)
			{
				return "0";
			}
			return "1";
		}
	}
}
