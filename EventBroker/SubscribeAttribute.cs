using System;

namespace Events
{
	public class SubscribeAttribute : Attribute
	{
		public readonly string Topic;
		
		public SubscribeAttribute (string topic)
		{
			Topic = topic;
		}
	}
}

