using System;
using UnityEngine;

namespace Temtem_Mod.Console
{
    public abstract class BaseConVar
    {
        public string helpText;
        public string defaultValue;
        public ConVarFlags flags;
        public string name;
        public abstract string GetString();
        public abstract void SetString(string newValue);
		protected BaseConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			this.name = name;
			this.flags = flags;
			this.defaultValue = defaultValue;
			if (helpText == null)
			{
				throw new ArgumentNullException("helpText");
			}
			this.helpText = helpText;
		}
		public void AttemptSetString(string newValue)
		{
			try
			{
				this.SetString(newValue);
			}
			catch (ConCommandException ex)
			{
				Debug.LogFormat("Could not set value of ConVar \"{0}\": {1}", new object[]
				{
					ex.Message
				});
			}
		}
	}
}
