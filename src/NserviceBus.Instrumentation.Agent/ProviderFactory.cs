using Castle.MicroKernel;
using NserviceBus.Instrumentation.Interfaces;

namespace NserviceBus.Instrumentation.Agent
{
	public class ProviderFactory : IProviderFactory
	{
		public IKernel Kernel { get; set; }

		public IInstrumentationProvider Construct(string typeName)
		{
			var key = typeName + "InstrumentationProvider";

			return Kernel.Resolve<IInstrumentationProvider>(key);
		}
	}

	public interface IProviderFactory
	{
		IInstrumentationProvider Construct(string typeName);
	}
}
