using System;

namespace Temtem_Mod.Console
{
	public class StringConVar : BaseConVar
	{
		public string value { get; protected set; }

		public StringConVar(string name, ConVarFlags flags, string defaultValue, string helpText) : base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			this.value = newValue;
		}

		public override string GetString()
		{
			return this.value;
		}
	}
}
