using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NServiceBus.Instrumentation.Interfaces
{
	public interface IInstrumentationProvider
	{
		void Instrument();
		void Setup(IEnumerable<string> serviceNames);
	}
}
