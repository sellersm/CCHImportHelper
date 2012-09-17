using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OCM.StringExtensions
{
	public static class StringExtensions
	{
		public static string Left (this String str, int strLength)
		{
			string leftString = "";

			if (!(str ==null))
			{
				if (str.Length <= strLength)
				{
					leftString = str;
				}
				else
				{
					leftString = str.Substring (0, strLength);
				}

			}
			return leftString;
		}

		public static string OtherDescription(this String str, bool otherChecked)
		{
			string description = "";

			if (otherChecked)
			{
				if (!(str == null))
				{
					description = str;
				}
			}

			return description;
		}
	}   
}
