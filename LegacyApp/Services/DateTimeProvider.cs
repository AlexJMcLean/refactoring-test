using System;
namespace LegacyApp.Services;

public class DateTimeProvider : IDateTimeProvider
{
	public DateTimeProvider()
	{
	}
	public DateTime DateTimeNow()
	{
		return DateTime.Now;
	}
}

