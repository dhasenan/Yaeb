using System;
using Castle.MicroKernel;
using Castle.Core.Configuration;
using System.Collections.Generic;
using System.Reflection;

namespace Events
{
	public class EventBrokerFacility : IFacility
	{
		public struct PubStub
		{
			public EventInfo Event;
			public string[] Topics;
		}

		public struct SubStub
		{
			public MethodInfo Method;
			public string Topic;
		}
		
		private IEventBroker _eventBroker;
		
		public EventBrokerFacility ()
		{
		}
		
		public void Init (IKernel kernel, IConfiguration cfg)
		{
			kernel.ComponentRegistered += ComponentRegistered;
			kernel.ComponentCreated += ComponentCreated;
			_eventBroker = kernel.Resolve<IEventBroker> ();
		}

		private void ComponentCreated (Castle.Core.ComponentModel model, object instance)
		{
			if (!model.ExtendedProperties.Contains (typeof(EventBrokerFacility)))
			{
				return;
			}
			var dict = (Dictionary<string, object>) model.ExtendedProperties [typeof(EventBrokerFacility)];
			var pubs = (List<PubStub>) dict ["pub"];
			var subs = (List<SubStub>) dict ["sub"];
			foreach (var pub in pubs)
			{
				_eventBroker.AddPublisher (pub.Topics, instance, pub.Event);
			}
			foreach (var sub in subs)
			{
				_eventBroker.AddSubscriber (sub.Topic, instance, sub.Method);
			}
		}

		private void ComponentRegistered (string key, IHandler handler)
		{
			var model = handler.ComponentModel;
			var type = handler.ComponentModel.Implementation;
			List<PubStub > events = new List<PubStub> ();
			List<SubStub > subscribers = new List<SubStub> ();
			foreach (var evt in type.GetEvents(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public))
			{
				var attrs = evt.GetCustomAttributes (typeof(PublishAttribute), true);
				if (attrs.Length == 0)
				{
					continue;
				}
				
				var bits = new string[attrs.Length];
				int i = 0;
				foreach (PublishAttribute attr in attrs)
				{
					bits[i] = attr.Topic;
				}
				events.Add (new PubStub {Topics = bits, Event = evt});
			}
			foreach (var method in type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public))
			{
				foreach (SubscribeAttribute attr in method.GetCustomAttributes (typeof(SubscribeAttribute), true))
				{
					subscribers.Add (new SubStub{Method = method, Topic = attr.Topic});
				}
			}
			if (events.Count > 0 || subscribers.Count > 0)
			{
				var dict = new Dictionary<string, object> ();
				dict ["pub"] = events;
				dict ["sub"] = subscribers;
				model.ExtendedProperties.Add (typeof(EventBrokerFacility), dict);
			}
		}
		
		public void Terminate ()
		{
		}
	}
}

