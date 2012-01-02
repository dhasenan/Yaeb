using System.Reflection;

namespace Events
{
	public interface IEventBroker
	{
		void AddSubscriber (string topic, object o, MethodInfo method);
		
		void AddPublisher (string[] topics, object o, EventInfo evt);
		
		void Register (object o);
		
		void Fire (string name);
	}
}

