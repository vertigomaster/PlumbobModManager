//Created by: Cayden Chancey
//Edited by: Julian Noel

using System;

#if ODIN_INSPECTOR
using Sirenix.Utilities;
#else
using IDEK.Tools.ShocktroopUtils.OdinFallbacks;
#endif

using IDEK.Tools.Logging;

namespace IDEK.Tools.TimeHelpers
{
	[Serializable]
	public class CalendarDate
	{
		//TODO: localization and multiple formats

		public enum Month
		{
			None = 0,
			January = 1,
			Febuary = 2,
			March = 3,
			April = 4,
			May = 5,
			June = 6,
			July = 7,
			August = 8,
			September = 9,
			October = 10,
			November = 11,
			December = 12
		}

		public enum MonthAbbreviation 
		{
			None = 0,
			Jan = 1,
			Feb = 2,
			Mar = 3,
			Apr = 4,
			May = 5,
			Jun = 6,
			Jul = 7,
			Aug = 8,
			Sep = 9,
			Oct = 10,
			Nov = 11,
			Dec = 12
		}

		public Month month;

		public int day;
		public string postfix;

		public int year;

		public CalendarDate(DateTime time) 
		{
			month = (Month)time.Month;
			day = time.Day;
			year = time.Year;
		}

		public void PrintDate()
		{
			ConsoleLog.Log(ToString());
		}

		public DateTime AsDateTime() 
		{
			DateTime date = new(year, (int)month, day);
			return date;
		}

        public override string ToString() => $"{month} {day}, {year}";

    }
}
