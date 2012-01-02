using System;

namespace Events
{
	public class PublishAttribute : Attribute
	{
		public readonly string Topic;
		
		public PublishAttribute (string topic)
		{
			Topic = topic;
		}
	}
}

