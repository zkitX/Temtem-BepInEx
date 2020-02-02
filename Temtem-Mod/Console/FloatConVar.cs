using System;

namespace Temtem_Mod.Console
{
	public class FloatConVar : BaseConVar
	{
		public float value { get; protected set; }

		public FloatConVar(string name, ConVarFlags flags, string defaultValue, string helpText) : base(name, flags, defaultValue, helpText)
		{
		}
		public override void SetString(string newValue)
		{
			float num;
			if (TextSerialization.TryParseInvariant(newValue, out num) && !float.IsNaN(num) && !float.IsInfinity(num))
			{
				this.value = num;
			}
		}
		public override string GetString()
		{
			return TextSerialization.ToStringInvariant(this.value);
		}
	}
}
