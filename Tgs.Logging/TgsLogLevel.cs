namespace Tgs.Logging
{
	public enum TgsLogLevel
	{
		All = int.MinValue,
		Verbose = 10000,
		Debug = 30000,
		Info = 40000,
		Warning = 60000,
		Error = 70000,
		Fatal = 110000
	}
}
