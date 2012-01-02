using System;
using System.Collections.Generic;
using System.Reflection;

namespace Events
{
	public class EventBroker : IEventBroker
	{
		private struct Subscription
		{
			public MethodInfo Method;
			public WeakReference Target;
		}
		
		private class Publication
		{
			public string[] Topics;
			public EventBroker Broker;

			public void Fire ()
			{
				foreach (var topic in Topics)
				{
					Console.WriteLine ("firing topic {0}", topic);
					Broker.Fire (topic);
				}
			}
		}
		
		private Dictionary<string, List<Subscription>> _subscriptions = new Dictionary<string, List<Subscription>> ();
		private readonly MethodInfo _pubmethod;
		
		public EventBroker ()
		{
			_pubmethod = typeof(Publication).GetMethod ("Fire");
		}
		
		public void AddSubscriber (string topic, object o, MethodInfo method)
		{
			if (o == null)
			{
				throw new ArgumentNullException ("o");
			}
			if (method == null)
			{
				throw new ArgumentNullException ("method");
			}
			List<Subscription > subs;
			if (!_subscriptions.TryGetValue (topic, out subs))
			{
				subs = _subscriptions [topic] = new List<Subscription> ();
			}
			Console.WriteLine ("added subscription for {0}, method {1} on type {2}", topic, method, o.GetType ());
			subs.Add (new Subscription{Method = method, Target = new WeakReference (o)});
		}
		
		public void AddPublisher (string[] topics, object o, EventInfo evt)
		{
			if (o == null)
			{
				throw new ArgumentNullException ("o");
			}
			if (evt == null)
			{
				throw new ArgumentNullException ("evt");
			}
			var pub = new Publication {Topics = topics, Broker = this};
			var dg = Delegate.CreateDelegate (typeof(Action), pub, _pubmethod);
			evt.AddEventHandler (o, dg);
			var str = "";
			foreach (var s in topics)
			{
				str += s;
				str += ",";
			}
			Console.WriteLine ("added publishers for [{0}], event {1} on type {2}", str, evt, o.GetType ());
		}
		
		public void Register (object o)
		{
			if (o == null)
			{
				return;
			}
			var type = o.GetType ();
			foreach (var method in type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public))
			{
				var subscribings = method.GetCustomAttributes (typeof(SubscribeAttribute), true);
				foreach (SubscribeAttribute sub in subscribings)
				{
					AddSubscriber (sub.Topic, o, method);
				}
			}
			foreach (var evt in type.GetEvents(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public))
			{
				var publishings = evt.GetCustomAttributes (typeof(PublishAttribute), true);
				if (publishings.Length > 0)
				{
					var p = new string[publishings.Length];
					int i = 0;
					foreach (PublishAttribute attr in publishings)
					{
						p [i] = attr.Topic;
						i++;
					}
					AddPublisher(p, o, evt);
				}
			}
		}
		
		public void Fire (string name)
		{
			List<Subscription > subs;
			if (_subscriptions.TryGetValue (name, out subs))
			{
				Console.WriteLine ("found {0} subscribers", subs.Count);
				foreach (var sub in subs)
				{
					var target = sub.Target.Target;
					if (target != null)
					{
						Console.WriteLine ("target: {0} method target type: {1}", target, sub.Method.DeclaringType);
						sub.Method.Invoke (target, new object[0]);
					}
					else
					{
						Console.WriteLine ("sorry, the princess is in another castle!");
					}
				}
			}
		}
	}
}

