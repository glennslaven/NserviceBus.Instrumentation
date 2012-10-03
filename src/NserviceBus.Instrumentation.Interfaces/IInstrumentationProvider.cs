using System.Collections.Generic;

namespace NServiceBus.Instrumentation.Interfaces
{
	public interface IInstrumentationProvider
	{
		void Instrument();
		void Setup(IEnumerable<IServiceInfo> services);
	}
}
