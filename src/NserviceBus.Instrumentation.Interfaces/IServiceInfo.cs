using System.Diagnostics;

namespace NServiceBus.Instrumentation.Interfaces
{	
	public interface IServiceInfo
	{
		string Name { get; set; }
		string FolderPath { get; set; }
		string ConfigPath { get; set; }
		string ErrorQueue { get; set; }
		string AuditQueue { get; set; }
	}
}